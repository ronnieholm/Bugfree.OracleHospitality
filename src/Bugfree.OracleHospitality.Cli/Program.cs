using System;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommandLine;
using Bugfree.OracleHospitality.Clients.PosMessageSequencingStrategies;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using Bugfree.OracleHospitality.Clients;

namespace Bugfree.OracleHospitality.Cli;

[Verb("point-issue", HelpText = "GL POS POINT_ISSUE operation")]
public class PointIssueOptions
{
    [Option("account-number", Required = true, HelpText = "Account number in GL account store")]
    public string AccountNumber { get; set; }
}

[Verb("coupon-issue", HelpText = "GL POS SV_ISSUE_COUPON operation")]
public class CouponIssueOptions
{
    [Option("account-number", Required = true, HelpText = "Account number in GL account store")]
    public string AccountNumber { get; set; }

    [Option("coupon-code", Required = true, HelpText = "Coupon code (serial number) in GL coupon store")]
    public string CouponCode { get; set; }
}

[Verb("coupon-inquiry", HelpText = "GL POS COUPON_INQUIRY operation")]
public class CouponInquiryOptions
{
    [Option("account-number", Required = true, HelpText = "Account number in GL account store")]
    public string AccountNumber { get; set; }
}

[Verb("coupon-accept", HelpText = "GL POS SV_ACCEPT_COUPON operation")]
public class CouponAcceptOptions
{
    [Option("account-number", Required = true, HelpText = "Account number in GL account store")]
    public string AccountNumber { get; set; }

    [Option("coupon-code", Required = true, HelpText = "Coupon code (serial number) in GL coupon store")]
    public string CouponCode { get; set; }
}

[Verb("set-customer", HelpText = "GL CRM SetCustomer operation")]
public class SetCustomerOptions
{
    [Option("row-id", Required = false, HelpText = "Row Id must be non-null when associating customer to existing account")]
    public int? RowId { get; set; }

    [Option("column-values", Required = true, HelpText = "Columns/values to set")]
    public IEnumerable<string> ColumnValues { get; set; }
}

[Verb("post-account-transaction", HelpText = "GL CRM PostAccountTransaction operation")]
public class PostAccountTransactionOptions
{
    [Option("type", Required = true, HelpText = "Only CloseAccount or ReopenAccount supported")]
    public Transaction.Type.Kind Type { get; set; }

    [Option("program-code", Required = true, HelpText = "Program code in GL program store")]
    public string ProgramCode { get; set; }

    [Option("account-pos-ref", Required = true, HelpText = "Account number in GL account store")]
    public string AccountPosRef { get; set; }
}

[Verb("get-column-list", HelpText = "GL GRM GetColumnList operation")]
public class GetColumnListOptions
{
    [Option("request", Required = true, HelpText = "Entity to lookup such as 'customer', 'account', or 'program'")]
    public string Request { get; set; }
}

[Verb("get-customer", HelpText = "GL CRM GetCustomer operation")]
public class GetCustomerOptions
{
    [Option("conditions", Required = true, HelpText = "Where clause such as 'primaryposref = ?'")]
    public string Conditions { get; set; }

    [Option("column-values", Required = true, HelpText = "Keys/values matching 'conditions' option")]
    public IEnumerable<string> ColumnValues { get; set; }

    [Option("columns", Required = true, HelpText = "Output columns")]
    public IEnumerable<string> Columns { get; set; }
}

[Verb("get-account", HelpText = "GL CRM GetAccount operation")]
public class GetAccountOptions
{
    [Option("conditions", Required = true, HelpText = "Where clause such as 'accountposref = ?'")]
    public string Conditions { get; set; }

    [Option("column-values", Required = true, HelpText = "Keys/values matching 'conditions' option")]
    public IEnumerable<string> ColumnValues { get; set; }

    [Option("columns", Required = true, HelpText = "Output columns")]
    public IEnumerable<string> Columns { get; set; }
}

[Verb("get-program", HelpText = "GL CRM GetProgram operation")]
public class GetProgramOptions
{
    [Option("conditions", Required = true, HelpText = "Where clause such as 'programid = ?'")]
    public string Conditions { get; set; }

    [Option("column-values", Required = true, HelpText = "Keys/values matching 'conditions' option")]
    public IEnumerable<string> ColumnValues { get; set; }

    [Option("columns", Required = true, HelpText = "Output columns")]
    public IEnumerable<string> Columns { get; set; }
}

[Verb("get-coupons", HelpText = "GL CRM GetCoupons operation")]
public class GetCouponsOptions
{
    [Option("conditions", Required = true, HelpText = "Where clause such as 'primaryposref = ?'")]
    public string Conditions { get; set; }

    [Option("column-values", Required = true, HelpText = "Keys/values matching 'conditions' option")]
    public IEnumerable<string> ColumnValues { get; set; }
}

[Verb("guid-to-account-number", HelpText = "Converts Guid to GL account number")]
public class GuidToAccountNumberOptions
{
    [Option("guid", Required = true, HelpText = "Guid to convert")]
    public Guid Guid { get; set; }
}

public static class Program
{
    public static IServiceCollection AddFileConfiguration(this IServiceCollection services)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        services.Configure<OracleHospitalityClientsOptions>(builder.GetSection("OracleHospitalityClients"));

        var clientOptions = services.BuildServiceProvider().GetRequiredService<IOptions<OracleHospitalityClientsOptions>>();
        clientOptions.Value.Validate();
        return services;
    }

    public static IServiceCollection AddHttpClientsConfiguration(this IServiceCollection services)
    {
        // Protects against ephemeral port exhaustion per
        // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests.
        // This not only registers an HttpClient for use with
        // OracleHospitalityExecutor, but adds the executor to the container
        // with transient scope.
        services.AddHttpClient<IOracleHospitalityExecutor, OracleHospitalityExecutor>()
            .ConfigureHttpClient(_ =>
            {
                // Potentially define a retry strategy using Polly
                // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
            });

        return services;
    }

    private static IServiceProvider ConfigureServices(IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddFileConfiguration()
            .AddSingleton<IPosMessageSequencingStrategy, TerminalIdOnlyStrategy>()
            .AddScoped<IPosClient, PosClient>()
            .AddScoped<ICrmClient, CrmClient>()
            .AddScoped<Application, Application>()

            // HttpClient configuration must follow above addition of
            // dependent types to container or an exception is thrown:
            // System.InvalidOperationException: 'Unable to resolve service
            // for type 'System.Net.Http.HttpClient' while attempting to
            // activate' [...] PosClient'.'
            .AddHttpClientsConfiguration()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
            })
            .BuildServiceProvider();
    }

    public static async Task Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = ConfigureServices(serviceCollection);
        var application = serviceProvider.GetRequiredService<Application>();

        await Parser.Default.ParseArguments<
                PointIssueOptions,
                CouponIssueOptions,
                CouponInquiryOptions,
                CouponAcceptOptions,
                SetCustomerOptions,
                PostAccountTransactionOptions,
                GetColumnListOptions,
                GetCustomerOptions,
                GetAccountOptions,
                GetProgramOptions,
                GetCouponsOptions,
                GuidToAccountNumberOptions>(args)
            .MapResult(
                (PointIssueOptions o) => application.RunAsync(o),
                (CouponIssueOptions o) => application.RunAsync(o),
                (CouponInquiryOptions o) => application.RunAsync(o),
                (CouponAcceptOptions o) => application.RunAsync(o),
                (SetCustomerOptions o) => application.RunAsync(o),
                (PostAccountTransactionOptions o) => application.RunAsync(o),
                (GetColumnListOptions o) => application.RunAsync(o),
                (GetCustomerOptions o) => application.RunAsync(o),
                (GetAccountOptions o) => application.RunAsync(o),
                (GetProgramOptions o) => application.RunAsync(o),
                (GetCouponsOptions o) => application.RunAsync(o),
                (GuidToAccountNumberOptions o) => application.RunAsync(o),
                _ => Task.CompletedTask);

        await ((ServiceProvider)serviceProvider).DisposeAsync();
    }
}
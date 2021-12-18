using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bugfree.OracleHospitality.Clients.IntegrationTests;

public static class ConfigurationHelpers
{
    public static IOptions<OracleHospitalityClientsOptions> ParseConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var section = builder.GetSection("OracleHospitalityClients");
        var options = new OracleHospitalityClientsOptions
        {
            StoredValueServiceUrl = new Uri(section["StoredValueServiceUrl"]),
            LogonName = section["LogonName"],
            Password = section["Password"],
            PointOfSaleOperations = new PointOfSaleOperations
            {
                TerminalIdLowerBound = int.Parse(section.GetSection("PointOfSaleOperations")["TerminalIdLowerBound"]),
                TerminalIdUpperBound = int.Parse(section.GetSection("PointOfSaleOperations")["TerminalIdUpperBound"]),
            },
            CustomerRelationshipManagementOperations = new CustomerRelationshipManagementOperations
            {
                RequestSourceName = section.GetSection("CustomerRelationshipManagementOperations")["RequestSourceName"]
            }
        };
        options.Validate();
        return Options.Create(options);
    }
}
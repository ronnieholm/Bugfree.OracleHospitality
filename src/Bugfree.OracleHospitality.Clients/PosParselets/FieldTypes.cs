using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bugfree.OracleHospitality.Clients.PosParselets;

static class FieldTypes
{
    // Create fixed regular expression patterns (for hardcoded sizes) rather
    // than dynamically create regular expression because matching is done a
    // lot and we don't need the flexibility.
    //
    // When we specify xN, then N is the maximum number of characters. We
    // don't assert exactly N because most of the time 1 <= actual <= N. If
    // a specific length is required, a parslet can explicitly assert its
    // length.
    //
    // UNDOCUMENTED: what encompasses an alphanumeric character except the
    // obvious digits and alphanumeric characters is undefined. "-" is
    // inferred from currency example of "en-US" (A5), "." from
    // PosInterfaceVesion "1.00" and HostVersion "9.1.0000.2301" examples.
    // "_" from RequestCode example of "COUPON_INQUIRY". " ", "(", and ")"
    // from DisplayMessage, "&" and ";" from PrintLine formatting examples,
    // '%' from "Employee Discount 20%" program name.
    private const string APattern = @"^[a-zA-Z0-9\-_\.,\(\);&% ]";
    private const string A1Pattern = APattern + "$";
    private const string A3Pattern = APattern + "{1,3}$";
    private const string A5Pattern = APattern + "{1,5}$";
    private const string A8Pattern = APattern + "{1,8}$";
    private const string A16Pattern = APattern + "{1,16}$";
    private const string A24Pattern = APattern + "{1,24}$";
    private const string A25Pattern = APattern + "{1,25}$";
    private const string A32Pattern = APattern + "{1,32}$";
    private const string A100Pattern = APattern + "{1,100}$";
    private const string A128Pattern = APattern + "{1,128}$";
    private const string NPattern = "^[0-9]";
    private const string N2Pattern = NPattern + "{1,2}$";
    private const string N4Pattern = NPattern + "{1,4}$";
    private const string N9Pattern = NPattern + "{1,9}$";
    private const string DatePattern = @"^(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})$";
    private const string TimePattern = @"^(?<hour>\d{2})(?<minute>\d{2})(?<second>\d{2})$";

    // Decimal pattern of "[-][1-9][0-9]*[.][0-9]*" in POS API spec, Page
    // 17, is incorrect as it makes leading zeros not permitted. A leading
    // zero is allowed and sometimes even required. For instance,
    // COUPON_INQUIRY requests must contain an Amount XML element or the
    // request fails. When we don't have a reasonable amount to specify, we
    // must specify 0.00.
    private const string DecimalPattern = @"^(?<sign>(-{0,1}))(?<characteristic>([0-9]|[1-9][0-9]*))($|(?<comma>(\.))(?<mantissa>([0-9]+)))$";

    private static readonly Regex A1 = new(A1Pattern, RegexOptions.Compiled);
    private static readonly Regex A3 = new(A3Pattern, RegexOptions.Compiled);
    private static readonly Regex A5 = new(A5Pattern, RegexOptions.Compiled);
    private static readonly Regex A8 = new(A8Pattern, RegexOptions.Compiled);
    private static readonly Regex A16 = new(A16Pattern, RegexOptions.Compiled);
    private static readonly Regex A24 = new(A24Pattern, RegexOptions.Compiled);
    private static readonly Regex A25 = new(A25Pattern, RegexOptions.Compiled);
    private static readonly Regex A32 = new(A32Pattern, RegexOptions.Compiled);
    private static readonly Regex A100 = new(A100Pattern, RegexOptions.Compiled);
    private static readonly Regex A128 = new(A128Pattern, RegexOptions.Compiled);
    private static readonly Regex N2 = new(N2Pattern, RegexOptions.Compiled);
    private static readonly Regex N4 = new(N4Pattern, RegexOptions.Compiled);
    private static readonly Regex N9 = new(N9Pattern, RegexOptions.Compiled);

    private static readonly Regex Decimal = new(DecimalPattern, RegexOptions.Compiled);
    private static readonly Regex Date = new(DatePattern, RegexOptions.Compiled);
    private static readonly Regex Time = new(TimePattern, RegexOptions.Compiled);

    private static void AssertString(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Must not be null or whitespace");
    }

    public static void AssertA1(string s)
    {
        AssertString(s);
        if (!A1.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A1Pattern}'");
    }

    public static string AssertA3(string s)
    {
        AssertString(s);
        if (!A3.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A3Pattern}'");
        return s;
    }

    public static string AssertA5(string s)
    {
        AssertString(s);
        if (!A5.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A5Pattern}'");
        return s;
    }

    public static string AssertA8(string s)
    {
        AssertString(s);
        if (!A8.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A8Pattern}'");
        return s;
    }

    public static string AssertA16(string s)
    {
        AssertString(s);
        if (!A16.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A16Pattern}'");
        return s;
    }

    public static string AssertA24(string s)
    {
        AssertString(s);
        if (!A24.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A24Pattern}'");
        return s;
    }

    public static string AssertA25(string s)
    {
        AssertString(s);
        if (!A25.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A25Pattern}'");
        return s;
    }

    public static string AssertA32(string s)
    {
        AssertString(s);
        if (!A32.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A32Pattern}'");
        return s;
    }

    public static string AssertA100(string s)
    {
        AssertString(s);
        if (!A100.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A100Pattern}'");
        return s;
    }

    public static string AssertA128(string s)
    {
        AssertString(s);
        if (!A128.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{A128Pattern}'");
        return s;
    }

    public static int AssertN2(string s)
    {
        AssertString(s);
        if (!N2.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{N2Pattern}'");
        return int.Parse(s);
    }

    public static int AssertN4(string s)
    {
        AssertString(s);
        if (!N4.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{N4Pattern}'");
        return int.Parse(s);
    }

    public static int AssertN9(string s)
    {
        AssertString(s);
        if (!N9.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{N9Pattern}'");
        return int.Parse(s);
    }

    public static decimal AssertDecimal(string s)
    {
        AssertString(s);
        if (!Decimal.IsMatch(s))
            throw new ArgumentException($"'{s}' should match '{DecimalPattern}'");
        return decimal.Parse(s, CultureInfo.GetCultureInfo("en-US"));
    }

    public static DateTime AssertDate(string s)
    {
        AssertString(s);
        var m = Date.Match(s);
        if (!m.Success)
            throw new ArgumentException($"'{s}' should match '{DatePattern}'");

        var year = int.Parse(m.Groups["year"].Value);
        var month = int.Parse(m.Groups["month"].Value);
        if (month < 1 || month > 12)
            throw new ArgumentException($"Month must be in range 01 to 12. Was {month}");

        var day = int.Parse(m.Groups["day"].Value);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        if (day < 1 || day > daysInMonth)
            throw new ArgumentException($"Day must in range 1 to {daysInMonth} for year {year} and month {month}");

        return new DateTime(year, month, day);
    }

    public static TimeSpan AssertTime(string s)
    {
        AssertString(s);
        var m = Time.Match(s);
        if (!m.Success)
            throw new ArgumentException($"'{s}' should match '{TimePattern}'");

        var hour = int.Parse(m.Groups["hour"].Value);
        if (hour > 23)
            throw new ArgumentException($"Hour must be in range 00 to 23. Was {hour}");

        var minute = int.Parse(m.Groups["minute"].Value);
        if (minute > 59)
            throw new ArgumentException($"Minute must be in range 00 to 59. Was {minute}");

        var second = int.Parse(m.Groups["second"].Value);
        if (second > 59)
            throw new ArgumentException($"Second must be in range 00 to 59. Was {second}");

        return new TimeSpan(hour, minute, second);
    }
}
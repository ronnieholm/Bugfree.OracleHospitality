namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    static class Constants
    {
        // From Oracle's XML elements and attribute, we retain their casing.
        // Elements start with an uppercase, attributes start with a lowercase.
        // Using original case makes it easier to spot wrong usage, such as
        // using element names where attributes are expected.

        // Attributes in alphabetic order
        public const string tid = nameof(tid);
        public const string currency = nameof(currency);
        public const string hostCode = nameof(hostCode);
        public const string hostVersion = nameof(hostVersion);
        public const string language = nameof(language);
        public const string pid = nameof(pid);
        public const string posIntfcName = nameof(posIntfcName);
        public const string posIntfcVersion = nameof(posIntfcVersion);
        public const string retransmit = nameof(retransmit);
        public const string sequence = nameof(sequence);
        public const string version = nameof(version);

        // Elements in alphabetic order
        public const string AccountBalance = nameof(AccountBalance);
        public const string AccountCurrency = nameof(AccountCurrency);
        public const string Actions = nameof(Actions);
        public const string Action = nameof(Action);
        public const string Amount = nameof(Amount);
        public const string BonusPointsIssued = nameof(BonusPointsIssued);
        public const string BusinessDate = nameof(BusinessDate);
        public const string CheckNumber = nameof(CheckNumber);
        public const string Code = nameof(Code);
        public const string CouponCode = nameof(CouponCode);
        public const string Data = nameof(Data);
        public const string DisplayMessage = nameof(DisplayMessage);
        public const string ExchangeRate = nameof(ExchangeRate);
        public const string ItemNumber = nameof(ItemNumber);
        public const string ItemType = nameof(ItemType);
        public const string LocalBalance = nameof(LocalBalance);
        public const string LocalCurrency = nameof(LocalCurrency);
        public const string LocalDate = nameof(LocalDate);
        public const string LocalTime = nameof(LocalTime);
        public const string PointsIssued = nameof(PointsIssued);
        public const string PrintLine = nameof(PrintLine);
        public const string PrintLines = nameof(PrintLines);
        public const string ProgramCode = nameof(ProgramCode);
        public const string ProgramName = nameof(ProgramName);
        public const string RequestCode = nameof(RequestCode);
        public const string ResponseCode = nameof(ResponseCode);
        public const string RevenueCenter = nameof(RevenueCenter);
        public const string SVAN = nameof(SVAN);
        public const string SVCMessage = nameof(SVCMessage);
        public const string TerminalID = nameof(TerminalID);
        public const string TerminalType = nameof(TerminalType);
        public const string Text = nameof(Text);
        public const string TraceID = nameof(TraceID);
        public const string TransactionEmployee = nameof(TransactionEmployee);
        public const string Type = nameof(Type);
    }
}
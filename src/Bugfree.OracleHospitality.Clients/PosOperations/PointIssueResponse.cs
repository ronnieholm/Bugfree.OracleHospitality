using System;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;
using Bugfree.OracleHospitality.Clients.PosParselets;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.PosOperations;

public class PointIssueResponse : PosResponse
{
    public ItemType ItemType { get; private set; }
    public ItemNumber ItemNumber { get; private set; }
    public Currency AccountCurrency { get; private set; }
    public Balance AccountBalance { get; private set; }
    public Balance LocalBalance { get; private set; }
    public ExchangeRate ExchangeRate { get; private set; }
    public ProgramCode ProgramCode { get; private set; }
    public ProgramName ProgramName { get; private set; }
    public Points PointsIssued { get; private set; }
    public Points BonusPointsIssued { get; private set; }
    public PrintLines PrintLines { get; private set; }

    public PointIssueResponse(XE request, XE response)
        : base(request, response)
    {
    }

    public override void DeconstructResponse()
    {
        if (ExceptionToRaiseAfterParsing != null)
            return;

        var elementMappings = new (string, Action<XE>)[]
        {
            (C.ItemType, x => ItemType = new ItemType(x.Value)),
            (C.ItemNumber, x => ItemNumber = new ItemNumber(x.Value)),
            (C.AccountCurrency, x => AccountCurrency = new Currency(x.Value)),
            (C.AccountBalance, x => AccountBalance = new Balance(x.Value)),
            (C.LocalBalance, x => LocalBalance = new Balance(x.Value)),
            (C.ExchangeRate, x => ExchangeRate = new ExchangeRate(x.Value)),
            (C.ProgramCode, x => ProgramCode = new ProgramCode(x.Value)),
            (C.ProgramName, x => ProgramName = new ProgramName(x.Value)),
            (C.PointsIssued, x => PointsIssued = new Points(x.Value)),
            (C.BonusPointsIssued, x => BonusPointsIssued = new Points(x.Value))
        };
        foreach (var (elementName, creatorFn) in elementMappings)
        {
            MapElement(Response_, elementName, creatorFn);
            ConsumeElement(UnconsumedResponse, elementName);
        }

        // UNDOCUMENTED: when under Oracle Reporting & Analytics web portal
        // -> Gift & Loyalty -> G&L Setup -> Programs, Cards, Coupons and
        // Rules -> Loyalty Rules -> <program> -> Edit -> Awards -> POS
        // Print Text field, its value in non-empty, then response contains
        // a PrintLines element.
        var printLinesElement = Response_.Element(C.PrintLines);
        if (printLinesElement != null)
        {
            PrintLines = new PrintLines(printLinesElement);
            ConsumeElement(UnconsumedResponse, C.PrintLines);
        }
    }
}
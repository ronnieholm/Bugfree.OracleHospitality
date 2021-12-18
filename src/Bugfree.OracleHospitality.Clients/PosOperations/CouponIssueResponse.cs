using System;
using Bugfree.OracleHospitality.Clients.PosParselets;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.PosOperations;

public class CouponIssueResponse : PosResponse
{
    public CouponCode CouponCode { get; private set; }
    public ItemType ItemType { get; private set; }
    public ItemNumber ItemNumber { get; private set; }
    public PrintLines PrintLines { get; private set; }
    public Currency AccountCurrency { get; private set; }
    public ExchangeRate ExchangeRate { get; private set; }

    public CouponIssueResponse(XE request, XE response)
        : base(request, response)
    {
    }

    public override void DeconstructResponse()
    {
        if (ExceptionToRaiseAfterParsing != null)
        {
            var elementMappingsInner = new (string, Action<XE>)[]
            {
                (C.ItemType, x => ItemType = new ItemType(x.Value)),
                (C.ItemNumber, x => ItemNumber = new ItemNumber(x.Value)),
                (C.AccountCurrency, x => AccountCurrency = new Currency(x.Value)),
                (C.ExchangeRate, x => ExchangeRate = new ExchangeRate(x.Value))
            };
            foreach (var (elementName, creatorFn) in elementMappingsInner)
            {
                MapElement(Response_, elementName, creatorFn);
                ConsumeElement(UnconsumedResponse, elementName);
            }

            return;
        }

        var elementMappings = new (string, Action<XE>)[]
        {
            (C.CouponCode, x => CouponCode = new CouponCode(x.Value)),
            (C.ItemType, x => ItemType = new ItemType(x.Value)),
            (C.ItemNumber, x => ItemNumber = new ItemNumber(x.Value)),
            (C.PrintLines, x => PrintLines = new PrintLines(x)),
            (C.AccountCurrency, x => AccountCurrency = new Currency(x.Value)),
            (C.ExchangeRate, x => ExchangeRate = new ExchangeRate(x.Value))
        };
        foreach (var (elementName, creatorFn) in elementMappings)
        {
            MapElement(Response_, elementName, creatorFn);
            ConsumeElement(UnconsumedResponse, elementName);
        }
    }
}
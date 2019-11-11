using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bugfree.OracleHospitality.Clients.PosParselets;
using Action = Bugfree.OracleHospitality.Clients.PosParselets.Action;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.PosOperations
{
    public class CouponInqueryResponse : PosResponse
    {
        public ReadOnlyCollection<Action> Actions { get; private set; }
        public Currency AccountCurrency { get; private set; }
        public ExchangeRate ExchangeRate { get; private set; }
        public PrintLines PrintLines { get; private set; }

        public CouponInqueryResponse(XE request, XE response)
            : base(request, response)
        {
        }

        public override void DeconstructResponse()
        {
            // If no coupon is available, response contains
            //
            // <DisplayMessage>There are no eligible coupons for this card.</DisplayMessage>
            //
            // If coupons are available, Actions element contains at least one 
            // Action element:
            // 
            // <Actions>
            //   <Action>
            //     <Type tid="12">Accept Coupon</Type>
            //     <Data pid="9">1004019</Data>
            //     <Code>10DKK</Code>
            //     <Text>Coupon: 10 DKK, Always Valid</Text>
            //   </Action>
            // </Actions>
            // <DisplayMessage>There is an eligible coupon for this card.</DisplayMessage>
            //
            // For more than one eligible coupons, DisplayMessage changes to 
            // include a count: "There are 2 eligible coupons for this card."
            if (ExceptionToRaiseAfterParsing != null)
                return;

            var elementMappings = new (string, Action<XE>)[]
            {
                (C.AccountCurrency, x => AccountCurrency = new Currency(x.Value)),
                (C.ExchangeRate, x => ExchangeRate = new ExchangeRate(x.Value))
            };
            foreach (var (elementName, creatorFn) in elementMappings)
            {
                MapElement(Response_, elementName, creatorFn);
                ConsumeElement(UnconsumedResponse, elementName);
            }

            // In POS API spec, Page 17, Field Type Definitions, Actions element
            // has type XML, meaning it must be valid XML. That's already the
            // case from converting the Soap reply into an XElement. POS API
            // spec, Page 26 specifies Actions element structure in more detail.
            var actionsElement = Response_.Element(C.Actions);
            if (actionsElement != null)
            {
                Actions = new Actions(actionsElement).Values;
                ConsumeElement(UnconsumedResponse, C.Actions);

                var profilesElement = Response_.Element(C.PrintLines);
                if (profilesElement == null)
                    throw new ArgumentException($"Expected '{C.PrintLines}' element to always be present when '{C.Actions}' element is");
                PrintLines = new PrintLines(profilesElement);
                ConsumeElement(UnconsumedResponse, C.PrintLines);
            }
            else
                Actions = new List<Action>().AsReadOnly();
        }
    }
}
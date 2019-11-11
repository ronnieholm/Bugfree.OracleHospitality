using System;
using Bugfree.OracleHospitality.Clients.PosParselets;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.PosOperations
{
    public class CouponAcceptResponse : PosResponse
    {
        public ItemType ItemType { get; private set; }
        public ItemNumber ItemNumber { get; private set; }

        public CouponAcceptResponse(XE request, XE response)
            : base(request, response)
        {
        }

        public override void AssertSvanElement()
        {
            // AccountNumber element isn't always present in response.
            // SV_ACCEPT_COUPON differs in that its request and response both
            // has a SVAN, but only upon failure does response hold the actual
            // SVAN from the request. Upon success, while the element is named
            // SVAN, it's no longer SVAN but CouponCode (serial number)
            // accepted.
            var requestCouponCodeElement = ExpectElement(Request, C.CouponCode);
            var requestSvanElement = ExpectElement(Request, C.SVAN);
            var responseSvanElement = ExpectElement(Response_, C.SVAN);

            if (ExceptionToRaiseAfterParsing == null)
            {
                if (new CouponCode(requestCouponCodeElement.Value).ToString() != new AccountNumber(responseSvanElement.Value).ToString())
                    throw new OracleHospitalityClientException($"Expected element values to be equal. Was '{requestCouponCodeElement.Value}' and '{responseSvanElement.Value}'");
            }
            else
            {
                if (new AccountNumber(requestSvanElement.Value).ToString() != new AccountNumber(responseSvanElement.Value).ToString())
                    throw new OracleHospitalityClientException($"Expected element values to be equal. Was '{requestSvanElement.Value}' and '{responseSvanElement.Value}'");
            }
        }

        public override void DeconstructResponse()
        {
            if (ExceptionToRaiseAfterParsing != null)
                return;

            var elementMappings = new (string, Action<XE>)[]
            {
                (C.ItemType, x => ItemType = new ItemType(x.Value)),
                (C.ItemNumber, x => ItemNumber = new ItemNumber(x.Value))
            };
            foreach (var (elementName, creatorFn) in elementMappings)
            {
                MapElement(Response_, elementName, creatorFn);
                ConsumeElement(UnconsumedResponse, elementName);
            }
        }
    }
}
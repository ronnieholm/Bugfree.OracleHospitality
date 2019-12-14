using System;
using System.Xml.Linq;
using Xunit;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using Bugfree.OracleHospitality.Clients.UnitTests.Seedwork;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;

namespace Bugfree.OracleHospitality.UnitTests.CrmParselets
{
    public class TransactionTests
    {
        [Fact]
        public void valid_transaction_element()
        {
            const string expected = @"
                <Transaction>
                    <Type>5</Type>
                    <CustFriendlyDesc>Updated by service</CustFriendlyDesc>
                    <ProgramCode>EMPDISC</ProgramCode>
                    <AccountPOSRef>2200005</AccountPOSRef>
                    <TransDateTime>2019-10-07 13:38:20.1</TransDateTime>
                    <BusinessDate>2019-10-07</BusinessDate>
                    <CardPresent>0</CardPresent>
                    <LocalCurrencyISOCode>DKK</LocalCurrencyISOCode>
                    <TraceID>39WKty782jxDpEVg8glTuH</TraceID>
                </Transaction>";

            var now = DateTime.Parse("2019-10-07T13:38:20.1");
            using (new TimeProviderTestScope(() => now))
            {
                var transaction =
                    new CloseReopenTransactionBuilder()
                        .WithType(Transaction.Type.Kind.CloseAccount)
                        .WithCustomerFriendlyDescription("Updated by service")
                        .WithProgramCode("EMPDISC")
                        .WithAccountPosRef("2200005")
                        .WithTransactionDateTime(now)
                        .WithBusinessDate(now)
                        .WithCardPresent(false)
                        .WithCurrency(Currency.Kind.DKK)
                        .WithTraceId(new Guid("8802d123-fb71-4e3b-a940-0abc769fd767"))
                        .Build();

                Assert.Equal(XElement.Parse(expected).ToString(), transaction.ToString());
            }
        }
    }
}
using System;
using Bugfree.OracleHospitality.Clients.CrmParselets;
using static Bugfree.OracleHospitality.Clients.CrmParselets.Transaction;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients.CrmOperations
{
    // This multi-purpose operation has the ability to create a loyalty account
    // also. We could use it in place of the SV_POINT_ISSUE operation, but then
    // we'd have to infer program code, length of preamble, and other
    // information that SV_POINT_ISSUE looks up by account number. On the
    // upside, PostAccountTransaction doesn't require correct POS setup,
    // (probably) doesn't leave a trace in the account's transaction log, and
    // (probably) isn't subject to the 9,999 checknumber limit without grace
    // period that inhibits SV_POINT_ISSUE.
    //
    // Within its Transaction element, we see hints of POS API activity under
    // the covers. TraceID for instance is a POS API concept, and so is
    // BusinessDate and ProgramCode. We don't reuse POS parselets here, but
    // define CRM ones as we can't be sure logic of this operation follows that
    // of POS API elements simply because names of element overlap. With CRM
    // API, for instance, documentation for TraceID characterizes it as "[a]n
    // arbitrary trace number, used to specifically identify a message." whereas
    // the POS API equivalent mandates a strict format that perhaps isn't
    // enforced.

    public class PostAccountTransactionRequest : CrmRequest
    {
        public TransactionBuilder Builder { get; }

        public PostAccountTransactionRequest(string requestSourceName, TransactionBuilder builder)
            : base(requestSourceName)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public override XE BuildRequestDocument()
        {
            var request = BuildBaseDocument(RequestCode.Kind.PostAccountTransaction);
            request.Add(Builder.Build());
            return request;
        }
    }
}
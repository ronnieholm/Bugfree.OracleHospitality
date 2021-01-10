using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    // Parses <Type tid="12">Accept Coupon</Type>
    public class ActionType : IResponseElement
    {
        // We don't expose the "Accept Coupon" value because it maps one-to-one
        // to TransactionId. We do, however, assert that it has the correct
        // value.
        public TransactionId Id { get; }

        // According to POS API spec, Page 28.
        public enum TransactionId
        {
            None,
            AcceptCoupon = 12
        }

        readonly Dictionary<TransactionId, string> TidToString = new Dictionary<TransactionId, string>
        {
            { TransactionId.AcceptCoupon, "Accept Coupon" }
        };

        public ActionType(XE actionType)
        {
            if (actionType == null)
                throw new ArgumentNullException(nameof(actionType));

            var tidAttribute = actionType.Attribute(C.tid);
            var tidAttributeValue = int.Parse(tidAttribute.Value);
            if (!Enum.IsDefined(typeof(TransactionId), tidAttributeValue))
                throw new ArgumentException($"Unknown {nameof(C.tid)} '{tidAttributeValue}'");
            Id = (TransactionId)int.Parse(tidAttribute.Value);

            var success = TidToString.TryGetValue(Id, out var value);
            if (!success)
                throw new ArgumentException($"{nameof(TidToString)} mapping doesn't contain {nameof(TransactionId)} with value {tidAttribute.Value}");
            if (value != actionType.Value)
                throw new ArgumentException($"Expected Action Type element to have value '{value}'. Was '{actionType.Value}'");

            // UNDOCUMENTED: type or length of Action/Type content. We can only
            // assume an Ax of sufficient length to represent every
            // TransactionId in textual form.
            FieldTypes.AssertA25(actionType.Value);
        }
    }

    public class ActionDataValue
    {
        public string Value { get; }

        public ActionDataValue(string value)
        {
            // UNDOCUMENTED: POS API spec, Page 28 states that Data element's
            // content is limited to 80 characters, but not which characters.
            // From the table on Page 28, '/' isn't part of the existing Ax set
            // and it's unclear whether it's correct to add it so we check
            // length only.
            const int DataElementValueMaxLength = 80;
            if (value.Length > DataElementValueMaxLength)
                throw new ArgumentException($"Expected Data element value to be less than {DataElementValueMaxLength} characters");
            Value = value;
        }
    }

    // Parses <Data pid="9">1004019</Data>
    public class ActionData : IResponseElement
    {
        public PromptId Id { get; }
        public ActionDataValue Value { get; }

        // According to POS API spec, Page 28
        public enum PromptId
        {
            None,
            PleaseEnterCoupon = 9
        }

        public ActionData(XE actionData)
        {
            var pidAttribute = actionData.Attribute(C.pid);
            var pidAttributeValue = int.Parse(pidAttribute.Value);
            if (!Enum.IsDefined(typeof(PromptId), pidAttributeValue))
                throw new ArgumentException($"Unknown {nameof(C.pid)} '{pidAttributeValue}'");
            Id = (PromptId)pidAttributeValue;
            Value = new ActionDataValue(actionData.Value);
        }
    }

    // Parses <Code>10DKK</Code>
    public class ActionCode : IResponseElement
    {
        public string Value { get; }

        public ActionCode(XE actionCode)
        {
            // UNDOCUMENTED: here Code is short for coupon code, and so we
            // assume its format is what ISSUE_COUPON operation uses for
            // <CouponCode>.
            if (string.IsNullOrWhiteSpace(actionCode.Value))
                throw new ArgumentException($"{nameof(C.Action)}/{nameof(C.Code)} value must not be null or whitespace");
            Value = actionCode.Value;
        }
    }

    public class ActionTextValue
    {
        public string Value { get; }

        public ActionTextValue(string value)
        {
            // UNDOCUMENTED: POS API spec, Page 28, states that text content is
            // limited to 80 characters but not which characters of
            // FieldTypes.Ax. Content is input by a human configuring the coupon
            // and real world usage includes ",", ":", and " ".
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{nameof(C.Text)} must contain non-whitespace characters");

            const int MinValue = 1;
            const int MaxValue = 80;
            if (value.Length < MinValue || value.Length > MaxValue)
                throw new ArgumentException($"{nameof(C.Text)} content not expected to be in range [{MinValue},{MaxValue}] characters");
            Value = value;
        }
    }

    // Parses <Text>Coupon: 10 DKK, Always Valid</Text>
    public class ActionText : IResponseElement
    {
        public ActionTextValue Value { get; }

        public ActionText(XE actionText)
        {
            Value = new ActionTextValue(actionText.Value);
        }
    }

    public class Action : IResponseElement
    {
        public ActionType Type { get; }
        public ActionData Data { get; }
        public ActionCode Code { get; }
        public ActionText Text { get; }

        public Action(XE action)
        {

            var typeElement = ExpectElement(action, C.Type);
            Type = new ActionType(typeElement);
            var dataElement = ExpectElement(action, C.Data);
            Data = new ActionData(dataElement);

            // UNDOCUMENTED: POS API spec, Page 27, states that each <Action>
            // element must contain <Type>, <Data>, and <Text> elements. It's
            // unclear why <Code>, short for coupon code, is left out. It's part
            // of the response of the COUPON_INQUIRY operation. Perhaps it isn't
            // always part of the response of other operations or perhaps
            // documentation is out of date.
            var codeElement = ExpectElement(action, C.Code);
            Code = new ActionCode(codeElement);
            var textElement = ExpectElement(action, C.Text);
            Text = new ActionText(textElement);
        }
    }

    public class Actions : IResponseElement
    {
        /* Parses input of the form:

           <Actions>
               <Action>
                   <Type tid=""12"">Accept Coupon</Type>
                   <Data pid=""9"">1004019</Data>
                   <Code>10DKK</Code>
                   <Text>Coupon: 10 DKK, Always Valid</Text>
               </Action>
           </Actions>
        */

        public ReadOnlyCollection<Action> Values { get; }

        public Actions(XE actions)
        {
            var actionElements = ExpectElements(actions, C.Action);
            Values = Array.AsReadOnly(actionElements.Select(ae => new Action(ae)).ToArray());
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using C = Bugfree.OracleHospitality.Clients.CrmParselets.Constants;
using XE = System.Xml.Linq.XElement;
using static Bugfree.OracleHospitality.Clients.ParserHelpers;

namespace Bugfree.OracleHospitality.Clients.CrmParselets
{
    public class QueryCriteria : IRequestElement
    {
        public class Condition : IRequestElement
        {
            public class ConditionName : IRequestAttribute
            {
                public string Value { get; }
                public ConditionName(string value) => Value = FieldTypes.AssertString(value);
                public override string ToString() => Value;
            }

            public class Value : IRequestAttribute
            {
                public string Value_ { get; }
                public Value(string value) => Value_ = FieldTypes.AssertString(value);
                public override string ToString() => Value_;
            }

            public ConditionName Name { get; }
            public Value Value_ { get; }

            public Condition(XE condition)
            {
                var nameAttribute = ExpectAttribute(condition, C.name);
                Name = new ConditionName(nameAttribute.Value);
                var valueAttribute = ExpectAttribute(condition, C.value);
                Value_ = new Value(valueAttribute.Value);
            }
        }

        public class Conditions : IRequestAttribute
        {
            public string Value { get; }
            public Conditions(string value) => Value = FieldTypes.AssertString(value);
            public override string ToString() => Value;
        }

        public Conditions Conditions_ { get; }
        public ReadOnlyCollection<Condition> ConditionDetails { get; }

        // Parses
        //
        // <QueryCriteria conditions="primaryPOSRef = ?">
        //   <Condition name="primaryPOSRef" value="2200005" />
        // </QueryCriteria>
        public QueryCriteria(XE queryCriteria)
        {
            var conditionsAttribute = ExpectAttribute(queryCriteria, C.conditions);
            Conditions_ = new Conditions(conditionsAttribute.Value);

            var conditionElements = ExpectElements(queryCriteria, C.Condition);
            ConditionDetails = Array.AsReadOnly(conditionElements.Select(ce => new Condition(ce)).ToArray());
        }
    }
}
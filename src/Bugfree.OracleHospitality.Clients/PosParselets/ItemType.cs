using System;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class ItemType : IResponseElement
    {
        public enum Kind
        {
            None,
            MenuOrSalesItem,
            Discount,
            ServiceCharge,
            Tender
        }

        public Kind Value { get; }

        public ItemType(string value)
        {
            // UNDOCUMENTED: according to POS API spec, Page 21, ItemType is of
            // type XML with no field type definition. We've yet to encounter
            // any XML beneath the ItemType element. In the responses we've
            // encountered, the format is <ItemType>T</ItemType>.
            FieldTypes.AssertA1(value);
            Value = value switch
            {
                "M" => Kind.MenuOrSalesItem,
                "D" => Kind.Discount,
                "S" => Kind.ServiceCharge,
                "T" => Kind.Tender,
                _ => throw new ArgumentException($"Unsupported {nameof(Kind)} value: '{value}'"),
            };
        }

        public ItemType(Kind kind)
        {
            if (kind == Kind.None)
                throw new ArgumentException($"{nameof(Kind)} must not be {kind}");
            Value = kind;
        }

        public override string ToString()
        {
            return Value switch
            {
                Kind.MenuOrSalesItem => "M",
                Kind.Discount => "D",
                Kind.ServiceCharge => "S",
                Kind.Tender => "T",
                _ => throw new ArgumentException($"Unsupported {nameof(Kind)}: {Value}"),
            };
        }
    }
}
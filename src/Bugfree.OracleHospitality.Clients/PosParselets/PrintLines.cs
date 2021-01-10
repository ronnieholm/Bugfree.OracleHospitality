using System;
using System.Linq;
using System.Collections.ObjectModel;
using XE = System.Xml.Linq.XElement;
using C = Bugfree.OracleHospitality.Clients.PosParselets.Constants;

namespace Bugfree.OracleHospitality.Clients.PosParselets
{
    public class PrintLine : IResponseElement
    {
        public string Value { get; }
        public PrintLine(string value) => Value = FieldTypes.AssertA128(value);
        public override string ToString() => Value;
    }

    public class PrintLines : IResponseElement
    {
        public ReadOnlyCollection<PrintLine> Values { get; }

        // UNDOCUMENTED: an empty element such as <PrintLines /> or, presumably, 
        // a list of elements like this:
        //
        // <PrintLines>
        //   <PrintLine>Foo</PrintLine>
        //   <PrintLine>Bar</PrintLine>
        // </PrintLines>
        //
        // The PrintLines element is undocumented and from the example in POS 
        // API spec, Page 12, it appears multiple <PrintLine> may follow each 
        // other without the PrintLines parent element. That isn't the case, 
        // and documentation is likely out of date.
        public PrintLines(XE printLines)
        {
            var printLineElements = printLines.Elements(C.PrintLine).ToArray();
            Values =
                printLineElements.Length == 0
                ? new ReadOnlyCollection<PrintLine>(new PrintLine[] { })
                : Array.AsReadOnly(printLineElements.Select(pl => new PrintLine(pl.Value)).ToArray());
        }
    }
}
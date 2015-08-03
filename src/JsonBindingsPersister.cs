using AlfredTrx.Helpers;

namespace BroccoliTaskRunner
{
    public class JsonBindingsPersister : IPersistTaskRunnerBindings
    {
        public string FormatBindingsXmlForPersistence(string bindingsXml)
        {
            return "///" + bindingsXml;
        }

        public bool TryGetBindingsFromLine(string line, out LineRange rangeInLine)
        {
            int hashIndex = line.IndexOf("///");

            if (hashIndex < 0)
            {
                rangeInLine = new LineRange();
                return false;
            }

            int openIndex = line.IndexOf("<binding", hashIndex);

            if (openIndex < 0)
            {
                rangeInLine = new LineRange();
                return false;
            }

            hashIndex = line.LastIndexOf("///", openIndex);

            if (hashIndex < 0)
            {
                rangeInLine = new LineRange();
                return false;
            }

            int closeIndex = line.IndexOf("/>", openIndex);

            if (closeIndex < 0)
            {
                rangeInLine = new LineRange();
                return false;
            }

            //The tag can be closed tightly or have a space after the element name <binding/> or <binding ... />
            if (closeIndex != openIndex + 8 && !char.IsWhiteSpace(line[openIndex + 8]))
            {
                rangeInLine = new LineRange();
                return false;
            }

            rangeInLine = new LineRange { Start = hashIndex, Length = closeIndex + 2 - hashIndex };
            return true;
        }

        public string UnformatBindingsXmlForPersistence(string line, LineRange rangeInLine)
        {
            string interestingPart = line.Substring(rangeInLine.Start, rangeInLine.Length);
            int bindingStart = interestingPart.IndexOf("<binding");
            return interestingPart.Substring(bindingStart, rangeInLine.Length - bindingStart);
        }

    }
}

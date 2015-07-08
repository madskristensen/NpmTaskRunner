using System.Linq;
using AlfredTrx.Helpers;
using Newtonsoft.Json.Linq;

namespace NpmTaskRunner
{
    public class JsonBindingsPersister : IPersistTaskRunnerBindings
    {
        public string FormatBindingsXmlForPersistence(string bindingsXml)
        {
            return "  \"-vs-bindings\": " + "\"" + bindingsXml + "\",";
        }

        public bool TryGetBindingsFromLine(string line, out LineRange rangeInLine)
        {
            rangeInLine = new LineRange { Start = 0, Length = line.Length };

            if (string.IsNullOrEmpty(line) || line.Length < 20)
                return false;

            try
            {
                JObject root = JObject.Parse("{" + line.TrimEnd(',') + "}");

                var element = root.Children<JProperty>().First();

                if (element.Name != "-vs-bindings")
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string UnformatBindingsXmlForPersistence(string line, LineRange rangeInLine)
        {
            string interestingPart = line.Substring(rangeInLine.Start, rangeInLine.Length);
            int bindingStart = interestingPart.IndexOf("<binding");
            return interestingPart.Substring(bindingStart, rangeInLine.Length - bindingStart).TrimEnd('"', ',');
        }
    }
}

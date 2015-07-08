using Microsoft.VisualStudio.TextManager.Interop;

namespace AlfredTrx.Helpers
{
    public static class PersistTaskRunnerBindingsExtensions
    {
        public static string Load(this IPersistTaskRunnerBindings persister, string configPath)
        {
            IVsTextView configTextView = TextViewUtil.FindTextViewFor(configPath);
            ITextUtil textUtil;

            if (configTextView != null)
            {
                textUtil = new VsTextViewTextUtil(configTextView);
            }
            else
            {
                textUtil = new FileTextUtil(configPath);
            }

            string line;
            while (textUtil.TryReadLine(out line))
            {
                LineRange range;
                if (persister.TryGetBindingsFromLine(line, out range))
                {
                    string bindingPart = line.Substring(range.Start, range.Length);
                    return persister.UnformatBindingsXmlForPersistence(line, range);
                }
            }

            return "<binding />";
        }

        public static bool Save(this IPersistTaskRunnerBindings persister, string configPath, string bindingsXml)
        {
            string bindingsText = persister.FormatBindingsXmlForPersistence(bindingsXml);
            IVsTextView configTextView = TextViewUtil.FindTextViewFor(configPath);
            ITextUtil textUtil;

            if (configTextView != null)
            {
                textUtil = new VsTextViewTextUtil(configTextView);
            }
            else
            {
                textUtil = new FileTextUtil(configPath);
            }

            string line;
            int lineNumber = 0;
            while (textUtil.TryReadLine(out line))
            {
                LineRange range;
                if (persister.TryGetBindingsFromLine(line, out range))
                {
                    return textUtil.Replace(new Range { LineNumber = lineNumber, LineRange = range }, bindingsText);
                }
                ++lineNumber;
            }

            return textUtil.Insert(new Range { LineNumber = 1, LineRange = new LineRange { Start = 0, Length = bindingsXml.Length } }, bindingsText, true);
        }
    }
}
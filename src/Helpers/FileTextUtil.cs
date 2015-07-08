using System;
using System.IO;

namespace AlfredTrx.Helpers
{
    internal class FileTextUtil : ITextUtil
    {
        private int _currentLineLength;
        private string _filename;
        private int _lineNumber;

        public FileTextUtil(string filename)
        {
            _filename = filename;
        }

        public Range CurrentLineRange
        {
            get { return new Range { LineNumber = _lineNumber, LineRange = new LineRange { Start = 0, Length = _currentLineLength } }; }
        }

        public bool Delete(Range range)
        {
            if (range.LineRange.Length == 0)
            {
                return true;
            }

            bool success = true;
            string fileContents = File.ReadAllText(_filename);

            using (StringReader reader = new StringReader(fileContents))
            using (TextWriter writer = new StreamWriter(File.Open(_filename, FileMode.Create)))
            {
                string lineText;
                if (SeekTo(reader, writer, range, out lineText))
                {
                    writer.WriteLine(lineText.Substring(0, range.LineRange.Start) + lineText.Substring(range.LineRange.Start + range.LineRange.Length));
                }

                lineText = reader.ReadLine();

                while (lineText != null)
                {
                    writer.WriteLine(lineText);
                    lineText = reader.ReadLine();
                }
            }

            return success;
        }

        public bool Insert(Range range, string text, bool addNewline)
        {
            if (text.Length == 0)
            {
                return true;
            }

            bool success = true;
            string fileContents = File.ReadAllText(_filename);

            using (StringReader reader = new StringReader(fileContents))
            using (TextWriter writer = new StreamWriter(File.Open(_filename, FileMode.Create)))
            {
                string lineText;
                if (SeekTo(reader, writer, range, out lineText))
                {
                    writer.WriteLine(lineText.Substring(0, range.LineRange.Start) + text + (addNewline ? Environment.NewLine : string.Empty) + lineText.Substring(range.LineRange.Start));
                }

                lineText = reader.ReadLine();

                while (lineText != null)
                {
                    writer.WriteLine(lineText);
                    lineText = reader.ReadLine();
                }
            }

            return success;
        }

        public bool TryReadLine(out string line)
        {
            line = null;
            Stream stream = File.OpenRead(_filename);
            using (TextReader reader = new StreamReader(stream))
            {
                int lineCount = _lineNumber;
                for (int i = 0; i < lineCount + 1; ++i)
                {
                    line = reader.ReadLine();
                }

                if (line != null)
                {
                    _currentLineLength = line.Length;
                    ++_lineNumber;
                    return true;
                }

                _currentLineLength = 0;
                return false;
            }
        }

        private bool SeekTo(StringReader reader, TextWriter writer, Range range, out string lineText)
        {
            bool success = true;

            for (int lineNumber = 0; lineNumber < range.LineNumber; ++lineNumber)
            {
                string line = reader.ReadLine();

                if (line != null)
                {
                    writer.WriteLine(line);
                }
                else
                {
                    success = false;
                    break;
                }
            }

            lineText = reader.ReadLine();

            if (success)
            {
                if (lineText != null)
                {
                    if (lineText.Length < range.LineRange.Start)
                    {
                        success = false;
                        writer.WriteLine(lineText);
                    }
                }
            }

            return success;
        }
    }
}

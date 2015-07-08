using System;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace AlfredTrx.Helpers
{
    internal class VsTextViewTextUtil : ITextUtil
    {
        private int _currentLineLength;
        private int _lineNumber;
        private IVsTextView _view;

        public VsTextViewTextUtil(IVsTextView view)
        {
            _view = view;
        }

        public Range CurrentLineRange
        {
            get { return new Range { LineNumber = _lineNumber, LineRange = new LineRange { Start = 0, Length = _currentLineLength } }; }
        }

        public bool Delete(Range range)
        {
            try
            {
                GetEditPointForRange(range)?.Delete(range.LineRange.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Insert(Range position, string text, bool addNewline)
        {
            try
            {
                GetEditPointForRange(position)?.Insert(text + (addNewline ? Environment.NewLine : string.Empty));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryReadLine(out string line)
        {
            IVsTextLines textLines;
            int hr = _view.GetBuffer(out textLines);

            if (hr != VSConstants.S_OK || textLines == null)
            {
                line = null;
                return false;
            }

            int lineNumber = _lineNumber++;
            hr = textLines.GetLengthOfLine(lineNumber, out _currentLineLength);

            if(hr != VSConstants.S_OK)
            {
                line = null;
                return false;
            }

            hr = textLines.GetLineText(lineNumber, 0, lineNumber, _currentLineLength, out line);

            if (hr != VSConstants.S_OK)
            {
                line = null;
                return false;
            }

            return true;
        }

        private EditPoint GetEditPointForRange(Range range)
        {
            IVsTextLines textLines;
            int hr = _view.GetBuffer(out textLines);

            if (hr != VSConstants.S_OK || textLines == null)
            {
                return null;
            }

            object editPointObject;
            hr = textLines.CreateEditPoint(range.LineNumber, range.LineRange.Start, out editPointObject);
            EditPoint editPoint = editPointObject as EditPoint;

            if (hr != VSConstants.S_OK || editPoint == null)
            {
                return null;
            }

            return editPoint;
        }
    }
}

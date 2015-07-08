namespace AlfredTrx.Helpers
{
    public interface ITextUtil
    {
        Range CurrentLineRange { get; }

        bool Delete(Range range);

        bool Insert(Range position, string text, bool addNewline);

        bool TryReadLine(out string line);
    }
}

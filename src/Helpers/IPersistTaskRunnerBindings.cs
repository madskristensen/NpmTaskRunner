namespace AlfredTrx.Helpers
{
    public interface IPersistTaskRunnerBindings
    {
        string FormatBindingsXmlForPersistence(string bindingsXml);

        bool TryGetBindingsFromLine(string line, out LineRange rangeInLine);

        string UnformatBindingsXmlForPersistence(string line, LineRange rangeInLine);
    }
}

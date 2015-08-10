using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.JSON.Core.Parser;
using Microsoft.JSON.Core.Parser.TreeItems;
using Microsoft.JSON.Editor.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace NpmTaskRunner
{
    [Export(typeof(IJSONCompletionListProvider))]
    [Name("NpmTaskCompletionProvider")]
    internal class TaskCompletionProvider : IJSONCompletionListProvider
    {
        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public JSONCompletionContextType ContextType
        {
            get { return JSONCompletionContextType.ArrayElement; }
        }

        public IEnumerable<JSONCompletionEntry> GetListEntries(JSONCompletionContext context)
        {
            ITextDocument document;
            if (TextDocumentFactoryService.TryGetTextDocument(context.Snapshot.TextBuffer, out document))
            {
                string fileName = Path.GetFileName(document.FilePath).ToLowerInvariant();

                if (string.IsNullOrEmpty(fileName) || !fileName.Equals(Constants.FILENAME, StringComparison.OrdinalIgnoreCase))
                    yield break;
            }
            else
            {
                yield break;
            }

            JSONMember member = context.ContextItem.FindType<JSONMember>();

            while (member != null && member.Parent != null && member.UnquotedNameText != Constants.ELEMENT_NAME)
            {
                member = member.Parent.FindType<JSONMember>();
            }

            if (member == null || member.UnquotedNameText != Constants.ELEMENT_NAME)
                yield break;

            foreach (var task in GetTasks(context.ContextItem))
            {
                yield return new SimpleCompletionEntry(task.Item1, task.Item2, StandardGlyphGroup.GlyphGroupEvent, context.Session);
            }
        }

        public static IEnumerable<Tuple<string, string>> GetTasks(JSONParseItem item)
        {
            var visitor = new JSONItemCollector<JSONMember>(true);
            item.JSONDocument.Accept(visitor);

            var scripts = visitor.Items.FirstOrDefault(member => member.UnquotedNameText == "scripts");

            if (scripts == null)
                yield break;

            foreach (JSONObject child in scripts.Children.Where(c => c is JSONObject))
                foreach (JSONMember taskItem in child.Children.Where(c => c is JSONMember))
                {
                    yield return Tuple.Create(taskItem.UnquotedNameText, taskItem.Value.Text);
                }
        }
    }
}
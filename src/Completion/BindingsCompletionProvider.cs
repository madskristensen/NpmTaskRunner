using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.JSON.Core.Parser.TreeItems;
using Microsoft.JSON.Editor.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace NpmTaskRunner
{
    [Export(typeof(IJSONCompletionListProvider))]
    [Name("NpmBindingsCompletionProvider")]
    internal class BindingsCompletionProvider : IJSONCompletionListProvider
    {
        private static StandardGlyphGroup _glyph =  StandardGlyphGroup.GlyphArrow;

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public JSONCompletionContextType ContextType
        {
            get { return JSONCompletionContextType.PropertyName; }
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

            JSONMember member = context.ContextItem.Parent?.FindType<JSONMember>();

            if (member == null || member.UnquotedNameText != Constants.ELEMENT_NAME)
                yield break;

            yield return new SimpleCompletionEntry("AfterBuild", "Fires after the MSBuild process ended.", _glyph, context.Session);
            yield return new SimpleCompletionEntry("BeforeBuild", "Fires before the MSBuild process ended.", _glyph, context.Session);
            yield return new SimpleCompletionEntry("Clean", "Fires after the MSBuild 'Clean' process ended.", _glyph, context.Session);
            yield return new SimpleCompletionEntry("ProjectOpen", "Fires when the project is opened in Visual Studio.", _glyph, context.Session);
        }
    }
}
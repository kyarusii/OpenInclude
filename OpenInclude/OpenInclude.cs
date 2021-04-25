using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using EnvDTE;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OpenInclude.Utility;
using Task = System.Threading.Tasks.Task;

namespace OpenInclude
{
    internal static class FileAndContentTypeDefinitions
    {
        [Export] [Name("shader")] [BaseDefinition("text")]
        internal static ContentTypeDefinition shaderContentTypeDefinition;

        [Export] [Name(".shader")] [ContentType("shader")]
        internal static FileExtensionToContentTypeDefinition shaderFileExtensionDefinition;

        [Export]
        [Name(".hlsl")]
        [ContentType("shader")]
        internal static FileExtensionToContentTypeDefinition HLSLFileExtensionDefinition;

        [Export]
        [Name(".glsl")]
        [ContentType("shader")]
        internal static FileExtensionToContentTypeDefinition GLSLFileExtensionDefinition;

        [Export]
        [Name(".cginc")]
        [ContentType("shader")]
        internal static FileExtensionToContentTypeDefinition CGINCLUDEFileExtensionDefinition;
    }

    internal abstract class OpenIncludeSuggestedActionSourceProviderBase : ISuggestedActionsSourceProvider
    {
        [Import(typeof(ITextStructureNavigatorSelectorService))]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        {
            if (textBuffer == null || textView == null)
            {
                return null;
            }

            return new OpenIncludeSuggestedActionSource(this, textView, textBuffer);
        }
    }

    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("Open Include Text Suggested Action ")]
    [ContentType("text")]
    internal class OpenIncludeSuggestedActionSourceProviderText : OpenIncludeSuggestedActionSourceProviderBase
    {

    }

    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("Open Include Text Suggested Action ")]
    [ContentType("shader")]
    internal class OpenIncludeSuggestedActionSourceProviderShader : OpenIncludeSuggestedActionSourceProviderBase
    {

    }

    internal class OpenDocumentAction : ISuggestedAction
    {
        private readonly ITrackingSpan m_span;
        private readonly string m_display;
        private readonly ITextSnapshot m_snapshot;

        public OpenDocumentAction(ITrackingSpan span)
        {
            m_span = span;
            m_snapshot = span.TextBuffer.CurrentSnapshot;

            m_display = string.Format("Open '{0}'", GetExportedPath(GetTrimText(span)));
        }

        private string GetTrimText(ITrackingSpan span)
        {
            string text = span.GetText(m_snapshot);
            return text.Replace(" ", "");
        }

        private string GetExportedPath(string trimmedText)
        {
            var text = trimmedText.Replace("#include", "").Replace("\"", "").Replace(" ", "");
            return text;
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            string docName = dte.ActiveDocument.FullName;

            var includeLine = GetExportedPath(GetTrimText(m_span));

            var currentDocument1 = new UnityPath(docName);
            var includeDocument = new UnityInclude(includeLine);

            var resolved = currentDocument1.GetResolvedPath(includeDocument.PackageName) + "/" + includeDocument.SubDirectory;

            var sr = new StreamReader(resolved);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// {resolved}\n");
            for (int i = 0; i < 5; ++i)
            {
                sb.AppendLine(sr.ReadLine());
            }
            sb.AppendLine("...");

            var textBlock = new TextBlock();
            textBlock.Padding = new Thickness(5);

            textBlock.Inlines.Add(new Run()
            {
                Text = $"{sb.ToString()}"
            });

            return Task.FromResult<object>(textBlock);
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
        }

        public bool HasActionSets
        {
            get { return false; }
        }
        public string DisplayText
        {
            get { return m_display; }
        }
        public ImageMoniker IconMoniker
        {
            get { return default(ImageMoniker); }
        }
        public string IconAutomationText
        {
            get
            {
                return null;
            }
        }
        public string InputGestureText
        {
            get
            {
                return null;
            }
        }
        public bool HasPreview
        {
            get { return true; }
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
                var path = dte.ActiveDocument.FullName;

                var includeLine = GetExportedPath(GetTrimText(m_span));

                var currentDocument1 = new UnityPath(path);
                var includeDocument = new UnityInclude(includeLine);

                var resolved = currentDocument1.GetResolvedPath(includeDocument.PackageName) + "/" + includeDocument.SubDirectory;
                dte.ItemOperations.OpenFile(resolved);
            });
        }

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample action and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }
}

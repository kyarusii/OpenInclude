using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace OpenInclude
{
    internal class OpenIncludeSuggestedActionSource : ISuggestedActionsSource
    {
        private readonly OpenIncludeSuggestedActionSourceProviderBase m_provider;
        private readonly ITextView m_textView;
        private readonly ITextBuffer m_textBuffer;
        
        public OpenIncludeSuggestedActionSource(OpenIncludeSuggestedActionSourceProviderBase provider, ITextView textView,
            ITextBuffer textBuffer)
        {
            this.m_provider = provider;
            this.m_textView = textView;
            this.m_textBuffer = textBuffer;
        }

        private bool TryGetWordUnderCaret(out TextExtent wordExtent)
        {
            ITextCaret caret = m_textView.Caret;
            SnapshotPoint point;

            if (caret.Position.BufferPosition > 0)
            {
                point = caret.Position.BufferPosition - 1;
            }
            else
            {
                wordExtent = default(TextExtent);
                return false;
            }

            ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_textBuffer);

            wordExtent = navigator.GetExtentOfWord(point);
            return true;
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                TextExtent extent;
                if (TryGetWordUnderCaret(out extent))
                {
                    // don't display the action if the extent has whitespace
                    return extent.IsSignificant;
                }
                return false;
            });
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            TextExtent extent;
            if (TryGetWordUnderCaret(out extent) && extent.IsSignificant)
            {
                ITextSnapshotLine line = range.Snapshot.GetLineFromPosition(range.Start.Position);

                var lineSpan = range.Snapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeInclusive);

                if (!lineSpan.GetText(lineSpan.TextBuffer.CurrentSnapshot).Contains("#include"))
                {
                    return Enumerable.Empty<SuggestedActionSet>();
                }

                var openAction = new OpenDocumentAction(lineSpan);

                return new SuggestedActionSet[] { new SuggestedActionSet(new ISuggestedAction[] { openAction }) };
            }
            return Enumerable.Empty<SuggestedActionSet>();
        }

        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample provider and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
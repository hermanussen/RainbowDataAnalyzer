namespace RainbowDataAnalyzer.Vsix
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    internal class RainbowQuickInfoSource : IQuickInfoSource
    {
        private readonly RainbowQuickInfoSourceProvider provider;
        private readonly ITextBuffer subjectBuffer;
        private bool isDisposed;

        public RainbowQuickInfoSource(RainbowQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            this.provider = provider;
            this.subjectBuffer = subjectBuffer;
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan)
        {
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(this.subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                applicableToSpan = null;
                return;
            }

            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            //look for occurrences of our QuickInfo words in the span
            ITextStructureNavigator navigator = this.provider.NavigatorService.GetTextStructureNavigator(this.subjectBuffer);
            var extent = navigator.GetSpanOfEnclosing(new SnapshotSpan(this.subjectBuffer.CurrentSnapshot, querySpan));
            string searchText = extent.GetText()?.Trim('"');

            Debug.WriteLine("searchText=" + searchText);

            Guid parsedId;
            if (Guid.TryParse(searchText, out parsedId))
            {
                var file = Rainbow.Repository.rainbowFiles.Values.FirstOrDefault(f => parsedId.Equals(f.Id));
                if(file != null)
                {
                    applicableToSpan = currentSnapshot.CreateTrackingSpan
                        (
                            extent.Span.Start, file.Id.ToString().Length, SpanTrackingMode.EdgeInclusive
                        );

                    qiContent.Add($"Sitecore path: {file.Path}");
                    return;
                }
            }
            else
            {
                foreach (var file in Rainbow.Repository.rainbowFiles.Values.OrderByDescending(f => f.Path))
                {
                    int foundIndex = searchText.IndexOf(file.Path, StringComparison.CurrentCultureIgnoreCase);
                    if (foundIndex > -1)
                    {
                        applicableToSpan = currentSnapshot.CreateTrackingSpan
                            (
                                extent.Span.Start + foundIndex, file.Id.ToString().Length, SpanTrackingMode.EdgeInclusive
                            );

                        qiContent.Add($"Sitecore ID: {{{file.Id}}}");
                        return;
                    }
                }
            }

            applicableToSpan = null;
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                GC.SuppressFinalize(this);
                this.isDisposed = true;
            }
        }
    }
}

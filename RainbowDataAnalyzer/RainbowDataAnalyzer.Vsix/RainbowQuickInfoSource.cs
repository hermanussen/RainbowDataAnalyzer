namespace RainbowDataAnalyzer.Vsix
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Operations;

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

            Guid parsedId;
            var allFiles = Rainbow.Repository.rainbowFiles.Values;
            if (Guid.TryParse(searchText, out parsedId))
            {
                var file = allFiles.FirstOrDefault(f => f != null && parsedId.Equals(f.Id));
                if(file != null)
                {
                    applicableToSpan = currentSnapshot.CreateTrackingSpan
                        (
                            extent.Span.Start, file.Id.ToString().Length, SpanTrackingMode.EdgeInclusive
                        );

                    try
                    {
                        qiContent.Add($"Sitecore path: {file.Path}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        // The collection may be modified, so we can't add anything here right now
                        Debug.Write($"Exception occurred when trying to add path {file.Path} (skipping as it is not essential): {ex.Message}");
                    }

                    return;
                }
            }
            else
            {
                foreach (var file in allFiles.Where(f => f != null && f.Id != Guid.Empty && f.Path != null).OrderByDescending(f => f.Path))
                {
                    int foundIndex = searchText.IndexOf(file.Path, StringComparison.CurrentCultureIgnoreCase);
                    if (foundIndex > -1)
                    {
                        applicableToSpan = currentSnapshot.CreateTrackingSpan
                            (
                                extent.Span.Start + foundIndex, file.Id.ToString().Length, SpanTrackingMode.EdgeInclusive
                            );

                        try
                        {
                            qiContent.Add($"Sitecore ID: {{{file.Id}}}");
                        }
                        catch (InvalidOperationException ex)
                        {
                            // The collection may be modified, so we can't add anything here right now
                            Debug.Write($"Exception occurred when trying to add ID {file.Id} (skipping as it is not essential): {ex.Message}");
                        }

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

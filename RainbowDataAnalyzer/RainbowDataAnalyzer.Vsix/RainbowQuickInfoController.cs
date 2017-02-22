namespace RainbowDataAnalyzer.Vsix
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;

    internal class RainbowQuickInfoController : IIntellisenseController
    {
        private ITextView textView;
        private readonly IList<ITextBuffer> subjectBuffers;
        private readonly RainbowQuickInfoControllerProvider provider;
        private IQuickInfoSession session;

        internal RainbowQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, RainbowQuickInfoControllerProvider provider)
        {
            this.textView = textView;
            this.subjectBuffers = subjectBuffers;
            this.provider = provider;

            this.textView.MouseHover += this.OnTextViewMouseHover;
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //find the mouse position by mapping down to the subject buffer
            SnapshotPoint? point = this.textView.BufferGraph.MapDownToFirstMatch
                 (new SnapshotPoint(this.textView.TextSnapshot, e.Position),
                PointTrackingMode.Positive,
                snapshot => this.subjectBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                PointTrackingMode.Positive);

                if (!this.provider.QuickInfoBroker.IsQuickInfoActive(this.textView))
                {
                    this.session = this.provider.QuickInfoBroker.TriggerQuickInfo(this.textView, triggerPoint, true);
                }
            }
        }

        public void Detach(ITextView textView)
        {
            if (this.textView == textView)
            {
                this.textView.MouseHover -= this.OnTextViewMouseHover;
                this.textView = null;
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}

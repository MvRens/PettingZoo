using System;

namespace PettingZoo.Core.ExportImport.Subscriber
{
    public abstract class BaseProgressDecorator
    {
        private static readonly TimeSpan DefaultReportInterval = TimeSpan.FromMilliseconds(100);

        private readonly IProgress<int> progress;
        private readonly long reportInterval;
        private long lastReport;

        protected BaseProgressDecorator(IProgress<int> progress, TimeSpan? reportInterval = null)
        {
            this.progress = progress;
            this.reportInterval = (int)(reportInterval ?? DefaultReportInterval).TotalMilliseconds;
        }


        protected abstract int GetProgress();

        protected void UpdateProgress()
        {
            // Because this method is called pretty frequently, not having DateTime's small overhead is worth it
            var now = Environment.TickCount64;
            if (now - lastReport < reportInterval) 
                return;

            progress.Report(GetProgress());
            lastReport = now;
        }
    }
}

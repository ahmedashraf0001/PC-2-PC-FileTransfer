using System.Diagnostics;
using Sender.Services;
using Share_App.Main;

namespace Share_App.Extensions
{
    public class TransferSpeedometer : IDisposable, ITransferSpeedometer
    {
        private readonly IProgress<string> ProgressInfo;
        private readonly IProgress<int> progressBar;

        Stopwatch stopwatch;
        long intervalReceived;
        public long Progressinterval { get; set; }
        public TransferSpeedometer(IProgress<string> _ProgressInfo, IProgress<int> _progressBar)
        {
            progressBar = _progressBar;
            ProgressInfo = _ProgressInfo;
            intervalReceived = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }
        public async Task UpdateSpeed(int bytesTransferred)
        {
            intervalReceived += bytesTransferred;

            if (stopwatch.ElapsedMilliseconds >= 1000)
            {
                double speedMbps = (intervalReceived / 1024.0 / 1024.0) / stopwatch.Elapsed.TotalSeconds;
                ProgressInfo.Report($"{speedMbps:F2} MB/s");
                stopwatch.Restart();
                intervalReceived = 0;
            }
        }
        public async Task UpdateProgress(long bytesTransferred, long TotalFileSize, long BufferSize, IProgress<String> status)
        {
            double percentage = (double)bytesTransferred / TotalFileSize * 100;
            ReportStatus($"{(int)percentage}%", status: status);
            if (bytesTransferred >= Progressinterval)
            {
                progressBar.Report((int)percentage);
                Progressinterval += BufferSize;
            }
        }
        public static void ReportStatus(string report = null, string show = null, IProgress<String> status = null)
        {
            if (report != null)
            {
                status.Report(report);
            }
            if (show != null)
            {
                MessageBox.Show(show);
            }
        }
        public void Dispose()
        {
            stopwatch.Stop();
        }
    }
}

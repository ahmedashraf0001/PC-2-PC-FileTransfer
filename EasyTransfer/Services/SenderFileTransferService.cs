using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using Sender.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Sender.Services
{
    public class SenderFileTransferService: IDisposable
    {
        string _ip;
        int dataPort;
        int controlPort;
        private const int BufferSize = 1048576;

        long totalFileSize;
        public string[] _filepath;
        private bool _disposed = false;

        private CancellationTokenSource Sendtoken;
        private CancellationTokenSource Canceltoken;
        private CancellationTokenSource TaskToken;

        NetworkStream Datastream;
        NetworkStream Controlstream;  
        
        public volatile bool _isPaused;
        public volatile bool _isfinished;
        public volatile bool _isCancelled;
        private readonly object _lock = new object();

        private TcpClient _dataClient;
        private TcpClient _controlClient;


        IProgress<int> progress;
        IProgress<bool> visible;
        IProgress<String> status;
        IProgress<string> speed;
        IProgress<string> filetransfared;

        public SenderFileTransferService(string[] filePath, string ip, int _dataport, int _Controlport,
            IProgress<int> _progress, IProgress<bool> _visible, IProgress<String> _status,
            IProgress<string> _speed, IProgress<string> _filetransfared)
        {
            progress = _progress;
            status = _status;
            visible = _visible;
            speed = _speed;
            filetransfared = _filetransfared;
            dataPort = _dataport;
            controlPort = _Controlport;

            Sendtoken = new CancellationTokenSource();
            Canceltoken = new CancellationTokenSource();
            TaskToken = new CancellationTokenSource();

            _ip = ip;
            _filepath = filePath;
        }

        public void Update_isPaused(bool value)
        {
            lock (_lock)
            {
                _isPaused = value;
            }
        }

        public bool Read_isPaused()
        {
            lock (_lock)
            {
                return _isPaused;
            }
        }
        public void Update_isCanceled(bool value)
        {
            lock (_lock)
            {
                _isCancelled = value;
            }
        }
        public bool Read_isFinished()
        {
            lock (_lock)
            {
                return _isfinished;
            }
        }
        public void Update_isFinished(bool value)
        {
            lock (_lock)
            {
                _isfinished = value;
            }
        }

        public bool Read_isCanceled()
        {
            lock (_lock)
            {
                return _isCancelled;
            }
        }

        private string FileInfo(string _filepath)
        {
            string filename = Path.GetFileName(_filepath);
            string filetype = Path.GetExtension(_filepath).ToLower();
            string filesize = totalFileSize.ToString();
            string delimiter = "|";
            string message = filename + delimiter + filesize + delimiter + filetype + delimiter;
            return message;
        }
        private async Task SendMessage(NetworkStream stream, string message)
        {
            byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
            if (stream.CanWrite)
            {
                await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);
            }
        }
        private async Task SendControlMessage(string message)
        {
            if (_controlClient != null && _controlClient.Connected)
            {
                StreamWriter writer = new StreamWriter(Controlstream);

                await writer.WriteLineAsync(message);
                await writer.FlushAsync();
            }
            else
            {
                throw new SocketException();
            }
        }
        public async Task Start()
        {
            Cleanup();
           
            if ((_dataClient == null || _controlClient == null) || !(_dataClient.Connected && _controlClient.Connected))
            {
                await EstablishConnections();
            }

            _ = Task.Run(() => ProcessControlMessages());

            await handleDataTransfer();
        }
        public async Task Resume()
        {
            try
            {
               await SendControlMessage("RESUME_TRANSFER");
            }
            catch (Exception ex)
            {
                ChangeStatus(
                   "Connection Error",
                   "The client is not connected. Please check the connection and try again.",
                   status
               );
                MessageBox.Show("The connection to the server is not active. Please ensure the connection is established.",
                                    "Connection Issue",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
            }
        }

        public async Task Pause()
        {
            try
            {
                await SendControlMessage("PAUSE_TRANSFER");
                await Task.Delay(100);
                Update_isPaused(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while trying to pause the transfer. Please try again later.",
                                "Pause Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public async Task Cancel()
        {
            try
            {
                await SendControlMessage("CANCEL_TRANSFER");
                Canceltoken.Cancel();
                Sendtoken.Cancel();
            }
            catch (Exception ex)
            {
                ChangeStatus(
                    "Cancel Error",
                    "An error occurred while canceling the transfer. Details: " + ex.Message,
                    status
                );
            }
        }
        private void Cleanup()
        {
             totalFileSize = 0;

            if (Sendtoken!= null)
            {
                Sendtoken.Cancel();
                Sendtoken.Dispose();
            }
            if (Canceltoken != null)
            {
                Canceltoken.Cancel();
                Canceltoken.Dispose();
            }
            if (TaskToken != null)
            {
                TaskToken.Cancel();
                TaskToken.Dispose();
            }

            Sendtoken = new CancellationTokenSource();
            Canceltoken = new CancellationTokenSource();
            TaskToken = new CancellationTokenSource();

            Update_isCanceled(false);
            Update_isFinished(false);
            Update_isPaused(false);
        }
        private async Task EstablishConnections()
        {
            if (_dataClient != null)
            {
                _dataClient.Close();
                _dataClient.Dispose();

                _dataClient = new TcpClient();
            }
            if (_dataClient == null)
            {
                _dataClient = new TcpClient();
            }
            if (_controlClient != null)
            {
                _controlClient.Close();
                _controlClient.Dispose();

                _controlClient = new TcpClient();
            }
            if (_controlClient == null)
            {
                _controlClient = new TcpClient();
            }

            await _dataClient.ConnectAsync(_ip, dataPort);
            await _controlClient.ConnectAsync(_ip, controlPort);

            Datastream = _dataClient.GetStream();
            Controlstream = _controlClient.GetStream();
            
            _dataClient.SendBufferSize = BufferSize;

        }
       
        public async Task handleDataTransfer()
        {
            try
            {
                if (_controlClient.Connected && _dataClient.Connected)
                {
                    string response;
                    StreamReader reader = new StreamReader(Datastream, encoding: Encoding.UTF8);
                    StreamWriter writer = new StreamWriter(Datastream);
                    
                        await writer.WriteLineAsync("START_TRANSFER");
                        await writer.FlushAsync();
                        response = await reader.ReadLineAsync();

                        if (response == "ACK_START_TRANSFER")
                        {
                        MessageBox.Show("Connected to Receiver!", "Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await ProcessDataTransfer();
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong while starting the file transfer. Please check your connection and try again.",
                                "File Transfer Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        private async Task TransferDataInfo(int index)
        {
            string infoMessage = FileInfo(_filepath[index]);
            byte[] typebuffer = Encoding.UTF8.GetBytes(infoMessage);
            await Datastream.WriteAsync(typebuffer, 0, typebuffer.Length).ConfigureAwait(false);
        }
        private async Task TransferData(int numOfFiles, int index)
        {
            progress.Report(0);

            TransferSpeedometer Speedometer = new TransferSpeedometer(speed,progress);

            using (FileStream fileStream = new FileStream(_filepath[index], FileMode.Open, FileAccess.Read))
            {
                int read;
                long bytesSent = 0;
                Speedometer.Progressinterval = BufferSize;

                ChangeStatus("Sending..", status: status);
                visible.Report(true);

                byte[] buffer = new byte[BufferSize];

                while (true)
                {
                    try
                    {
                        if (fileStream.Position < fileStream.Length)
                        {
                            if (Canceltoken.IsCancellationRequested)
                            {
                                progress.Report(0);
                                ChangeStatus("transfer stopped", status: status);
                                Canceltoken.Dispose();
                                Canceltoken = new CancellationTokenSource();
                                return;
                            }
                            if (Sendtoken.IsCancellationRequested)
                            {
                                ChangeStatus("transfer paused", status: status);
                                while (Sendtoken.IsCancellationRequested)
                                {
                                    if (!Canceltoken.IsCancellationRequested)
                                    {
                                        await Task.Delay(100);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            
                            read = await fileStream.ReadAsync(buffer, 0, buffer.Length, Sendtoken.Token).ConfigureAwait(false);

                            if (read > 0)
                            {
                                bytesSent += read;

                                await Speedometer.UpdateSpeedInfo(read);
                                await Speedometer.UpdateSpeedProgBar(bytesSent, totalFileSize, BufferSize, status);

                                if (Datastream.CanWrite)
                                {
                                    await Datastream.WriteAsync(buffer, 0, read, Sendtoken.Token).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            Update_isFinished(true);
                            progress.Report(0);
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred during the file transfer process. Please try again or contact support if the problem persists.",
                                        "File Transfer Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }


                }
            }
        }
        private async Task ProcessDataTransfer()
        {
            try
            {
                string delimiter = "|";
                string initMessage = $"{_filepath.Length}{delimiter}";

                await SendMessage(Datastream, initMessage).ConfigureAwait(false);

                for (int i = 0; i < _filepath.Length; i++)
                {
                    Update_isFinished(false);
                    filetransfared.Report($"{i + 1} Out Of {_filepath.Length}");
                    totalFileSize = new FileInfo(_filepath[i]).Length;

                    await TransferDataInfo(i);
                    await TransferData(_filepath.Length, i);

                    if (Read_isCanceled())
                    {
                        return;
                    }
                    await Task.Delay(100);
                }
            }
            catch (SocketException ex)
            {
                ChangeStatus(
                    "Connection Error",
                    "Network connection error occurred. Please ensure the server is running and reachable. Details: " + ex.Message,
                    status
                );
            }
            catch (IOException ex) when (ex.Message.Contains("closed"))
            {
                ChangeStatus(
                    "Transfer Cancelled",
                    "The file transfer was cancelled. Ensure the connection is active to retry.",
                    status
                );
            }
            catch (IOException ex)
            {
                ChangeStatus(
                    "I/O Error",
                    "An input/output error occurred during the transfer. Details: " + ex.Message,
                    status
                );
            }
            catch (OperationCanceledException)
            {
                ChangeStatus(
                    "Transfer Cancelled",
                    "The file transfer was cancelled by the user or system.",
                    status
                );
            }
            catch (Exception ex)
            {
                ChangeStatus(
                    "Unexpected Error",
                    "An unexpected error occurred during the file transfer. Details: " + ex.Message,
                    status
                );
            }
            finally
            {
                visible.Report(false);
                status.Report("Ready for the next transfer!");
            }
        }
        private async Task ProcessControlMessages()
        {
            try
            {
                StreamReader reader = new StreamReader(_controlClient.GetStream(), encoding: Encoding.UTF8);
                StreamWriter writer = new StreamWriter(_controlClient.GetStream());

                while (true)
                {
                    if (TaskToken.IsCancellationRequested)
                    {
                        return;
                    }

                    string message = await reader.ReadLineAsync(TaskToken.Token);

                    switch (message)
                    {
                        case "ACK_PAUSE_TRANSFER":
                            Update_isPaused(true);
                            await Task.Delay(100);
                            Sendtoken.Cancel();
                            break;

                        case "ACK_RESUME_TRANSFER":
                            Update_isPaused(false);
                            await Task.Delay(100);
                            Sendtoken.Dispose();
                            Sendtoken = new CancellationTokenSource();
                            break;

                        case "ACK_CANCEL_TRANSFER":
                            Update_isCanceled(true);
                            return;

                        case "CANCEL_TRANSFER":
                            await writer.WriteLineAsync("ACK_CANCEL_TRANSFER");
                            await writer.FlushAsync();
                            Update_isCanceled(true);
                            Sendtoken.Cancel();
                            Canceltoken.Cancel();
                            return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Intentionally left blank as cancellation is expected in some cases.
            }
            catch (ObjectDisposedException)
            {
                MessageBox.Show("The connection was closed unexpectedly. Please check the server status and try again.",
                                "Connection Closed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred during message processing: {ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        public static void ChangeStatus(string report = null, string show = null, IProgress<string> status = null)
        {
            if (report != null)
            {
                status.Report(report);
            }
            if (show != null)
            {
                MessageBox.Show(show, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                TaskToken?.Cancel();
                TaskToken?.Dispose();

                Canceltoken?.Cancel();
                Canceltoken?.Dispose();

                Sendtoken?.Cancel();
                Sendtoken?.Dispose();

                if (_dataClient != null && _dataClient.Connected)
                {
                    try
                    {
                        _dataClient.GetStream()?.Close();
                    }
                    catch {  }

                    _dataClient.Close();
                    _dataClient.Dispose();
                }

                if (_controlClient != null && _controlClient.Connected )
                {
                    try
                    {
                        _controlClient.GetStream()?.Close();
                    }
                    catch {  }

                    _controlClient.Close();
                    _controlClient.Dispose();
                }
            }


            _disposed = true;
        }
        ~SenderFileTransferService()
        {
            Dispose(false);
        }
    }

}

public class TransferSpeedometer : IDisposable
{
    private readonly IProgress<string> ProgressInfo;
    private readonly IProgress<int> progressBar;

    Stopwatch stopwatch;
    long intervalReceived;
    public long Progressinterval {  get; set; }
    public TransferSpeedometer(IProgress<string> _ProgressInfo, IProgress<int> _progressBar)
    {
        progressBar = _progressBar;
        ProgressInfo = _ProgressInfo;
        intervalReceived = 0;
        stopwatch = new Stopwatch();
        stopwatch.Start();
    }
    public async Task UpdateSpeedInfo(int bytesReceived)
    {
        intervalReceived += bytesReceived;

        if (stopwatch.ElapsedMilliseconds >= 1000)
        {
            double speedMbps = (intervalReceived / 1024.0 / 1024.0) / stopwatch.Elapsed.TotalSeconds;
            ProgressInfo.Report($"{speedMbps:F2} MB/s");
            stopwatch.Restart();
            intervalReceived = 0;
        }
    }
    public async Task UpdateSpeedProgBar(long bytesSent, long totalFileSize, long BufferSize, IProgress<String> status)
    {
        double percentage = (double)bytesSent / totalFileSize * 100;
        SenderFileTransferService.ChangeStatus($"{(int)percentage}%", status: status);
        if (bytesSent >= Progressinterval)
        {
            progressBar.Report((int)percentage);
            Progressinterval += BufferSize;
        }
    }
    public void Dispose()
    {
        stopwatch.Stop();
    }
}
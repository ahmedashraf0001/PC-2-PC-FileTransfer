using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.NetworkInformation;
using System.Diagnostics;
using WinFormsApp2;

namespace Reciever.Services
{
    public class ReceiverFileTransferService : IDisposable
    {
        int dataPort;
        int controlPort;

        public string savepath;
        int numOfFiles;
        string finalFilePath;
        long filesize;

        Receiver form;

        private CancellationTokenSource Sendtoken;
        private CancellationTokenSource Canceltoken;
        private CancellationTokenSource TaskToken;
        private CancellationTokenSource _listenerCancellationTokenSource;

        private const int BufferSize = 1048576;

        private TcpListener _dataListener;
        private TcpListener _controlListener;
        private bool _disposed = false;

        private TcpClient _dataClient;
        private TcpClient _controlClient;

        public bool _isTransfering = false;
        private volatile bool _isPaused;

        private volatile bool _isfinished;

        private volatile bool _isCanceled;

        private readonly object _lock = new object();

        
        private bool _areControlTasksRunning = false;
        private bool _areDataTransferTasksRunning = false;

        IProgress<String> status;
        //IProgress<bool> isRunning;
        IProgress<int> progress;
        IProgress<bool> visible;
        IProgress<string> speed;
        IProgress<string> filetransfared;


       

        public ReceiverFileTransferService(Receiver control, string _savepath, int _dataport, int _Controlport,
        IProgress<String> _status,
        IProgress<int> _progress,
        IProgress<bool> _visible,
        IProgress<string> _speed,
        IProgress<string> _filetransfared
        )
        {
            form = control;
            savepath = _savepath;
            status = _status;
            progress = _progress;
            visible = _visible;
            speed = _speed;
            filetransfared = _filetransfared;

            dataPort = _dataport;
            controlPort = _Controlport;

            Sendtoken = new CancellationTokenSource();
            Canceltoken = new CancellationTokenSource();
            TaskToken = new CancellationTokenSource();
            _listenerCancellationTokenSource = new CancellationTokenSource();

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

        public void Update_isfinished(bool value)
        {
            lock (_lock)
            {
                _isfinished = value;
            }
        }

        public bool Read_isfinished()
        {
            lock (_lock)
            {
                return _isfinished;
            }
        }

        public void Update_isCanceled(bool value)
        {
            lock (_lock)
            {
                _isCanceled = value;
            }
        }

        public bool Read_isCanceled()
        {
            lock (_lock)
            {
                return _isCanceled;
            }
        }

        public static string GetLocalIPAddress()
        {
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            throw new Exception("No IPv4 address found for this device.");
        }

        public async Task Start()
        {
            Cleanup();

            if ((_dataClient == null || _controlClient == null) || !(_dataClient.Connected && _controlClient.Connected))
            {
                await EstablishConnections();
            }
            if (!_areControlTasksRunning)
            {
                _areControlTasksRunning = true;
                _ = Task.Run(() => ProcessControlMessages());
            }
            if (!_areDataTransferTasksRunning)
            {
                _areDataTransferTasksRunning = true;

                await HandleDataTransferAsync();
            }
        }
        public void Cleanup()
        {
            numOfFiles = 0;
            finalFilePath = null;
            filesize = 0;

            if (Sendtoken != null)
            {
                Sendtoken.Cancel();
                Sendtoken.Dispose();
            }
            if (Canceltoken != null)
            {
                Canceltoken.Cancel();
                Canceltoken.Dispose();
            }

            Sendtoken = new CancellationTokenSource();
            Canceltoken = new CancellationTokenSource();

            Update_isCanceled(false);
            Update_isfinished(false);
            Update_isPaused(false);
        }
        public async Task Reset()
        {
            numOfFiles = 0;
            finalFilePath = null;
            filesize = 0;

            Update_isCanceled(false);
            Update_isfinished(false);
            Update_isPaused(false);

            TaskToken.Cancel();
            Sendtoken.Cancel();
            Canceltoken.Cancel();
           
            await EstablishConnections();

            TaskToken.Dispose();
            TaskToken = new CancellationTokenSource();

            Sendtoken.Dispose();
            Sendtoken = new CancellationTokenSource();

            Canceltoken.Dispose();
            Canceltoken = new CancellationTokenSource();

            _ = Task.Run(() => ProcessControlMessages());

            await HandleDataTransferAsync();
        }
        private async Task EstablishConnections()
        {

            if (_dataListener == null)
            {
                _dataListener = new TcpListener(IPAddress.Any, dataPort);
                _dataListener.Start();
            }
            if (_controlListener == null)
            {
                _controlListener = new TcpListener(IPAddress.Any, controlPort);
                _controlListener.Start();
            }

            if (_dataClient != null)
            {
                _dataClient.Close();
                _dataClient.Dispose();
            }
            if (_controlClient != null)
            {
                _controlClient.Close();
                _controlClient.Dispose();
            }

            _dataClient = await _dataListener.AcceptTcpClientAsync(_listenerCancellationTokenSource.Token);
            _controlClient = await _controlListener.AcceptTcpClientAsync(_listenerCancellationTokenSource.Token);
            
           
            _dataClient.ReceiveBufferSize = BufferSize;

        }
        private async Task ReadNumOfFiles(NetworkStream networkStream)
        {
            StringBuilder NumberOfFilesBuilder = new StringBuilder();
            byte[] typeBuffer = new byte[1];
            char delimiter = '|';
            bool isReadingNumOfFiles = true;

            while (true)
            {
                int bytesRead = await networkStream.ReadAsync(typeBuffer, 0, typeBuffer.Length);
                if (bytesRead > 0)
                {
                    char receivedChar = (char)typeBuffer[0];

                    if (receivedChar == delimiter)
                    {
                        isReadingNumOfFiles = false;
                    }
                    if (isReadingNumOfFiles)
                    {
                        NumberOfFilesBuilder.Append(receivedChar);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (NumberOfFilesBuilder.Length > 0)
            {
                numOfFiles = Convert.ToInt32(NumberOfFilesBuilder.ToString());
                typeBuffer = new byte[1];
            }
            else
            {
                numOfFiles = 0;
            }
        }
        private async Task ReadDataInfo(NetworkStream networkStream)
        {
            StringBuilder fileTypeBuilder = new StringBuilder();
            StringBuilder fileNameBuilder = new StringBuilder();
            StringBuilder fileSizeBuilder = new StringBuilder();

            byte[] typeBuffer = new byte[1];
            char delimiter = '|';

            bool isReadingFileName = true;
            bool isReadingFileSize = false;
            bool isReadingFileType = false;

            while (true)
            {
                int bytesRead = await networkStream.ReadAsync(typeBuffer, 0, typeBuffer.Length).ConfigureAwait(false);
                if (bytesRead > 0)
                {
                    char receivedChar = (char)typeBuffer[0];
                    if (receivedChar == delimiter)
                    {
                        if (isReadingFileName)
                        {
                            isReadingFileName = false;
                            isReadingFileSize = true;
                        }
                        else if (isReadingFileSize)
                        {
                            isReadingFileSize = false;
                            isReadingFileType = true;
                        }
                        else if (isReadingFileType)
                        {
                            isReadingFileType = false;
                            break;
                        }
                    }
                    else
                    {
                        if (isReadingFileName)
                        {
                            fileNameBuilder.Append(receivedChar);
                        }
                        else if (isReadingFileSize)
                        {
                            fileSizeBuilder.Append(receivedChar);
                        }
                        else if (isReadingFileType)
                        {
                            fileTypeBuilder.Append(receivedChar);
                        }
                    }
                }
            }
            string fileName = fileNameBuilder.ToString();
            string filetype = fileTypeBuilder.ToString();
            filesize = Convert.ToInt64(fileSizeBuilder.ToString());
            finalFilePath = Path.Combine(savepath, fileName);
        }
        private async Task TransferData(NetworkStream networkStream)
        {
            long received = 0;
            int progressInterval = 1048576;
            int intervalReceived = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            progress.Report(0);
            visible.Report(true);

            if (!string.IsNullOrEmpty(finalFilePath))
            {
                using (FileStream fileStream = new FileStream(finalFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[BufferSize];
                    while (true)
                    {
                      
                        try
                        {
                            if (received < filesize)
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
                                int read = await networkStream.ReadAsync(buffer, 0, buffer.Length, Sendtoken.Token).ConfigureAwait(false);
                              

                                received += read;
                                intervalReceived += read;
                                await fileStream.WriteAsync(buffer, 0, read, Sendtoken.Token).ConfigureAwait(false);
                                if (stopwatch.ElapsedMilliseconds >= 1000)
                                {
                                    double speedBps = (double)intervalReceived / stopwatch.Elapsed.TotalSeconds;
                                    double speedKbps = speedBps / 1024;
                                    double speedMbps = speedKbps / 1024;
                                    speed.Report($"{speedMbps:F2} MB/s");
                                    stopwatch.Restart();
                                    intervalReceived = 0;
                                }

                                await Task.Run(() =>
                                {
                                    double percentage = (double)received / filesize * 100;
                                    if (percentage > 100)
                                    {
                                        percentage = 100;
                                    }
                                    status.Report($"{(int)percentage}%");
                                    if (received >= progressInterval)
                                    {
                                        progress.Report((int)percentage);
                                        progressInterval += BufferSize;
                                    }
                                }).ConfigureAwait(false);
                            }
                            else
                            {
                                Update_isfinished(true);
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
        }
        public async Task ProcessDataTransfer(NetworkStream networkStream)
        {
            try
            {
                status.Report("Listening...");
                //isRunning.Report(true);
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() => form.EnableCancelButton()));
                }
                else
                {
                    form.EnableCancelButton();
                }
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() => form.DisableStopListeningButton()));
                }
                else
                {
                    form.DisableStopListeningButton();
                }

                await ReadNumOfFiles(networkStream);

                for (int i = 0; i < numOfFiles; i++)
                {
                    progress.Report(0);
                    Update_isfinished(false);
                    filetransfared.Report($"{i + 1} Out Of {numOfFiles}");

                    await ReadDataInfo(networkStream);

                    await TransferData(networkStream);
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
                progress.Report(0);
                visible.Report(false);
                status.Report("Ready for next Transfer!");
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() => form.DisableCancelButton()));
                }
                else
                {
                    form.DisableCancelButton();
                }
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() => form.EnableStopListeningButton()));
                }
                else
                {
                    form.EnableStopListeningButton();
                }
                await Reset();
            }
        }

        public async Task HandleDataTransferAsync()
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    if (_controlClient.Connected && _dataClient.Connected)
                    {
                        NetworkStream stream = _dataClient.GetStream();
                        StreamReader reader = new StreamReader(_dataClient.GetStream(), Encoding.UTF8);
                        StreamWriter writer = new StreamWriter(_dataClient.GetStream()) { AutoFlush = true };

                        while (_controlClient.Connected && _dataClient.Connected)
                        {
                            //try
                            //{
                                if (TaskToken.IsCancellationRequested)
                                {
                                    return;
                                }
                                string response = await reader.ReadLineAsync(TaskToken.Token);
                                if (response == null) continue;

                                if (response == "START_TRANSFER")
                                {
                                    await writer.WriteLineAsync("ACK_START_TRANSFER");
                                    await writer.FlushAsync();
                                    await ProcessDataTransfer(stream);
                                }
                            //}
                            //catch (Exception ex)
                            //{
                            //    MessageBox.Show($"Error in data transfer loop: {ex.Message}");
                            //    break;
                            //}
                            await Task.Delay(1000);
                        }
                    }
                });
                
            }
            catch(OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing data transfer: {ex.Message}");
            }
        }
        private async Task ProcessControlMessages()
        {
            try
            {
                if (_controlClient.Connected && _dataClient.Connected)
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
                            case "PAUSE_TRANSFER":
                                await writer.WriteLineAsync("ACK_PAUSE_TRANSFER");
                                await writer.FlushAsync();
                                Update_isPaused(true);
                                Sendtoken.Cancel();
                                break;
                            case "RESUME_TRANSFER":
                                await writer.WriteLineAsync("ACK_RESUME_TRANSFER");
                                await writer.FlushAsync();
                                Update_isPaused(false);
                                Sendtoken.Dispose();
                                Sendtoken = new CancellationTokenSource();
                                break;
                            case "CANCEL_TRANSFER":
                                await writer.WriteLineAsync("ACK_CANCEL_TRANSFER");
                                await writer.FlushAsync();
                                Update_isCanceled(true);
                                Sendtoken.Cancel();
                                Canceltoken.Cancel();
                                break;
                            case "ACK_CANCEL_TRANSFER":
                                Update_isCanceled(true);
                                break;
                        }
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

        public async Task Cancel()
        {
            try
            {
                if (_controlClient != null && _controlClient.Connected)
                {
                    StreamWriter writer = new StreamWriter(_controlClient.GetStream());

                    await writer.WriteLineAsync("CANCEL_TRANSFER");
                    await writer.FlushAsync();

                    Sendtoken.Cancel();
                    Canceltoken.Cancel();
                }
                else
                {
                    ChangeStatus(
                        "Connection Error",
                        "The client is not connected. Please check the connection and try again.",
                        status
                    );
                }
            }
            catch (Exception ex)
            {
                ChangeStatus(
                    "Resume Error",
                    "An error occurred while resuming the transfer. Details: " + ex.Message,
                    status
                );
            }
        }
        private void ChangeStatus(string report = null, string show = null, IProgress<String> status = null)
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

                _listenerCancellationTokenSource.Cancel();
                _listenerCancellationTokenSource.Dispose();

                if (_dataListener != null)
                {
                    try
                    {
                        _dataListener.Stop(); 
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        _dataListener.Dispose(); 
                        _dataListener = null; 
                    }
                }
                if (_controlListener != null)
                {
                    try
                    {
                        _controlListener.Stop();
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        _controlListener.Dispose();
                        _controlListener = null;
                    }
                }

                if (_dataClient != null && _dataClient.Connected)
                {
                    try
                    {
                        _dataClient.GetStream()?.Close();
                    }
                    catch
                    {
                    }

                    _dataClient.Close();
                    _dataClient.Dispose();
                }

                if (_controlClient != null && _controlClient.Connected)
                {
                    try
                    {
                        _controlClient.GetStream()?.Close();
                    }
                    catch
                    {
                    }

                    _controlClient.Close();
                    _controlClient.Dispose();
                }
            }

            _disposed = true;
        }

        ~ReceiverFileTransferService()
        {
            Dispose(false);
        }
    }
}

using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.NetworkInformation;
using System.Diagnostics;
using WinFormsApp2;
using Share_App.Configurations;
using Share_App.Extensions;
using Share_App.Main;
using Share_App.Error_Handling;

namespace Reciever.Services
{
    public class ReceiverFileTransferService : IDisposable, IFileTransferService
    {
        public ReceiverConfigurations Configurations;
        ExceptionHelper ExceptionHelper;
        public ReceiverFileTransferService(Receiver control, string _savepath, int _dataport, int _Controlport,
                                            IProgress<String> _status, IProgress<int> _progress,
                                            IProgress<bool> _visible, IProgress<string> _speed,
                                            IProgress<string> _filetransfared)
        {
            Configurations = new ReceiverConfigurations(control, _savepath, _dataport, _Controlport, _status,
                                                        _progress, _visible,_speed, _filetransfared);
            ExceptionHelper = new ExceptionHelper();
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
        private async Task EstablishConnections()
        {

            if (Configurations._dataListener == null)
            {
                Configurations._dataListener = new TcpListener(IPAddress.Any, Configurations.dataPort);
                Configurations._dataListener.Start();
            }
            if (Configurations._controlListener == null)
            {
                Configurations._controlListener = new TcpListener(IPAddress.Any, Configurations.controlPort);
                Configurations._controlListener.Start();
            }

            if (Configurations._dataClient != null)
            {
                Configurations._dataClient.Close();
                Configurations._dataClient.Dispose();
            }
            if (Configurations._controlClient != null)
            {
                Configurations._controlClient.Close();
                Configurations._controlClient.Dispose();
            }

            Configurations._dataClient = await Configurations._dataListener.AcceptTcpClientAsync(Configurations._listenerCancellationTokenSource.Token).ConfigureAwait(false);
            Configurations._controlClient = await Configurations._controlListener.AcceptTcpClientAsync(Configurations._listenerCancellationTokenSource.Token).ConfigureAwait(false);

            Configurations._dataClient.NoDelay = true;
            Configurations._controlClient.NoDelay = true;

            Configurations._dataClient.ReceiveBufferSize = Configurations.BufferSize;

        }

        public async Task Start()
        {
            Cleanup();

            if ((Configurations._dataClient == null || Configurations._controlClient == null) ||
                !(Configurations._dataClient.Connected && Configurations._controlClient.Connected))
            {
                await EstablishConnections().ConfigureAwait(false);
            }
            if (!Configurations._areControlTasksRunning)
            {
                Configurations._areControlTasksRunning = true;
                _ = Task.Run(() => ProcessControlMessages()).ConfigureAwait(false);
            }
            if (!Configurations._areDataTransferTasksRunning)
            {
                Configurations._areDataTransferTasksRunning = true;

                await HandleDataTransferAsync().ConfigureAwait(false);
            }
        }
     
        public Task Resume()
        {
            throw new NotImplementedException();
        }

        public Task Pause()
        {
            throw new NotImplementedException();
        }
        public async Task Cancel()
        {
            try
            {
                await SendControlMessage(TransferControlMessage.CancelTransfer).ConfigureAwait(false);
                Configurations.Sendtoken.Cancel();
                Configurations.Canceltoken.Cancel();

            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException( ex, "Receiver ");
            }
        }
        private async Task ReadNumOfFiles(NetworkStream networkStream)
        {
            StringBuilder NumberOfFilesBuilder = new StringBuilder();
            byte[] typeBuffer = new byte[1];
            char delimiter = '|';
            bool isReadingNumOfFiles = true;

            while (true)
            {
                int bytesRead = await networkStream.ReadAsync(typeBuffer, 0, typeBuffer.Length).ConfigureAwait(false);
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
                Configurations.numOfFiles = Convert.ToInt32(NumberOfFilesBuilder.ToString());
                typeBuffer = new byte[1];
            }
            else
            {
                Configurations.numOfFiles = 0;
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
            Configurations.filesize = Convert.ToInt64(fileSizeBuilder.ToString());
            Configurations.finalFilePath = Path.Combine(Configurations.savepath, fileName);
        }
        private async Task TransferData(NetworkStream networkStream)
        {
            long received = 0;
            TransferSpeedometer Speedometer = new TransferSpeedometer(Configurations.speed, Configurations.progress);

            Configurations.progress.Report(0);
            Configurations.visible.Report(true);

            if (!string.IsNullOrEmpty(Configurations.finalFilePath))
            {
                using (FileStream fileStream = new FileStream(Configurations.finalFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[Configurations.BufferSize];
                    while (true)
                    {
                      
                        try
                        {
                            if (received < Configurations.filesize)
                            {
                                if (Configurations.Canceltoken.IsCancellationRequested)
                                {
                                    Configurations.progress.Report(0);
                                    TransferSpeedometer.ReportStatus("Transfer Stopped", status: Configurations.status);
                                    Configurations.Canceltoken.Dispose();
                                    Configurations.Canceltoken = new CancellationTokenSource();
                                    return;
                                }
                                if (Configurations.Sendtoken.IsCancellationRequested)
                                {
                                    TransferSpeedometer.ReportStatus("Transfer Paused", status: Configurations.status);
                                    while (Configurations.Sendtoken.IsCancellationRequested)
                                    {
                                        if (!Configurations.Canceltoken.IsCancellationRequested)
                                        {
                                            await Task.Delay(100);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                int read = await networkStream.ReadAsync(buffer, 0, buffer.Length, Configurations.Sendtoken.Token).ConfigureAwait(false);
                              

                                received += read;
                                await fileStream.WriteAsync(buffer, 0, read, Configurations.Sendtoken.Token).ConfigureAwait(false);
                                
                                await Speedometer.UpdateSpeed(read);

                                await Speedometer.UpdateProgress(received, Configurations.filesize, Configurations.BufferSize, Configurations.status);

                            }
                            else
                            {
                                Configurations.UpdateTransferState(isFinished: true);
                                Configurations.progress.Report(0);
                                break;
                            }
                        }
                        catch (OperationCanceledException) {  }
                        catch (Exception ex)
                        {
                            ExceptionHelper.handleException(ex, "Receiver 1");
                            break;
                        }
                    }
                }
            }
        }
        public async Task ProcessDataTransfer(NetworkStream networkStream)
        {
            try
            {
                Configurations.status.Report("Listening...");

                StartTransferUIState();

                await ReadNumOfFiles(networkStream).ConfigureAwait(false);

                for (int i = 0; i < Configurations.numOfFiles; i++)
                {
                    Configurations.progress.Report(0);
                    Configurations.UpdateTransferState(isFinished: false);
                    Configurations.filetransfared.Report($"{i + 1} Out Of {Configurations.numOfFiles}");

                    await ReadDataInfo(networkStream).ConfigureAwait(false);

                    await TransferData(networkStream).ConfigureAwait(false);
                    if (Configurations.ReadTransferState(isCancelled: true))
                    {
                        return;
                    }
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Receiver 2");
            }
            finally
            {
                Configurations.progress.Report(0);
                Configurations.visible.Report(false);
                Configurations.status.Report("Ready for next Transfer!");
                FinishUIState();
                await Reset().ConfigureAwait(false);
            }
        }
        private void StartTransferUIState()
        {
            if (Configurations.form.InvokeRequired)
            {
                Configurations.form.Invoke(new Action(() => Configurations.form.EnableCancelButton()));
            }
            else
            {
                Configurations.form.EnableCancelButton();
            }
            if (Configurations.form.InvokeRequired)
            {
                Configurations.form.Invoke(new Action(() => Configurations.form.DisableStopListeningButton()));
            }
            else
            {
                Configurations.form.DisableStopListeningButton();
            }
        }
        private void FinishUIState()
        {
            if (Configurations.form.InvokeRequired)
            {
                Configurations.form.Invoke(new Action(() => Configurations.form.DisableCancelButton()));
            }
            else
            {
                Configurations.form.DisableCancelButton();
            }
            if (Configurations.form.InvokeRequired)
            {
                Configurations.form.Invoke(new Action(() => Configurations.form.EnableStopListeningButton()));
            }
            else
            {
                Configurations.form.EnableStopListeningButton();
            }
        }
        public async Task HandleDataTransferAsync()
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    if (Configurations._controlClient.Connected && Configurations._dataClient.Connected)
                    {
                        NetworkStream stream = Configurations._dataClient.GetStream();
                        StreamReader reader = new StreamReader(Configurations._dataClient.GetStream(), Encoding.UTF8);
                        StreamWriter writer = new StreamWriter(Configurations._dataClient.GetStream()) { AutoFlush = true };

                        while (Configurations._controlClient.Connected && Configurations._dataClient.Connected)
                        {
                            if (Configurations.TaskToken.IsCancellationRequested)
                            {
                                return;
                            }
                            string response = await reader.ReadLineAsync(Configurations.TaskToken.Token);
                            if (response == null) continue;

                            if (response == TransferControlMessage.StartTransfer.ToMessageString())
                            {
                                await writer.WriteLineAsync(TransferControlMessage.AckStartTransfer.ToMessageString()).ConfigureAwait(false);
                                await writer.FlushAsync().ConfigureAwait(false);
                                await ProcessDataTransfer(stream);
                            }
                            await Task.Delay(1000);
                        }
                    }
                });
                
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Receiver 3");
            }
        }
        private async Task SendControlMessage(TransferControlMessage message)
        {
            if (Configurations._controlClient != null && Configurations._controlClient.Connected)
            {
                StreamWriter writer = new StreamWriter(Configurations._controlClient.GetStream());

                await writer.WriteLineAsync(message.ToMessageString()).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
            else
            {
                throw new SocketException();
            }

            if (message == TransferControlMessage.AckPauseTransfer)
            {
                Configurations.UpdateTransferState(isPaused: true);
                Configurations.Sendtoken.Cancel();
            }
            if (message == TransferControlMessage.AckResumeTransfer)
            {
                Configurations.UpdateTransferState(isPaused: false);
                Configurations.Sendtoken.Dispose();
                Configurations.Sendtoken = new CancellationTokenSource();

            }
            if (message == TransferControlMessage.AckCancelTransfer)
            {
                Configurations.UpdateTransferState(isCancelled: true);
                Configurations.Sendtoken.Cancel();
                Configurations.Canceltoken.Cancel();
            }
        }
        private async Task HandleControlMessage(TransferControlMessage message, StreamWriter writer)
        {
            switch (message)
            {
                case TransferControlMessage.PauseTransfer:
                    await SendControlMessage(TransferControlMessage.AckPauseTransfer).ConfigureAwait(false);

                    break;
                case TransferControlMessage.ResumeTransfer:
                    await SendControlMessage(TransferControlMessage.AckResumeTransfer).ConfigureAwait(false);

                    break;
                case TransferControlMessage.CancelTransfer:
                    await SendControlMessage(TransferControlMessage.AckCancelTransfer).ConfigureAwait(false);

                    break;
                case TransferControlMessage.AckCancelTransfer:
                    Configurations.UpdateTransferState(isCancelled: true);
                    break;
            }
        }
        private async Task ProcessControlMessages()
        {
            try
            {
                if (Configurations._controlClient.Connected && Configurations._dataClient.Connected)
                {
                    StreamReader reader = new StreamReader(Configurations._controlClient.GetStream(), encoding: Encoding.UTF8);
                    StreamWriter writer = new StreamWriter(Configurations._controlClient.GetStream());
                    while (!Configurations.TaskToken.IsCancellationRequested)
                    {
                        string messageString = await reader.ReadLineAsync(Configurations.TaskToken.Token).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(messageString)) continue;
                       
                        TransferControlMessage message = TransferControlMessageExtensions.ParseControlMessage(messageString);
                        await HandleControlMessage(message,  writer).ConfigureAwait(false);
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Receiver 4");
            }
        }

        public void Cleanup()
        {
            Configurations.CleanConfigs();
            Configurations.UpdateTransferState(isCancelled: false, isFinished: false, isPaused: false);
        }
        public async Task Reset()
        {
            Configurations.UpdateTransferState(isCancelled: false, isFinished: false, isPaused: false);

            Configurations.RebuildConfigs();

            await EstablishConnections().ConfigureAwait(false);

            _ = Task.Run(() => ProcessControlMessages()).ConfigureAwait(false);

            await HandleDataTransferAsync().ConfigureAwait(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Configurations._disposed)
                return;

            Configurations.Dispose(disposing);

            Configurations._disposed = true;
        }

        ~ReceiverFileTransferService()
        {
            Dispose(false);
        }
    }
}

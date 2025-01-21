using System.Net.Sockets;
using System.Text;
using Share_App.Configurations;
using Share_App.Error_Handling;
using Share_App.Extensions;
using Share_App.Main;

namespace Sender.Services
{
    public class FileTransferSenderService : IDisposable, IFileTransferService
    {
        SenderConfigurations Configurations;
        ExceptionHelper ExceptionHelper;
        public FileTransferSenderService(string[] filePath, string ip, int _dataport, int _Controlport,
                                         IProgress<int> _progress, IProgress<bool> _visible, IProgress<String> _status,
                                         IProgress<string> _speed, IProgress<string> _filetransfared)
        {
            Configurations = new SenderConfigurations(filePath, ip, _dataport, _Controlport, _progress,
                                                      _visible, _status, _speed, _filetransfared);
            ExceptionHelper = new ExceptionHelper();
        }

        private string CreateFileMetadata(string _filepath)
        {
            string filename = Path.GetFileName(_filepath);
            string filetype = Path.GetExtension(_filepath).ToLower();
            string filesize = Configurations.totalFileSize.ToString();
            string delimiter = "|";
            string message = filename + delimiter + filesize + delimiter + filetype + delimiter;
            return message;
        }

        private async Task SendNetworkMessage(NetworkStream stream, string message)
        {
            byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
            if (stream.CanWrite)
            {
                await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);
            }
        }

        private async Task InitializeNetworkConnections()
        {
            if (Configurations._dataClient != null)
            {
                Configurations._dataClient.Close();
                Configurations._dataClient.Dispose();

                Configurations._dataClient = new TcpClient();
            }
            if (Configurations._dataClient == null)
            {
                Configurations._dataClient = new TcpClient();
            }
            if (Configurations._controlClient != null)
            {
                Configurations._controlClient.Close();
                Configurations._controlClient.Dispose();

                Configurations._controlClient = new TcpClient();
            }
            if (Configurations._controlClient == null)
            {
                Configurations._controlClient = new TcpClient();
            }

            TransferSpeedometer.ReportStatus(
                  "Connecting...",
                  status: Configurations.status
              );
            await Configurations._dataClient.ConnectAsync(Configurations._ip, Configurations.dataPort);
            await Configurations._controlClient.ConnectAsync(Configurations._ip, Configurations.controlPort);

            Configurations.Datastream = Configurations._dataClient.GetStream();
            Configurations.Controlstream = Configurations._controlClient.GetStream();

            Configurations._dataClient.SendBufferSize = Configurations.BufferSize;
        }

        public async Task Start()
        {
            CleanupResources();

            if ((Configurations._dataClient == null || Configurations._controlClient == null) ||
                !(Configurations._dataClient.Connected && Configurations._controlClient.Connected))
            {
                await InitializeNetworkConnections();
            }

            _ = Task.Run(() => MonitorControlMessages());

            await InitiateDataTransfer();
        }

        private async Task SendTransferControlMessage(TransferControlMessage message)
        {
            if (Configurations._controlClient != null && Configurations._controlClient.Connected)
            {
                StreamWriter writer = new StreamWriter(Configurations.Controlstream);

                await writer.WriteLineAsync(message.ToMessageString());
                await writer.FlushAsync();
            }
            else
            {
                throw new SocketException();
            }

            if (message == TransferControlMessage.PauseTransfer)
            {
                await Task.Delay(100);
                Configurations.UpdateTransferState(isPaused: true);
            }
            if (message == TransferControlMessage.CancelTransfer)
            {
                Configurations.UpdateTransferState(isCancelled: true);
                Configurations.Sendtoken.Cancel();
                Configurations.Canceltoken.Cancel();
            }
        }

        public async Task Resume()
        {
            try
            {
                await SendTransferControlMessage(TransferControlMessage.ResumeTransfer);
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Sender ");
            }
        }

        public async Task Pause()
        {
            try
            {
                await SendTransferControlMessage(TransferControlMessage.PauseTransfer);
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Sender ");
            }
        }

        public async Task Cancel()
        {
            try
            {
                await SendTransferControlMessage(TransferControlMessage.CancelTransfer);
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Sender ");
            }
        }

        public async Task InitiateDataTransfer()
        {
            try
            {
                if (Configurations._controlClient.Connected && Configurations._dataClient.Connected)
                {
                    string response;
                    StreamReader reader = new StreamReader(Configurations.Datastream, encoding: Encoding.UTF8);
                    StreamWriter writer = new StreamWriter(Configurations.Datastream);

                    await writer.WriteLineAsync(TransferControlMessage.StartTransfer.ToMessageString());
                    await writer.FlushAsync();
                    response = await reader.ReadLineAsync();

                    if (response == TransferControlMessage.AckStartTransfer.ToMessageString())
                    {
                        MessageBox.Show("Connected to Receiver!", "Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await ExecuteDataTransfer();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Sender ");
            }
        }

        private async Task SendFileMetadata(int index)
        {
            string infoMessage = CreateFileMetadata(Configurations._filepath[index]);
            byte[] typebuffer = Encoding.UTF8.GetBytes(infoMessage);
            await Configurations.Datastream.WriteAsync(typebuffer, 0, typebuffer.Length);
        }

        private async Task TransferFileContent(int numOfFiles, int index)
        {
            Configurations.progress.Report(0);

            TransferSpeedometer Speedometer = new TransferSpeedometer(Configurations.speed, Configurations.progress);

            using (FileStream fileStream = new FileStream(Configurations._filepath[index], FileMode.Open, FileAccess.Read))
            {
                int read;
                long bytesSent = 0;
                Speedometer.Progressinterval = Configurations.BufferSize;

                TransferSpeedometer.ReportStatus("Sending..", status: Configurations.status);
                Configurations.visible.Report(true);

                byte[] buffer = new byte[Configurations.BufferSize];
                const string EndOfFileMarker = "<EOF>";

                while (true)
                {
                    try
                    {
                        if (fileStream.Position < fileStream.Length)
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
                                        await Task.Delay(50);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            read = await fileStream.ReadAsync(buffer, 0, buffer.Length, Configurations.Sendtoken.Token);

                            if (read > 0)
                            {
                                bytesSent += read;

                                await Speedometer.UpdateSpeed(read);
                                await Speedometer.UpdateProgress(bytesSent, Configurations.totalFileSize, Configurations.BufferSize, Configurations.status);

                                if (Configurations.Datastream.CanWrite)
                                {
                                    await Configurations.Datastream.WriteAsync(buffer, 0, read, Configurations.Sendtoken.Token);
                                }
                            }
                        }
                        else
                        {
                            byte[] eofMarker = Encoding.UTF8.GetBytes(EndOfFileMarker);
                            await Configurations.Datastream.WriteAsync(eofMarker, 0, eofMarker.Length);
                            await Configurations.Datastream.FlushAsync();

                            Configurations.UpdateTransferState(isFinished: true);
                            Configurations.progress.Report(0);
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        ExceptionHelper.handleException(ex, "Sender ");
                        break;
                    }
                }
            }
        }

        private async Task ExecuteDataTransfer()
        {
            try
            {
                const string delimiter = "|";
                string initMessage = $"{Configurations._filepath.Length}{delimiter}";

                await SendNetworkMessage(Configurations.Datastream, initMessage);

                for (int i = 0; i < Configurations._filepath.Length; i++)
                {
                    Configurations.UpdateTransferState(isFinished: false);
                    Configurations.filetransfared.Report($"{i + 1} Out Of {Configurations._filepath.Length}");
                    Configurations.totalFileSize = new FileInfo(Configurations._filepath[i]).Length;

                    await SendFileMetadata(i); 
                    await TransferFileContent(Configurations._filepath.Length, i);

                    if (Configurations.ReadTransferState(isCancelled: true))
                    {
                        return;
                    }
                    TransferSpeedometer.ReportStatus("Preparing...", status: Configurations.status);
                    Configurations.progress.Report(100);
                    await Task.Delay(1500);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.handleException(ex, "Sender ");
            }
            finally
            {
                Configurations.visible.Report(false);
                Configurations.status.Report("Ready for the next transfer!");
            }
        }

        private async Task ProcessControlMessageResponse(TransferControlMessage message, StreamWriter writer)
        {
            switch (message)
            {
                case TransferControlMessage.AckPauseTransfer:
                    Configurations.UpdateTransferState(isPaused: true);
                    await Task.Delay(100);
                    Configurations.Sendtoken.Cancel();
                    break;

                case TransferControlMessage.AckResumeTransfer:
                    Configurations.UpdateTransferState(isPaused: false);
                    await Task.Delay(100);
                    Configurations.Sendtoken.Dispose();
                    Configurations.Sendtoken = new CancellationTokenSource();
                    break;

                case TransferControlMessage.AckCancelTransfer:
                    Configurations.UpdateTransferState(isCancelled: true, isPaused: false);
                    return;

                case TransferControlMessage.CancelTransfer:
                    await SendTransferControlMessage(message);
                    return;
            }
        }

        private async Task MonitorControlMessages()
        {
            try
            {
                StreamReader reader = new StreamReader(Configurations._controlClient.GetStream(), encoding: Encoding.UTF8);
                StreamWriter writer = new StreamWriter(Configurations._controlClient.GetStream());

                while (!Configurations.TaskToken.IsCancellationRequested)
                {
                    string messageString = await reader.ReadLineAsync(Configurations.TaskToken.Token);
                    if (string.IsNullOrEmpty(messageString)) continue;

                    TransferControlMessage message = TransferControlMessageExtensions.ParseControlMessage(messageString);
                    await ProcessControlMessageResponse(message, writer);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) when (ex is not System.IO.IOException)
            {
                ExceptionHelper.handleException(ex, "Sender ");
            }
        }

        private void CleanupResources()
        {
            Configurations.ResetConfigs();
            Configurations.UpdateTransferState(isPaused: false, isCancelled: false, isFinished: false);
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

        ~FileTransferSenderService()
        {
            Dispose(false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinFormsApp2;

namespace Share_App.Configurations
{
    public class ReceiverConfigurations
    {
        public int dataPort;
        public int controlPort;

        public string savepath;
        public int numOfFiles;
        public string finalFilePath;
        public long filesize;

        public Receiver form;

        public CancellationTokenSource Sendtoken;
        public CancellationTokenSource Canceltoken;
        public CancellationTokenSource TaskToken;
        public CancellationTokenSource _listenerCancellationTokenSource;

        public readonly int BufferSize = 1048576;

        public TcpListener _dataListener;
        public TcpListener _controlListener;
        public bool _disposed = false;

        public TcpClient _dataClient;
        public TcpClient _controlClient;

        public bool _isTransfering = false;
        public volatile bool _isPaused;

        public volatile bool _isfinished;

        public volatile bool _isCanceled;

        public readonly object _lock = new object();


        public bool _areControlTasksRunning = false;
        public bool _areDataTransferTasksRunning = false;

        public IProgress<String> status;
        public IProgress<int> progress;
        public IProgress<bool> visible;
        public IProgress<string> speed;
        public IProgress<string> filetransfared;

        public ReceiverConfigurations(Receiver control, string _savepath, int _dataport, int _Controlport,
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
        public bool ReadTransferState(bool? isPaused = null, bool? isFinished = null, bool? isCancelled = null)
        {
            lock (_lock)
            {
                if (isPaused.HasValue)
                {
                    return _isPaused;
                }
                if (isFinished.HasValue)
                {
                    return _isfinished;
                }
                if (isCancelled.HasValue)
                {
                    return _isCanceled;
                }
            }
            return false;
        }
        public void UpdateTransferState(bool? isPaused = null, bool? isFinished = null, bool? isCancelled = null)
        {
            lock (_lock)
            {
                if (isPaused.HasValue)
                {
                    _isPaused = isPaused.Value;
                }
                if (isFinished.HasValue)
                {
                    _isfinished = isFinished.Value;
                }
                if (isCancelled.HasValue)
                {
                    _isCanceled = isCancelled.Value;
                }
            }
        }
        public void CleanConfigs()
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
        }
        public void RebuildConfigs()
        {
            numOfFiles = 0;
            finalFilePath = null;
            filesize = 0;

            TaskToken.Cancel();
            Sendtoken.Cancel();
            Canceltoken.Cancel();

            TaskToken.Dispose();
            TaskToken = new CancellationTokenSource();

            Sendtoken.Dispose();
            Sendtoken = new CancellationTokenSource();

            Canceltoken.Dispose();
            Canceltoken = new CancellationTokenSource();
        }
        public void Dispose(bool disposing)
        {
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

        }
    }
}

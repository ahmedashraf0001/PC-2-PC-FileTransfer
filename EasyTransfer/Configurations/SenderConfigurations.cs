using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Share_App.Configurations
{
    public class SenderConfigurations
    {
        public string _ip;
        public int dataPort;
        public int controlPort;
        public readonly int BufferSize = 1048576;

        public long totalFileSize;
        public string[] _filepath;
        public bool _disposed = false;

        public CancellationTokenSource Sendtoken;
        public CancellationTokenSource Canceltoken;
        public CancellationTokenSource TaskToken;

        public NetworkStream Datastream;
        public NetworkStream Controlstream;

        public volatile bool _isPaused;
        public volatile bool _isfinished;
        public volatile bool _isCancelled;
        public readonly object _lock = new object();

        public TcpClient _dataClient;
        public TcpClient _controlClient;


        public IProgress<int> progress;
        public IProgress<bool> visible;
        public IProgress<String> status;
        public IProgress<string> speed;
        public IProgress<string> filetransfared;

        public SenderConfigurations(string[] filePath, string ip, int _dataport, int _Controlport,
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
                    return _isCancelled;
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
                    _isCancelled = isCancelled.Value;
                }
            }
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

                if (_dataClient != null && _dataClient.Connected)
                {
                    try
                    {
                        _dataClient.GetStream()?.Close();
                    }
                    catch { }

                    _dataClient.Close();
                    _dataClient.Dispose();
                }

                if (_controlClient != null && _controlClient.Connected)
                {
                    try
                    {
                        _controlClient.GetStream()?.Close();
                    }
                    catch { }

                    _controlClient.Close();
                    _controlClient.Dispose();
                }
            }
        }
        public void ResetConfigs()
        {
            totalFileSize = 0;

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
            if (TaskToken != null)
            {
                TaskToken.Cancel();
                TaskToken.Dispose();
            }

            Sendtoken = new CancellationTokenSource();
            Canceltoken = new CancellationTokenSource();
            TaskToken = new CancellationTokenSource();
        }
    }
}

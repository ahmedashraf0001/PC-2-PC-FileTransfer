using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share_App.Main
{
    public interface ITransferSpeedometer : IDisposable
    {
        Task UpdateSpeed(int bytesTransferred);
        Task UpdateProgress(long bytesTransferred, long TotalFileSize, long BufferSize, IProgress<String> Status);
    }
}

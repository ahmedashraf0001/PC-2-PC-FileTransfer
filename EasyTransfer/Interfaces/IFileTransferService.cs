using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share_App.Main
{
    public interface IFileTransferService : IDisposable
    {
        Task Start();
        Task Resume();
        Task Pause();
        Task Cancel();
    }
}

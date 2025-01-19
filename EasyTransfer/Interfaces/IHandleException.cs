using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Share_App.Error_Handling.ExceptionHelper;

namespace Share_App.Interfaces
{
    public interface IHandleException
    {
        public void handleException(Exception exception, string from);
        public ErrorDetails GetExceptionDetails(Exception exception);
        public void ShowError(ErrorDetails details);
    }
}

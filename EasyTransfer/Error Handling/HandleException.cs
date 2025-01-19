using System.Net.Sockets;
using Share_App.Interfaces;

namespace Share_App.Error_Handling
{
    public class ExceptionHelper: IHandleException
    {
        public enum Severity
        {
            Info,
            Warning,
            Error,
            Critical
        }
        public class ErrorDetails
        {
            public string From { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public Severity Severity { get; set; }
            public MessageBoxIcon Icon { get; set; }
        }
        public void handleException(Exception exception, string from)
        {
            ErrorDetails details = GetExceptionDetails(exception);
            if (details != null) 
            {
                details.From = from;
                ShowError(details);
            }
        }

        public ErrorDetails GetExceptionDetails(Exception exception)
        {
            ErrorDetails details;

            switch (exception)
            {
                case SocketException socketException:
                    details = GetSocketErrorDetails(socketException);
                    break;

                case FileNotFoundException:
                    details = new ErrorDetails
                    {
                        Title = "File Not Found",
                        Message = "The file you're trying to send could not be found. Please verify the file path.",
                        Severity = Severity.Error,
                        Icon = MessageBoxIcon.Error
                    };
                    break;

                case UnauthorizedAccessException:
                    details = new ErrorDetails
                    {
                        Title = "Access Denied",
                        Message = "You don't have permission to access this file. Please check your permissions.",
                        Severity = Severity.Error,
                        Icon = MessageBoxIcon.Error
                    };
                    break;

                case IOException ioEx :
                    details = GetIoErrorDetails(ioEx);
                    break;
                case OperationCanceledException:
                    return null;

                case OutOfMemoryException:
                    details = new ErrorDetails
                    {
                        Title = "Memory Error",
                        Message = "Not enough memory to complete the operation. Please close some applications and try again.",
                        Severity = Severity.Critical,
                        Icon = MessageBoxIcon.Error
                    };
                    break;

                default:
                    details = new ErrorDetails
                    {
                        Title = exception.GetType().ToString(),
                        Message = exception.Message,
                        Severity = Severity.Error,
                        Icon = MessageBoxIcon.Error
                    };
                    break;
            }
            return details;
        }

        public ErrorDetails GetSocketErrorDetails(SocketException socketEx)
        {
            switch (socketEx.SocketErrorCode)
            {
                case SocketError.ConnectionRefused:
                    return new ErrorDetails
                    {
                        Title = "Connection Failed",
                        Message = "Could not connect to the receiver. Please check if it's running.",
                        Severity = Severity.Error,
                        Icon = MessageBoxIcon.Error
                    };

                case SocketError.TimedOut:
                    return new ErrorDetails
                    {
                        Title = "Connection Timeout",
                        Message = "The connection timed out. Please check your network and try again.",
                        Severity = Severity.Warning,
                        Icon = MessageBoxIcon.Warning
                    };

                default:
                    return new ErrorDetails
                    {
                        Title = "Network Error",
                        Message = "A network error occurred. Please check your connection.",
                        Severity = Severity.Error,
                        Icon = MessageBoxIcon.Error
                    };
            }
        }

        public ErrorDetails GetIoErrorDetails(IOException ioEx)
        {
            if (ioEx.Message.Contains("closed", StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorDetails
                {
                    Title = "Connection Lost",
                    Message = "The connection was lost unexpectedly. Please try again.",
                    Severity = Severity.Warning,
                    Icon = MessageBoxIcon.Warning
                };
            }

            return new ErrorDetails
            {
                Title = "I/O Error",
                Message = "An I/O error occurred during the operation. Please try again.",
                Severity = Severity.Error,
                Icon = MessageBoxIcon.Error
            };
        }

        public void ShowError(ErrorDetails details)
        {
            MessageBox.Show
            (
                details.Message,
                details.From + details.Title,
                MessageBoxButtons.OK,
                details.Icon
            );
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share_App.Extensions
{
    public enum TransferControlMessage
    {
        StartTransfer,
        AckStartTransfer,
        PauseTransfer,
        AckPauseTransfer,
        ResumeTransfer,
        AckResumeTransfer,
        CancelTransfer,
        AckCancelTransfer
    }
    public static class TransferControlMessageExtensions
    {
        public static readonly Dictionary<TransferControlMessage, string> _messageMap = new()
        {
            { TransferControlMessage.StartTransfer, "START_TRANSFER" },
            { TransferControlMessage.AckStartTransfer, "ACK_START_TRANSFER" },
            { TransferControlMessage.PauseTransfer, "PAUSE_TRANSFER" },
            { TransferControlMessage.AckPauseTransfer, "ACK_PAUSE_TRANSFER" },
            { TransferControlMessage.ResumeTransfer, "RESUME_TRANSFER" },
            { TransferControlMessage.AckResumeTransfer, "ACK_RESUME_TRANSFER" },
            { TransferControlMessage.CancelTransfer, "CANCEL_TRANSFER" },
            { TransferControlMessage.AckCancelTransfer, "ACK_CANCEL_TRANSFER" }
        };

        private static readonly Dictionary<string, TransferControlMessage> _reverseMessageMap =
             _messageMap.ToDictionary(x => x.Value, x => x.Key);

        public static string ToMessageString(this TransferControlMessage message) =>
            _messageMap.TryGetValue(message, out var value)
                ? value
                : throw new ArgumentException($"Unknown control message: {message}");

        public static TransferControlMessage ParseControlMessage(string message) =>
            _reverseMessageMap.TryGetValue(message, out var value)
                ? value
                : throw new ArgumentException($"Unknown message string: {message}");
    }
}

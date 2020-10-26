namespace Statyk
{
    public class StatykMessage
    {
        public string Message { get; private set; }
        public byte[] RawMessage { get; private set; }
        public bool IsBinaryMessage { get; private set; }

        public StatykMessage(string message, byte[] rawMessage, bool isBinaryMessage)
        {
            Message = message;
            RawMessage = rawMessage;
            IsBinaryMessage = isBinaryMessage;
        }
    }
}

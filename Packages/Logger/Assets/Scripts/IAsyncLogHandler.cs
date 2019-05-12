namespace Game.Util
{
    public interface IAsyncLogHandler
    {
        void Output(LogLevel logLevel, string formattedMessage);
    }
}

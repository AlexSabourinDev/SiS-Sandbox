namespace Game.Util
{
    // note: Order matters here as we use comparison 'a < b' for checking if a log level is enabled.
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}

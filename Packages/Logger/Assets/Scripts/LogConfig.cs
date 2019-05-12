namespace Game.Util
{
    public static class LogConfig
    {
        /// <summary>
        /// If false the log will create and overwrite any existing log.
        /// </summary>
        public static bool AppendLogFile { get; set; } = true;
        /// <summary>
        /// If true all log files (those within the logging directory with extension .log) will be destroyed.
        /// </summary>
        public static bool ClearOldLogFiles { get; set; } = true;
        /// <summary>
        /// Logging directory relative to the working path. (Usually where you run the executable)
        /// </summary>
        public static string LogDirectory { get; set; } = "";
        /// <summary>
        /// Whether or not to include the date with the log format
        /// </summary>
        public static bool FormatDate { get; set; } = false;
        /// <summary>
        /// Whether or not to include the time with the log format
        /// </summary>
        public static bool FormatTime { get; set; } = true;
        /// <summary>
        /// Whether or not to include the filename with the log format
        /// </summary>
        public static bool FormatFile { get; set; } = true;
        /// <summary>
        /// Whether or not to output to a log file
        /// </summary>
        public static bool OutputFile { get; set; } = true;
        /// <summary>
        /// Whether or not to output to the editor (Visual Studio Output Window)
        /// </summary>
        public static bool OutputEditor { get; set; } = false;
        /// <summary>
        /// Whether or not to output to the std output.
        /// </summary>
        public static bool OutputStd { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool OutputUnity { get; set; } = true;
        /// <summary>
        /// The level of logging we wish to actually output, anything below will be omitted.
        /// </summary>
        public static LogLevel LoggingLevel { get; set; } = LogLevel.Info;
    }
}

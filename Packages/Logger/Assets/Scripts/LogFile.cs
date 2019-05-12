using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Game.Util
{
    /// <summary>
    /// Wrapper around FileStream to ease log file creation.
    /// </summary>
    public class LogFile : IDisposable
    {
        private FileStream m_FileStream = null;
        private bool       m_Disposed = false;
        public string      LogDirectory { get; private set; } = string.Empty;

        public LogFile(string logDirectory, bool append = true, bool clearExpiredLogs = false)
        {
            LogDirectory = logDirectory;
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), logDirectory);
            bool pathExists = Directory.Exists(fullPath);
            if (!pathExists)
            {
                pathExists = Directory.CreateDirectory(fullPath).Exists;
            }

            DateTime now = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append("Log-")
                .Append(now.Year).Append('-')
                .Append(now.Month).Append('-')
                .Append(now.Day).Append(".log");

            string filePath = Path.Combine(fullPath, sb.ToString());

            if (clearExpiredLogs)
            {
                string[] files = Directory.GetFiles(fullPath);
                for (int i = 0, length = files.Length; i < length; ++i)
                {
                    if (Path.GetExtension(files[i]) == ".log" && files[i] != filePath)
                    {
                        File.Delete(files[i]);
                    }
                }
            }

            if (pathExists)
            {
                m_FileStream = File.Open(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write);
            }
        }

        public async Task WriteAsync(string message)
        {
            if (m_FileStream == null)
            {
                throw new InvalidOperationException("Missing filestream. Was 'Close' called?");
            }

            byte[] data = Encoding.UTF8.GetBytes(message);
            await m_FileStream.WriteAsync(data, 0, data.Length);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            if (disposing)
            {
                m_FileStream.Dispose();
                m_FileStream = null;
            }
            m_Disposed = true;
        }
    }
}

using Pree.CSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Pree.Services
{
    class LogService : Process
    {
        private StreamWriter _logFileWriter;

        public void OpenFile(string logFileName)
        {
            Enqueue(() => OnOpenFile(logFileName));
        }

        public void CloseFile()
        {
            Enqueue(() => OnCloseFile());
        }

        public void WriteSegment(TimeSpan startTime, TimeSpan duration)
        {
            Enqueue(() => OnWriteSegment(startTime, duration));
        }

        private void OnOpenFile(string logFileName)
        {
            if (_logFileWriter == null)
                _logFileWriter = new StreamWriter(logFileName);
        }

        private void OnCloseFile()
        {
            if (_logFileWriter != null)
            {
                _logFileWriter.Dispose();
                _logFileWriter = null;
            }
        }

        private void OnWriteSegment(TimeSpan startTime, TimeSpan duration)
        {
            if (_logFileWriter != null)
            {
                _logFileWriter.WriteLine("{0}, {1}", startTime, duration);
            }
        }
    }
}

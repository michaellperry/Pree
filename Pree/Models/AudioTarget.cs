using NAudio.Wave;
using System.IO;
using UpdateControls.Fields;

namespace Pree.Models
{
    class AudioTarget
    {
        private Independent<bool> _isOpen = new Independent<bool>();
        private WaveFileWriter _writer;

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public Stream Stream
        {
            get { return _writer; }
        }

        public void OpenFile(string destination)
        {
            if (_writer == null)
            {
                using (var waveIn = new WaveIn())
                {
                    _writer = new WaveFileWriter(destination, waveIn.WaveFormat);
                }
            }

            _isOpen.Value = true;
        }

        public void CloseFile()
        {
            _isOpen.Value = false;

            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }
    }
}

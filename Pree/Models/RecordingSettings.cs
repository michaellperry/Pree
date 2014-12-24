using NAudio.Wave;
using UpdateControls.Fields;

namespace Pree.Models
{
    public class RecordingSettings
    {
        private Independent<int> _deviceIndex = new Independent<int>();
        private Independent<int> _channels = new Independent<int>();
        private Independent<int> _bitsPerSample = new Independent<int>();
        private Independent<int> _sampleRate = new Independent<int>();

        public RecordingSettings()
        {
            using (var waveIn = new WaveIn())
            {
                _deviceIndex.Value = waveIn.DeviceNumber;
                _channels.Value = waveIn.WaveFormat.Channels;
                _bitsPerSample.Value = 32;
                _sampleRate.Value = 44100;
            }
        }

        public int DeviceIndex
        {
            get { return _deviceIndex; }
            set { _deviceIndex.Value = value; }
        }

        public int Channels
        {
            get { return _channels; }
            set { _channels.Value = value; }
        }

        public int SampleRate
        {
            get { return _sampleRate; }
            set { _sampleRate.Value = value; }
        }
    }
}

using NAudio.Wave;
using UpdateControls.Fields;

namespace Pree.Models
{
    public class RecordingSettings
    {
        private Independent<int> _deviceIndex = new Independent<int>();
        private Independent<int> _channels = new Independent<int>();
        private Independent<int> _sampleRate = new Independent<int>();
        private Independent<int> _trimMilliseconds = new Independent<int>();

        public RecordingSettings()
        {
            using (var waveIn = new WaveIn())
            {
                _deviceIndex.Value = waveIn.DeviceNumber;
                _channels.Value = waveIn.WaveFormat.Channels;
                _sampleRate.Value = 44100;
                _trimMilliseconds.Value = 400;
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

        public int TrimMilliseconds
        {
            get { return _trimMilliseconds; }
            set { _trimMilliseconds.Value = value; }
        }

        public WaveFormat CreateWaveFormat()
        {
            return WaveFormat.CreateCustomFormat(
                WaveFormatEncoding.Pcm,
                SampleRate,
                Channels,
                2 * SampleRate * Channels,
                2,
                16);
        }
    }
}

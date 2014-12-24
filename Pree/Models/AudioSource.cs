using NAudio.Wave;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using UpdateControls.Fields;

namespace Pree.Models
{
    class AudioSource
    {
        private DateTime _sessionStart;
        private TimeSpan _clipStart;

        private Independent<bool> _recording = new Independent<bool>();
        private Independent<float> _amplitude = new Independent<float>();
        private Independent<TimeSpan> _recordingTime = new Independent<TimeSpan>(TimeSpan.Zero);

        private IWaveIn _waveIn;
        private ISampleProvider _sampleProvider;
        private MemoryStream _content;
        private float[] _samples;

        public void BeginSession()
        {
            _sessionStart = DateTime.Now;
        }

        public bool Recording
        {
            get { return _recording; }
        }

        public float Amplitude
        {
            get
            {
                lock (this)
                {
                    return _amplitude;
                }
            }
            private set
            {
                lock (this)
                {
                    _amplitude.Value = value;
                }
            }
        }

        public TimeSpan RecordingTime
        {
            get { return _recordingTime.Value; }
        }

        public void StartRecording(RecordingSettings recordingSettings)
        {
            Contract.Requires(!Recording);
            Contract.Ensures(Recording);

            _waveIn = new WaveIn();
            _waveIn.WaveFormat = new WaveFormat(
                recordingSettings.SampleRate,
                recordingSettings.BitsPerSample,
                recordingSettings.Channels);
            WaveInProvider waveInProvider = new WaveInProvider(_waveIn);
            _sampleProvider = waveInProvider.ToSampleProvider();

            _waveIn.DataAvailable += DataAvailable;

            _content = new MemoryStream();
            _waveIn.StartRecording();

            _clipStart = DateTime.Now - _sessionStart;

            _recording.Value = true;
        }

        public Clip StopRecording()
        {
            Contract.Requires(Recording);
            Contract.Ensures(!Recording);

            _recording.Value = false;

            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.DataAvailable -= DataAvailable;
                _waveIn.Dispose();
                _waveIn = null;
            }
            _sampleProvider = null;

            Amplitude = 0.0f;

            Clip clip = new Clip(_content, _clipStart, _recordingTime.Value);
            _content = null;
            _recordingTime.Value = TimeSpan.Zero;
            return clip;
        }

        private void DataAvailable(object sender, WaveInEventArgs e)
        {
            _content.Write(e.Buffer, 0, e.BytesRecorded);

            UpdateAmplitude(e.BytesRecorded);
        }

        private void UpdateAmplitude(int bytesRecorded)
        {
            if (bytesRecorded == 0)
                return;

            int sampleCount = bytesRecorded * 8 / _waveIn.WaveFormat.BitsPerSample;
            if (_samples == null || _samples.Length < sampleCount)
                _samples = new float[sampleCount];
            int actualSampleCount = _sampleProvider.Read(_samples, 0, sampleCount);

            if (actualSampleCount == 0)
                return;

            _recordingTime.Value += TimeSpan.FromSeconds(
                (double)actualSampleCount / (double)_waveIn.WaveFormat.SampleRate);

            float amplitude = _samples
                .Take(actualSampleCount)
                .Select(s => Math.Abs(s))
                .Max();

            Amplitude = amplitude;
        }
    }
}

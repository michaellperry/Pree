using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using UpdateControls.Fields;

namespace Pree.Models
{
    class AudioSource
    {
        private Independent<bool> _recording = new Independent<bool>();
        private Independent<float> _amplitude = new Independent<float>();
        private Independent<TimeSpan> _elapsedTime = new Independent<TimeSpan>();
        private Independent<TimeSpan> _recordingTime = new Independent<TimeSpan>(TimeSpan.Zero);

        private IWaveIn _waveIn;
        private ISampleProvider _sampleProvider;
        private MemoryStream _clip;
        private float[] _samples;

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

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime.Value + _recordingTime.Value; }
        }

        public void Reset()
        {
            _elapsedTime.Value = TimeSpan.Zero;
        }

        public void StartRecording(RecordingSettings recordingSettings)
        {
            _waveIn = new WaveIn();
            _waveIn.WaveFormat = new WaveFormat(
                recordingSettings.SampleRate,
                recordingSettings.BitsPerSample,
                recordingSettings.Channels);
            WaveInProvider waveInProvider = new WaveInProvider(_waveIn);
            _sampleProvider = waveInProvider.ToSampleProvider();

            _waveIn.DataAvailable += DataAvailable;

            _clip = new MemoryStream();
            _waveIn.StartRecording();

            _recording.Value = true;
        }

        public void StopRecording()
        {
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
        }

        public long BytesAvailable
        {
            get { return _clip.Length; }
        }

        public void WriteClipTo(Stream writer)
        {
            _clip.WriteTo(writer);
            _elapsedTime.Value += _recordingTime.Value;
        }

        public void CloseClip()
        {
            _clip.Close();
            _recordingTime.Value = TimeSpan.Zero;
        }

        private void DataAvailable(object sender, WaveInEventArgs e)
        {
            _clip.Write(e.Buffer, 0, e.BytesRecorded);

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

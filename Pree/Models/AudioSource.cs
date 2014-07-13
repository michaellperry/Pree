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
        private Independent<DateTime> _recordingStarted = new Independent<DateTime>();

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

        public TimeSpan GetElapsedTime(DateTime now)
        {
            if (_recording.Value)
            {
                return _elapsedTime.Value + (now - _recordingStarted.Value);
            }
            else
            {
                return _elapsedTime.Value;
            }
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
            _recordingStarted.Value = DateTime.Now;
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

        public void WriteClipTo(Stream writer)
        {
            _clip.WriteTo(writer);
            _elapsedTime.Value += DateTime.Now - _recordingStarted.Value;
        }

        public void CloseClip()
        {
            _clip.Close();
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

            float amplitude = _samples
                .Take(actualSampleCount)
                .Select(s => Math.Abs(s))
                .Max();

            Amplitude = amplitude;
        }
    }
}

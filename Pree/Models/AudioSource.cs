﻿using NAudio.Wave;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using UpdateControls.Fields;

namespace Pree.Models
{
    class AudioSource
    {
        private readonly RecordingSettings _recordingSettings;

        private DateTime _sessionStart;
        private TimeSpan _clipStart;

        private Independent<bool> _recording = new Independent<bool>();
        private Independent<float> _amplitude = new Independent<float>();
        private Independent<TimeSpan> _recordingTime = new Independent<TimeSpan>(TimeSpan.Zero);
        private Independent<Exception> _lastException = new Independent<Exception>();

        private WaveIn _waveIn;
        private ISampleProvider _sampleProvider;
        private MemoryStream _content;
        private float[] _samples;
        
        public AudioSource(RecordingSettings recordingSettings)
        {
            _recordingSettings = recordingSettings;
        }

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
            get
            {
                TimeSpan recordingTime = _recordingTime.Value -
                    TimeSpan.FromMilliseconds(2 * _recordingSettings.TrimMilliseconds);
                return recordingTime < TimeSpan.Zero
                    ? TimeSpan.Zero
                    : recordingTime;
            }
        }

        public Exception LastException => _lastException;

        public void StartRecording(RecordingSettings recordingSettings)
        {
            _lastException.Value = null;
            try
            {
                Contract.Requires(!Recording);
                Contract.Ensures(Recording);

                _waveIn = new WaveIn();
                _waveIn.DeviceNumber = recordingSettings.DeviceIndex;
                _waveIn.WaveFormat = recordingSettings.CreateWaveFormat();

                WaveInProvider waveInProvider = new WaveInProvider(_waveIn);
                _sampleProvider = waveInProvider.ToSampleProvider();

                _waveIn.DataAvailable += DataAvailable;

                _content = new MemoryStream();
                _waveIn.StartRecording();

                _clipStart = DateTime.Now - _sessionStart;

                _recording.Value = true;
            }
            catch (Exception ex)
            {
                _lastException.Value = ex;
            }
        }

        public Clip StopRecording()
        {
            Contract.Requires(Recording);
            Contract.Ensures(!Recording);

            _recording.Value = false;

            if (_waveIn != null)
            {
                _lastException.Value = null;
                try
                {
                    _waveIn.StopRecording();
                    _waveIn.DataAvailable -= DataAvailable;
                    _waveIn.Dispose();
                }
                catch (Exception ex)
                {
                    _lastException.Value = ex;
                }
                _waveIn = null;
            }
            _sampleProvider = null;

            Amplitude = 0.0f;

            Clip clip = new Clip(_content, _clipStart, RecordingTime);
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
                (double)actualSampleCount / (double)_waveIn.WaveFormat.SampleRate / _waveIn.WaveFormat.Channels);

            float amplitude = _samples
                .Take(actualSampleCount)
                .Select(s => Math.Abs(s))
                .Max();

            Amplitude = amplitude;
        }
    }
}

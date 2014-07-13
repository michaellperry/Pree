﻿using Microsoft.Win32;
using Pree.Models;
using System.Windows.Input;
using UpdateControls.XAML;
using System;
using UpdateControls.Fields;
using System.Windows.Threading;
using System.Collections.Generic;
using NAudio.Wave;

namespace Pree.ViewModels
{
    class RecorderViewModel
    {
        private readonly AudioSource _audioSource;
        private readonly AudioTarget _audioTarget;
        private readonly RecordingSettings _recordingSettings;

        private Independent<DateTime> _now = new Independent<DateTime>(DateTime.Now);
        private DispatcherTimer _timer;
        
        public RecorderViewModel(
            AudioSource audioSource,
            AudioTarget audioTarget,
            RecordingSettings recordingSettings)
        {
            _recordingSettings = recordingSettings;
            _audioSource = audioSource;
            _audioTarget = audioTarget;

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1.0)
            };
            _timer.Tick += TimerTick;
        }

        public IEnumerable<DeviceViewModel> Devices
        {
            get
            {
                for (int deviceIndex = 0; deviceIndex < WaveIn.DeviceCount; deviceIndex++)
                {
                    yield return GetDeviceViewModel(deviceIndex);
                }
            }
        }

        public DeviceViewModel SelectedDevice
        {
            get { return GetDeviceViewModel(_recordingSettings.DeviceIndex); }
            set
            {
                _recordingSettings.DeviceIndex = value.DeviceIndex;
                _recordingSettings.Channels = value.Channels;
            }
        }

        public int BitsPerSample
        {
            get { return _recordingSettings.BitsPerSample; }
            set { _recordingSettings.BitsPerSample = value; }
        }

        public int SampleRate
        {
            get { return _recordingSettings.SampleRate; }
            set { _recordingSettings.SampleRate = value; }
        }

        public ICommand Start
        {
            get
            {
                return MakeCommand
                    .When(() => !_audioTarget.IsOpen)
                    .Do(() => StartSession());
            }
        }

        public ICommand Stop
        {
            get
            {
                return MakeCommand
                    .When(() => _audioTarget.IsOpen)
                    .Do(() => StopSession());
            }
        }

        public ICommand Record
        {
            get
            {
                return MakeCommand
                    .When(() => _audioTarget.IsOpen && !_audioSource.Recording)
                    .Do(() => StartRecording());
            }
        }

        public ICommand Keep
        {
            get
            {
                return MakeCommand
                    .When(() => _audioTarget.IsOpen && _audioSource.Recording)
                    .Do(() => KeepClip());
            }
        }

        public ICommand RecordOrKeep
        {
            get
            {
                return MakeCommand
                    .When(() => _audioTarget.IsOpen)
                    .Do(() =>
                    {
                        if (!_audioSource.Recording)
                            StartRecording();
                        else
                            KeepClip();
                    });
            }
        }

        public ICommand Discard
        {
            get
            {
                return MakeCommand
                    .When(() => _audioTarget.IsOpen && _audioSource.Recording)
                    .Do(() => DiscardClip());
            }
        }

        public float Amplitude
        {
            get { return _audioSource.Amplitude; }
        }

        public string ElapsedTime
        {
            get { return String.Format(@"{0:hh\:mm\:ss}", _audioSource.GetElapsedTime(_now.Value)); }
        }

        public void Closing()
        {
            StopSession();

            _timer.Tick -= TimerTick;
        }

        private static DeviceViewModel GetDeviceViewModel(int deviceIndex)
        {
            return new DeviceViewModel(deviceIndex, WaveIn.GetCapabilities(deviceIndex));
        }

        private void StartSession()
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                OverwritePrompt = true,
                DefaultExt = "wav",
                Filter = "Wave files (*.wav)|*.wav|All files (*.*)|*.*"
            };
            bool? result = dialog.ShowDialog();

            if (result ?? false)
            {
                _audioTarget.OpenFile(dialog.FileName, _recordingSettings);
            }
        }

        private void StopSession()
        {
            if (_audioSource.Recording)
                _audioSource.StopRecording();
            _audioSource.Reset();

            _audioTarget.CloseFile();
        }

        private void KeepClip()
        {
            _audioSource.StopRecording();

            _audioSource.WriteClipTo(_audioTarget.Stream);

            _audioSource.CloseClip();

            _timer.Stop();
        }

        private void DiscardClip()
        {
            _audioSource.StopRecording();

            _audioSource.CloseClip();

            _timer.Stop();
        }

        private void StartRecording()
        {
            _audioSource.StartRecording(_recordingSettings);

            _now.Value = DateTime.Now;
            _timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _now.Value = DateTime.Now;
        }
    }
}

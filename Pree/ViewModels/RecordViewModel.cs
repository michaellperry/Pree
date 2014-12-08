using Pree.Models;
using System;
using System.Windows.Input;
using UpdateControls.XAML;

namespace Pree.ViewModels
{
    class RecordViewModel : IContentViewModel
    {
        private readonly AudioSource _audioSource;
        private readonly AudioTarget _audioTarget;
        private readonly AudioFilter _audioFilter;
        private readonly RecordingSettings _recordingSettings;
        private readonly Timer _timer;

        public RecordViewModel(
            AudioSource audioSource,
            AudioTarget audioTarget,
            AudioFilter audioFilter,
            RecordingSettings recordingSettings,
            Timer timer)
        {
            _audioSource = audioSource;
            _audioTarget = audioTarget;
            _audioFilter = audioFilter;
            _recordingSettings = recordingSettings;
            _timer = timer;
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
            get { return String.Format(@"{0:hh\:mm\:ss}", _audioSource.ElapsedTime); }
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

            long bytesAvailable = _audioSource.BytesAvailable;
            using (var filterStream = _audioFilter.OpenStream(_audioTarget.Stream, bytesAvailable))
            {
                _audioSource.WriteClipTo(filterStream);
            }

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

            _timer.Start();
        }
    }
}

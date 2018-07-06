using Pree.Models;
using System;
using System.IO;
using System.Windows.Input;
using UpdateControls.XAML;

namespace Pree.ViewModels
{
    class RecordViewModel : IContentViewModel
    {
        private readonly AudioSource _audioSource;
        private readonly RecordingSession _recordingSession;
        
        public RecordViewModel(
            AudioSource audioSource,
            RecordingSession recordingSession)
        {
            _audioSource = audioSource;
            _recordingSession = recordingSession;
        }

        public ICommand Done
        {
            get
            {
                return MakeCommand
                    .Do(() => _recordingSession.EndSession());
            }
        }

        public ICommand Record
        {
            get
            {
                return MakeCommand
                    .When(() => !_recordingSession.Recording)
                    .Do(() => _recordingSession.StartRecording());
            }
        }

        public ICommand Stop
        {
            get
            {
                return MakeCommand
                    .When(() => _recordingSession.Recording)
                    .Do(() => _recordingSession.StopRecording());
            }
        }

        public ICommand Keep
        {
            get
            {
                return MakeCommand
                    .When(() => !_recordingSession.Recording && !_recordingSession.ShouldKeep)
                    .Do(() => _recordingSession.ShouldKeep = true);
            }
        }

        public ICommand Discard
        {
            get
            {
                return MakeCommand
                    .When(() => !_recordingSession.Recording && _recordingSession.ShouldKeep)
                    .Do(() => _recordingSession.ShouldKeep = false);
            }
        }

        public float Amplitude
        {
            get { return _audioSource.Amplitude; }
        }

        public string ElapsedTime
        {
            get { return String.Format(@"{0:hh\:mm\:ss}", _recordingSession.ElapsedTime); }
        }

        public string LastError => _recordingSession.LastException?.Message;
    }
}

using Pree.Models;
using System.Windows.Input;
using UpdateControls.XAML;

namespace Pree.ViewModels
{
    class MainViewModel
    {
        private readonly AudioSource _audioSource;
        private readonly AudioFilter _audioFilter;
        private readonly RecordingSession _recordingSession;
        private readonly RecordingSettings _recordingSettings;
        private readonly Timer _timer;

        private readonly IContentViewModel _startViewModel;
        private readonly IContentViewModel _recordViewModel;

        public MainViewModel(
            AudioSource audioSource,
            AudioFilter audioFilter,
            RecordingSettings recordingSettings,
            Timer timer,
            RecordingSession recordingSession)
        {
            _audioSource = audioSource;
            _audioFilter = audioFilter;
            _recordingSettings = recordingSettings;
            _timer = timer;
            _recordingSession = recordingSession;

            _startViewModel = new StartViewModel(
                _recordingSettings,
                _recordingSession);
            _recordViewModel = new RecordViewModel(
                _audioSource,
                _recordingSession);
        }

        public IContentViewModel Content
        {
            get
            {
                if (!_recordingSession.IsActive)
                    return _startViewModel;
                else
                    return _recordViewModel;
            }
        }

        public ICommand Record
        {
            get
            {
                return MakeCommand
                    .When(() => _recordingSession.IsActive && !_recordingSession.Recording)
                    .Do(() => _recordingSession.StartRecording());
            }
        }

        public ICommand StopOrDiscard
        {
            get
            {
                return MakeCommand
                    .Do(() =>
                    {
                        if (_recordingSession.Recording)
                            _recordingSession.StopRecording();
                        else
                            _recordingSession.ShouldKeep = !_recordingSession.ShouldKeep;
                    });
            }
        }

        public void Closing()
        {
            EndSession();

            _timer.Dispose();
        }

        private void EndSession()
        {
            if (_recordingSession.IsActive)
                _recordingSession.EndSession();
        }
    }
}

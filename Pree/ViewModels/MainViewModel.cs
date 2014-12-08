using Pree.Models;

namespace Pree.ViewModels
{
    class MainViewModel
    {
        private readonly AudioSource _audioSource;
        private readonly AudioTarget _audioTarget;
        private readonly AudioFilter _audioFilter;
        private readonly RecordingSettings _recordingSettings;
        private readonly Timer _timer;

        private readonly IContentViewModel _startViewModel;
        private readonly IContentViewModel _recordViewModel;

        public MainViewModel(
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

            _startViewModel = new StartViewModel(
                _audioSource,
                _audioTarget,
                _audioFilter,
                _recordingSettings);
            _recordViewModel = new RecordViewModel(
                _audioSource,
                _audioTarget,
                _audioFilter,
                _recordingSettings,
                _timer);
        }

        public IContentViewModel Content
        {
            get
            {
                if (!_audioTarget.IsOpen)
                    return _startViewModel;
                else
                    return _recordViewModel;
            }
        }

        public void Closing()
        {
            StopSession();

            _timer.Dispose();
        }

        private void StopSession()
        {
            if (_audioSource.Recording)
                _audioSource.StopRecording();
            _audioSource.Reset();

            _audioTarget.CloseFile();
        }
    }
}

using Pree.Models;
using Pree.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdateControls.XAML;

namespace Pree.ViewModels
{
    class ViewModelLocator : ViewModelLocatorBase
    {
        private AudioSource _audioSource = new AudioSource();
        private AudioTarget _audioTarget;
        private AudioFilter _audioFilter;
        private RecordingSettings _recordingSettings = new RecordingSettings();
        private Timer _timer = new Timer();
        private AudioFileService _audioFileService;
        private AudioFileService _timelineFileService;
        private RecordingSession _recordingSession;

        public ViewModelLocator()
        {
            _audioFilter = new AudioFilter(_recordingSettings);
            _audioFileService = new AudioFileService(_audioFilter);
            _timelineFileService = new AudioFileService(_audioFilter);
            _audioTarget = new AudioTarget(_audioFileService, _timelineFileService);

            _recordingSession = new RecordingSession(
                _audioSource,
                _audioTarget,
                _recordingSettings,
                _timer);
        }

        public object Main
        {
            get
            {
                return ViewModel(() => new MainViewModel(
                    _audioSource,
                    _audioFilter,
                    _recordingSettings,
                    _timer,
                    _recordingSession));
            }
        }
    }
}

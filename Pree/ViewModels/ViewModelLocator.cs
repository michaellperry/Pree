using Pree.Models;
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
        private AudioTarget _audioTarget = new AudioTarget();
        private AudioFilter _audioFilter;
        private RecordingSettings _recordingSettings = new RecordingSettings();

        public ViewModelLocator()
        {
            _audioFilter = new AudioFilter(_recordingSettings);
        }

        public object Recorder
        {
            get
            {
                return ViewModel(() => new RecorderViewModel(
                    _audioSource,
                    _audioTarget,
                    _audioFilter,
                    _recordingSettings));
            }
        }
    }
}

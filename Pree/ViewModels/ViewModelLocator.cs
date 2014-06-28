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
        private MixerSelection _mixerSelection = new MixerSelection();

        public object Main
        {
            get { return ViewModel(() => new MainViewModel(_mixerSelection)); }
        }

        public object Recorder
        {
            get { return ViewModel(() => new RecorderViewModel(_audioSource, _audioTarget)); }
        }
    }
}

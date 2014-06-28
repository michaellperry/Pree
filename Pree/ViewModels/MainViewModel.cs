using NAudio.Mixer;
using Pree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pree.ViewModels
{
    class MainViewModel
    {
        private readonly MixerSelection _selection;

        public MainViewModel(MixerSelection seletion)
        {
            _selection = seletion;
        }

        public IEnumerable<MixerViewModel> Mixers
        {
            get
            {
                return from mixer in Mixer.Mixers select new MixerViewModel(mixer);
            }
        }

        public MixerViewModel SelectedMixer
        {
            get { return _selection.SelectedMixer == null ? null : new MixerViewModel(_selection.SelectedMixer); }
            set { _selection.SelectedMixer = value == null ? null : value.Mixer; }
        }

        public IEnumerable<LineViewModel> Destinations
        {
            get
            {
                if (_selection.SelectedMixer == null)
                    return Enumerable.Empty<LineViewModel>();

                return from destination in _selection.SelectedMixer.Destinations
                       select new LineViewModel(destination);
            }
        }
    }
}

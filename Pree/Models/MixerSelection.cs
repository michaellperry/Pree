using NAudio.Mixer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdateControls.Fields;

namespace Pree.Models
{
    class MixerSelection
    {
        Independent<Mixer> _selectedMixer = new Independent<Mixer>();

        public Mixer SelectedMixer
        {
            get { return _selectedMixer; }
            set { _selectedMixer.Value = value; }
        }
    }
}

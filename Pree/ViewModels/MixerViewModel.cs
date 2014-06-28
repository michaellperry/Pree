using NAudio.Mixer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pree.ViewModels
{
    class MixerViewModel
    {
        private readonly Mixer _mixer;

        public MixerViewModel(Mixer mixer)
        {
            _mixer = mixer;
        }

        internal Mixer Mixer
        {
            get { return _mixer; }
        }

        public string Name
        {
            get { return _mixer.Name; }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            var that = obj as MixerViewModel;
            if (that == null)
                return false;

            return Object.Equals(this._mixer, that._mixer);
        }

        public override int GetHashCode()
        {
            return _mixer.GetHashCode();
        }
    }
}

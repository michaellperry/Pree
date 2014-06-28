using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Mixer;

namespace Pree.ViewModels
{
    class LineViewModel
    {
        private readonly MixerLine _line;

        public LineViewModel(MixerLine line)
        {
            _line = line;
        }

        public string Name
        {
            get { return _line.Name; }
        }
    }
}

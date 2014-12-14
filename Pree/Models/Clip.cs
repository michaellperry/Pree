using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pree.Models
{
    class Clip
    {
        private readonly MemoryStream _content;
        private readonly TimeSpan _duration;

        public Clip(MemoryStream content, TimeSpan duration)
        {
            _content = content;
            _duration = duration;
        }

        public MemoryStream Content
        {
            get { return _content; }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
        }
    }
}

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
        private readonly TimeSpan _startTime;
        private readonly TimeSpan _duration;

        public Clip(MemoryStream content, TimeSpan startTime, TimeSpan duration)
        {
            _content = content;
            _startTime = startTime;
            _duration = duration;
        }

        public MemoryStream Content
        {
            get { return _content; }
        }

        public TimeSpan StartTime
        {
            get { return _startTime; }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pree.Camtasia
{
    [DebuggerDisplay("\\{ Start = {Start}, Duration = {Duration} \\}")]
    public class Segment : IEquatable<Segment>
    {
        private readonly DateTime _start;
        private readonly DateTime _duration;

        private readonly int start;
        private readonly int duration;
        private readonly bool isSilent;

        public Segment(DateTime start, DateTime duration, bool isSilent)
        {
            _start = start;
            _duration = duration;
            this.start = (int)(start.TimeOfDay.TotalSeconds * 30.0 + 0.5);
            this.duration = (int)(duration.TimeOfDay.TotalSeconds * 30.0 + 0.5);
            this.isSilent = isSilent;
        }

        public Segment(int start, int duration, bool isSilent)
        {
            this.start = start;
            this.duration = duration;
            this.isSilent = isSilent;
        }

        public override bool Equals(object obj)
        {
            if (obj is Segment)
                return Equals((Segment)obj);
            return false;
        }
        public bool Equals(Segment obj)
        {
            if (obj == null)
                return false;
            if (start != obj.start)
                return false;
            if (duration != obj.duration)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= start;
            hash ^= duration;
            return hash;
        }
        public override string ToString()
        {
            return String.Format("{{ Start = {0}, Duration = {1} }}", start, duration);
        }

        public DateTime Start
        {
            get
            {
                return _start;
            }
        }
        public DateTime Duration
        {
            get
            {
                return _duration;
            }
        }

        public int CamStart => start;
        public int CamDuration => duration;

        public bool IsSilent => isSilent;
    }
}

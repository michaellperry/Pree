using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pree.Camtasia
{
    [DebuggerDisplay("\\{ Start = {Start}, Duration = {Duration} \\}")]
    public class ContinuousSegment : IEquatable<ContinuousSegment>
    {
        private readonly DateTime _start;
        private readonly DateTime _duration;

        public ContinuousSegment(DateTime start, DateTime duration)
        {
            _start = start;
            _duration = duration;
        }

        public override bool Equals(object obj)
        {
            if (obj is ContinuousSegment)
                return Equals((ContinuousSegment)obj);
            return false;
        }
        public bool Equals(ContinuousSegment obj)
        {
            if (obj == null)
                return false;
            if (!EqualityComparer<DateTime>.Default.Equals(_start, obj._start))
                return false;
            if (!EqualityComparer<DateTime>.Default.Equals(_duration, obj._duration))
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= EqualityComparer<DateTime>.Default.GetHashCode(_start);
            hash ^= EqualityComparer<DateTime>.Default.GetHashCode(_duration);
            return hash;
        }
        public override string ToString()
        {
            return String.Format("{{ Start = {0}, Duration = {1} }}", _start, _duration);
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
    }
}

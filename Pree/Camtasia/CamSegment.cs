using System;
using System.Diagnostics;

namespace Pree.Camtasia
{
    public class CamSegment : IEquatable<CamSegment>
    {
        private readonly int start;
        private readonly int duration;
        private readonly bool isSilent;

        public CamSegment(int start, int duration, bool isSilent)
        {
            this.start = start;
            this.duration = duration;
            this.isSilent = isSilent;
        }

        public override bool Equals(object obj)
        {
            if (obj is CamSegment)
                return Equals((CamSegment)obj);
            return false;
        }
        public bool Equals(CamSegment obj)
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

        public int CamStart => start;
        public int CamDuration => duration;

        public bool IsSilent => isSilent;
    }
}

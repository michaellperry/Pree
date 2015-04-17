using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pree.Camtasia
{
    [DebuggerDisplay("\\{ id = {id}, src = {src} \\}")]
    public class Source : IEquatable<Source>
    {
        private readonly int _id;
        private readonly string _src;

        public Source(int id, string src)
        {
            _id = id;
            _src = src;
        }

        public override bool Equals(object obj)
        {
            if (obj is Source)
                return Equals((Source)obj);
            return false;
        }
        public bool Equals(Source obj)
        {
            if (obj == null)
                return false;
            if (!EqualityComparer<int>.Default.Equals(_id, obj._id))
                return false;
            if (!EqualityComparer<string>.Default.Equals(_src, obj._src))
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= EqualityComparer<int>.Default.GetHashCode(_id);
            hash ^= EqualityComparer<string>.Default.GetHashCode(_src);
            return hash;
        }
        public override string ToString()
        {
            return String.Format("{{ id = {0}, src = {1} }}", _id, _src);
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }
        public string Src
        {
            get
            {
                return _src;
            }
        }
    }
}

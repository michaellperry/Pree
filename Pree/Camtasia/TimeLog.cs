﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Pree.Camtasia
{
    public class TimeLog
    {
        private readonly List<Segment> _segments;

        private TimeLog(List<Segment> segments)
        {
            _segments = segments;
        }

        public static TimeLog Load(string filename)
        {
            List<Segment> segments = new List<Segment>();

            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var times = line.Split(',')
                        .Select(s => s.Trim())
                        .Select(s => DateTime.Parse(s))
                        .ToArray();
                    segments.Add(new Segment(times[0], times[1]));
                }
            }
            return new TimeLog(segments);
        }

        public IEnumerable<Segment> Segments
        {
            get { return _segments; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using System.IO;

namespace Pree.Camtasia
{
    public class CamProject
    {
        private XmlDocument _document;

        private int _nextId;

        private CamProject(XmlDocument document)
        {
            _document = document;

            _nextId = _document.SelectNodes("//CSMLData//*[@id]")
                .OfType<XmlNode>()
                .Select(n => int.Parse(GetAttribute(n, "id")))
                .Max();
        }

        public static CamProject Load(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            return new CamProject(document);
        }

        public void TrimAndWrite(string targetFilename)
        {
            var timeline = GetTimeline();
            if (timeline == null)
                throw new ApplicationException("The _time.wav file is not in the project.");

            TimeSpan offset = GetOffset(timeline.Id);
            var timelineFilename = timeline.Src;
            string logFilename = timelineFilename.Substring(0, timelineFilename.Length - "_time.wav".Length) + ".log";
            if (!File.Exists(logFilename))
                throw new ApplicationException(String.Format("The timeline log file {0} does not exist.", logFilename));

            TimeLog log = TimeLog.Load(logFilename);

            var offsetSegments = log.Segments
                .Select(s => new Segment(s.Start + offset, s.Duration));
            Trim(offsetSegments);
            Write(targetFilename);
        }

        public Source GetTimeline()
        {
            var sources = _document.SelectNodes("/Project_Data/CSMLData/GoProject/Project/SourceBin/Source")
                .OfType<XmlNode>()
                .Select(n => new Source(int.Parse(GetAttribute(n, "id")), GetAttribute(n, "src")));

            return sources.Where(s => s.Src.EndsWith("_time.wav")).FirstOrDefault();
        }

        public TimeSpan GetOffset(int id)
        {
            var clip = _document
                .SelectNodes(String.Format("//Medias/AMFile[@src={0}]", id))
                .OfType<XmlNode>()
                .FirstOrDefault();
            if (clip == null)
                throw new ApplicationException("The _time.wav file is not in the timeline.");

            var start = int.Parse(GetAttribute(clip, "start"));
            string ms = GetAttribute(clip, "mediaStart");
            var mediaStart = int.Parse(ms.Substring(0, ms.Length - "/1".Length));
            return TimeSpan.FromSeconds((start - mediaStart) / 30.0);
        }

        public int GetNextId()
        {
            return _nextId;
        }

        public void Trim(IEnumerable<Segment> segments)
        {
            TrimAudio(segments);
            TrimVideo(segments);
            TrimUnifiedMedia(segments);
        }

        private void TrimAudio(IEnumerable<Segment> segments)
        {
            var clips = _document.SelectNodes("//Medias/AMFile")
                .OfType<XmlNode>()
                .ToList();

            foreach (var clip in clips)
            {
                var parent = clip.ParentNode;
                int start = int.Parse(GetAttribute(clip, "start"));
                int duration = int.Parse(GetAttribute(clip, "duration"));
                string ms = GetAttribute(clip, "mediaStart");
                int mediaStart = int.Parse(ms.Substring(0, ms.Length - "/1".Length));

                parent.RemoveChild(clip);

                int targetStart = 0;
                foreach (var segment in segments)
                {
                    int segmentStart = (int)(segment.Start.TimeOfDay.TotalSeconds * 30.0 + 0.5);
                    int segmentEnd = (int)((segment.Start.TimeOfDay.TotalSeconds + segment.Duration.TimeOfDay.TotalSeconds) * 30.0 + 0.5);

                    if (segmentStart < start)
                        segmentStart = start;
                    if (segmentEnd > start + duration)
                        segmentEnd = start + duration;
                    int segmentDuration = segmentEnd - segmentStart;
                    if (segmentDuration > 0)
                    {
                        var newClip = clip.Clone();
                        SetAttribute(newClip, "id", _nextId.ToString());
                        SetAttribute(newClip, "start", targetStart.ToString());
                        SetAttribute(newClip, "duration", segmentDuration.ToString());
                        SetAttribute(newClip, "mediaStart", string.Format("{0}/1", mediaStart + segmentStart - start));
                        SetAttribute(newClip, "mediaDuration", string.Format("{0}/1", segmentDuration));
                        parent.AppendChild(newClip);

                        targetStart += segmentDuration;
                        _nextId++;
                    }
                }
            }
        }

        private void TrimVideo(IEnumerable<Segment> segments)
        {
            var clips = _document.SelectNodes("//Medias/ScreenVMFile")
                .OfType<XmlNode>()
                .ToList();

            foreach (var clip in clips)
            {
                var parent = clip.ParentNode;
                int start = int.Parse(GetAttribute(clip, "start"));
                int duration = int.Parse(GetAttribute(clip, "duration"));
                string ms = GetAttribute(clip, "mediaStart");
                int mediaStart = int.Parse(ms.Substring(0, ms.Length - "/1".Length));

                parent.RemoveChild(clip);

                int targetStart = 0;
                foreach (var segment in segments)
                {
                    int segmentStart = (int)(segment.Start.TimeOfDay.TotalSeconds * 30.0 + 0.5);
                    int segmentEnd = (int)((segment.Start.TimeOfDay.TotalSeconds + segment.Duration.TimeOfDay.TotalSeconds) * 30.0 + 0.5);

                    if (segmentStart < start)
                        segmentStart = start;
                    if (segmentEnd > start + duration)
                        segmentEnd = start + duration;
                    int segmentDuration = segmentEnd - segmentStart;
                    if (segmentDuration > 0)
                    {
                        var newClip = clip.Clone();
                        SetAttribute(newClip, "id", _nextId.ToString());
                        SetAttribute(newClip, "start", targetStart.ToString());
                        SetAttribute(newClip, "duration", segmentDuration.ToString());
                        SetAttribute(newClip, "mediaStart", string.Format("{0}/1", mediaStart + segmentStart - start));
                        SetAttribute(newClip, "mediaDuration", string.Format("{0}/1", segmentDuration));
                        parent.AppendChild(newClip);

                        targetStart += segmentDuration;
                        _nextId++;
                    }
                }
            }
        }

        private void TrimUnifiedMedia(IEnumerable<Segment> segments)
        {
            var clips = _document.SelectNodes("//Medias/UnifiedMedia")
                .OfType<XmlNode>()
                .ToList();

            foreach (var clip in clips)
            {
                var parent = clip.ParentNode;
                var amfile = clip.SelectSingleNode("AMFile");
                int start = int.Parse(GetAttribute(amfile, "start"));
                int duration = int.Parse(GetAttribute(amfile, "duration"));
                string ms = GetAttribute(amfile, "mediaStart");
                int mediaStart = int.Parse(ms.Substring(0, ms.Length - "/1".Length));

                parent.RemoveChild(clip);

                int targetStart = 0;
                foreach (var segment in segments)
                {
                    int segmentStart = (int)(segment.Start.TimeOfDay.TotalSeconds * 30.0 + 0.5);
                    int segmentEnd = (int)((segment.Start.TimeOfDay.TotalSeconds + segment.Duration.TimeOfDay.TotalSeconds) * 30.0 + 0.5);

                    if (segmentStart < start)
                        segmentStart = start;
                    if (segmentEnd > start + duration)
                        segmentEnd = start + duration;
                    int segmentDuration = segmentEnd - segmentStart;
                    if (segmentDuration > 0)
                    {
                        var newClip = clip.Clone();
                        var newAmfile = newClip.SelectSingleNode("AMFile");
                        SetAttribute(newAmfile, "id", _nextId.ToString());
                        SetAttribute(newAmfile, "start", targetStart.ToString());
                        SetAttribute(newAmfile, "duration", segmentDuration.ToString());
                        SetAttribute(newAmfile, "mediaStart", string.Format("{0}/1", mediaStart + segmentStart - start));
                        SetAttribute(newAmfile, "mediaDuration", string.Format("{0}/1", segmentDuration));
                        var newScreenvmfile = newClip.SelectSingleNode("ScreenVMFile");
                        if (newScreenvmfile != null)
                        {
                            SetAttribute(newScreenvmfile, "id", _nextId.ToString());
                            SetAttribute(newScreenvmfile, "start", targetStart.ToString());
                            SetAttribute(newScreenvmfile, "duration", segmentDuration.ToString());
                            SetAttribute(newScreenvmfile, "mediaStart", string.Format("{0}/1", mediaStart + segmentStart - start));
                            SetAttribute(newScreenvmfile, "mediaDuration", string.Format("{0}/1", segmentDuration));
                        }
                        var newVmfile = newClip.SelectSingleNode("VMFile");
                        if (newVmfile != null)
                        {
                            SetAttribute(newVmfile, "id", _nextId.ToString());
                            SetAttribute(newVmfile, "start", targetStart.ToString());
                            SetAttribute(newVmfile, "duration", segmentDuration.ToString());
                            SetAttribute(newVmfile, "mediaStart", string.Format("{0}/1", mediaStart + segmentStart - start));
                            SetAttribute(newVmfile, "mediaDuration", string.Format("{0}/1", segmentDuration));
                        }
                        parent.AppendChild(newClip);

                        targetStart += segmentDuration;
                        _nextId++;
                    }
                }
            }
        }

        public void Write(string filename)
        {
            using (var writer = XmlWriter.Create(filename))
            {
                _document.WriteTo(writer);
            }
        }

        private static string GetAttribute(XmlNode n, string name)
        {
            var attribute = n.Attributes
                .OfType<XmlAttribute>()
                .Where(a => a.Name == name)
                .Single();
            return attribute.Value;
        }

        private void SetAttribute(XmlNode n, string name, string value)
        {
            var attribute = n.Attributes
                .OfType<XmlAttribute>()
                .Where(a => a.Name == name)
                .Single();
            attribute.Value = value;
        }
    }
}

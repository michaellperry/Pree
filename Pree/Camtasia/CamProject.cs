using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;

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

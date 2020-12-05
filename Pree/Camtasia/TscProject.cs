using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Pree.Camtasia
{
    public class TscProject
    {
        private JObject _document;

        private int _nextId;

        private TscProject(JObject document)
        {
            _document = document;

            _nextId = _document.SelectTokens("$..id")
                .Where(token => token.Type == JTokenType.Integer)
                .Select(token => token.Value<int>())
                .Max();
        }

        public static TscProject Load(string filename)
        {
            using (var file = File.OpenText(filename))
            {
                var contents = file.ReadToEnd();
                Regex error = new Regex(@"""integratedLUFS"" : [0-9\-.]+e[0-9+]+");
                contents = error.Replace(contents, @"""integratedLUFS"" : 0");
                var document = (JObject)JToken.Parse(contents);
                return new TscProject(document);
            }
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
            var sources = _document
                .SelectTokens("$.sourceBin[*]")
                .OfType<JObject>()
                .Select(n => new Source((int)n["id"], (string)n["src"]));

            return sources.Where(s => s.Src.EndsWith("_time.wav")).FirstOrDefault();
        }

        public TimeSpan GetOffset(int id)
        {
            var clip = _document
                .SelectTokens("$.timeline.sceneTrack.scenes[*].csml.tracks[*].medias[*]")
                .OfType<JObject>()
                .Where(n => (string)n["_type"] == "AMFile" && (int)n["src"] == id)
                .FirstOrDefault();
            if (clip == null)
                throw new ApplicationException("The _time.wav file is not in the timeline.");

            var start = (int)clip["start"];
            var mediaStart = (int)clip["mediaStart"];
            return TimeSpan.FromSeconds((start - mediaStart) / 30.0);
        }

        public int GetNextId()
        {
            return _nextId;
        }

        public void Trim(IEnumerable<Segment> segments)
        {
            TrimAudio(segments);
            //TrimVideo(segments);
            TrimUnifiedMedia(segments);
        }

        private void TrimAudio(IEnumerable<Segment> segments)
        {
            var clips = _document
                .SelectTokens("$.timeline.sceneTrack.scenes[*].csml.tracks[*].medias[*]")
                .OfType<JObject>()
                .Where(n => (string)n["_type"] == "AMFile")
                .ToList();

            foreach (var clip in clips)
            {
                var parent = clip.Parent;
                int start = (int)clip["start"];
                int duration = (int)clip["duration"];
                int mediaStart = (int)clip["mediaStart"];

                clip.Remove();

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
                        var newClip = clip.DeepClone();
                        newClip["id"] = _nextId++;
                        newClip["start"] = targetStart;
                        newClip["duration"] = segmentDuration;
                        newClip["mediaStart"] = mediaStart + segmentStart - start;
                        newClip["mediaDuration"] = segmentDuration;
                        parent.Add(newClip);

                        targetStart += segmentDuration;
                    }
                }
            }
        }

        private void TrimUnifiedMedia(IEnumerable<Segment> segments)
        {
            var clips = _document
                .SelectTokens("$.timeline.sceneTrack.scenes[*].csml.tracks[*].medias[*]")
                .OfType<JObject>()
                .Where(n => (string)n["_type"] == "UnifiedMedia")
                .ToList();

            foreach (var clip in clips)
            {
                var parent = clip.Parent;
                var amfile = clip["audio"];
                int start = (int)clip["start"];
                int duration = (int)clip["duration"];
                int mediaStart = (int)clip["mediaStart"];

                clip.Remove();

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
                        var newClip = clip.DeepClone();
                        newClip["id"] = _nextId++;
                        newClip["start"] = targetStart;
                        newClip["duration"] = segmentDuration;
                        newClip["mediaStart"] = mediaStart + segmentStart - start;
                        newClip["mediaDuration"] = segmentDuration;

                        var newAmfile = newClip["audio"];
                        newAmfile["id"] = _nextId++;
                        newAmfile["start"] = targetStart;
                        newAmfile["duration"] = segmentDuration;
                        newAmfile["mediaStart"] = mediaStart + segmentStart - start;
                        newAmfile["mediaDuration"] = segmentDuration;

                        var newScreenvmfile = newClip["video"];
                        if (newScreenvmfile != null)
                        {
                            newScreenvmfile["id"] = _nextId++;
                            newScreenvmfile["start"] = targetStart;
                            newScreenvmfile["duration"] = segmentDuration;
                            newScreenvmfile["mediaStart"] = mediaStart + segmentStart - start;
                            newScreenvmfile["mediaDuration"] = segmentDuration;
                        }

                        parent.Add(newClip);

                        targetStart += segmentDuration;
                    }
                }
            }
        }

        public void Write(string filename)
        {
            using (var file = File.CreateText(filename))
            using (var writer = new JsonTextWriter(file))
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

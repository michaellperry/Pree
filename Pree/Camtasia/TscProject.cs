using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pree.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
            int camOffset = (int)(offset.TotalSeconds * 30.0 + 0.5);
            var timelineFilename = timeline.Src;
            string logFilename = timelineFilename.Substring(0, timelineFilename.Length - "_time.wav".Length) + ".log";
            if (!File.Exists(logFilename))
                throw new ApplicationException(String.Format("The timeline log file {0} does not exist.", logFilename));

            string compressedAudioFilename = timelineFilename.Substring(0, timelineFilename.Length - "_time.wav".Length) + ".wav";
            int? compressedAudio = FindAudio(compressedAudioFilename);

            TimeLog log = TimeLog.Load(logFilename);

            var silenceService = new SilenceService(timelineFilename);
            var offsetSegments = log.Segments
                .Select(s => new Segment(s.CamStart + camOffset, s.CamDuration, silenceService.IsSilent(s.Start, s.Duration)));
            if (compressedAudio == null)
            {
                TrimAudio(offsetSegments);
                TrimUnifiedMedia(offsetSegments);
            }
            else
            {
                var scenes = ConstructScenes(offsetSegments).ToImmutableList();
                RemoveAudioExcept((int)compressedAudio);
                TrimAndCompressUnifiedMedia(scenes);
                TrimAndCompressAudio(scenes);
            }
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

        public int? FindAudio(string source)
        {
            int? id = _document
                .SelectTokens("$.sourceBin[*]")
                .OfType<JObject>()
                .Where(n => (string)n["src"] == source)
                .Select(n => (int)n["id"])
                .FirstOrDefault();
            return id;
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

        private IEnumerable<Scene> ConstructScenes(IEnumerable<Segment> segments)
        {
            bool takeAudio = false;
            int position = 0;
            ImmutableList<Segment> audio = ImmutableList<Segment>.Empty;
            ImmutableList<Segment> video = ImmutableList<Segment>.Empty;
            foreach (var segment in segments)
            {
                if (takeAudio && segment.IsSilent)
                {
                    // Switching from audio to video.
                    // Start a new scene.
                    position += video.Sum(v => v.CamDuration);
                    if (audio.Any() && video.Any())
                    {
                        yield return new Scene(audio, video, position);
                    }
                    position += audio.Sum(a => a.CamDuration);
                    audio = ImmutableList<Segment>.Empty;
                    video = ImmutableList<Segment>.Empty;
                }
                if (segment.IsSilent)
                {
                    video = video.Add(segment);
                    takeAudio = false;
                }
                else
                {
                    audio = audio.Add(segment);
                    takeAudio = true;
                }
            }

            position += video.Sum(v => v.CamDuration);
            if (audio.Any() && video.Any())
            {
                yield return new Scene(audio, video, position);
            }
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
                    int segmentStart = segment.CamStart;
                    int segmentEnd = segment.CamStart + segment.CamDuration;

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

        private void TrimAndCompressAudio(IEnumerable<Scene> scenes)
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

                int sceneStart = 0;
                int segmentStart = 0;
                foreach (var scene in scenes)
                {
                    int sceneDuration = scene.Duration;
                    int targetStart = 0;
                    foreach (var segment in scene.AudioSegments)
                    {
                        int segmentDuration = segment.CamDuration;
                        if (segmentDuration > 0)
                        {
                            if (!segment.IsSilent)
                            {
                                var newClip = clip.DeepClone();
                                newClip["id"] = _nextId++;
                                newClip["start"] = sceneStart + targetStart;
                                newClip["duration"] = segmentDuration;
                                newClip["mediaStart"] = mediaStart + scene.Position + targetStart - start;
                                newClip["mediaDuration"] = segmentDuration;
                                parent.Add(newClip);
                            }

                            segmentStart += segmentDuration;
                            targetStart += segmentDuration;
                        }
                    }
                    sceneStart += sceneDuration;
                }
            }
        }

        private void RemoveAudioExcept(int id)
        {
            var clips = _document
                .SelectTokens("$.timeline.sceneTrack.scenes[*].csml.tracks[*].medias[*]")
                .OfType<JObject>()
                .Where(n => (string)n["_type"] == "AMFile" && (int)n["src"] != id)
                .ToList();

            foreach (var clip in clips)
            {
                clip.Remove();
            }

            var tracks = _document
                .SelectTokens("$.timeline.sceneTrack.scenes[*].csml.tracks[*]")
                .OfType<JObject>()
                .ToList();
            int trackIndex = 0;

            foreach (var track in tracks)
            {
                if (!track.SelectTokens("$.medias[*]").Any())
                {
                    track.Remove();
                }
                else
                {
                    track["trackIndex"] = trackIndex++;
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
                    int segmentStart = segment.CamStart;
                    int segmentEnd = segment.CamStart + segment.CamDuration;

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

        private void TrimAndCompressUnifiedMedia(IEnumerable<Scene> scenes)
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

                int sceneStart = 0;
                foreach (var scene in scenes)
                {
                    int sceneDuration = scene.Duration;
                    int targetStart = 0;
                    foreach (var segment in scene.VideoSegments)
                    {
                        int segmentStart = segment.CamStart;
                        int segmentEnd = segment.CamStart + segment.CamDuration;

                        if (segmentStart < start)
                            segmentStart = start;
                        if (segmentEnd > start + duration)
                            segmentEnd = start + duration;
                        int segmentDuration = segmentEnd - segmentStart;
                        if (segmentDuration > 0)
                        {
                            if (segment.IsSilent)
                            {
                                var newClip = clip.DeepClone();
                                newClip["id"] = _nextId++;
                                newClip["start"] = sceneStart + targetStart;
                                newClip["duration"] = segmentDuration;
                                newClip["mediaStart"] = mediaStart + segmentStart - start;
                                newClip["mediaDuration"] = segmentDuration;

                                var newAmfile = newClip["audio"];
                                newAmfile["id"] = _nextId++;
                                newAmfile["start"] = sceneStart + targetStart;
                                newAmfile["duration"] = segmentDuration;
                                newAmfile["mediaStart"] = mediaStart + segmentStart - start;
                                newAmfile["mediaDuration"] = segmentDuration;

                                var newScreenvmfile = newClip["video"];
                                if (newScreenvmfile != null)
                                {
                                    newScreenvmfile["id"] = _nextId++;
                                    newScreenvmfile["start"] = sceneStart + targetStart;
                                    newScreenvmfile["duration"] = segmentDuration;
                                    newScreenvmfile["mediaStart"] = mediaStart + segmentStart - start;
                                    newScreenvmfile["mediaDuration"] = segmentDuration;
                                }

                                parent.Add(newClip);
                            }

                            targetStart += segmentDuration;
                        }
                    }
                    sceneStart += sceneDuration;
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
    }
}

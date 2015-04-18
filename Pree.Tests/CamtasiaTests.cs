using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pree.Camtasia;
using FluentAssertions;
using System.Linq;

namespace Pree.Tests
{
    [TestClass]
    public class CamtasiaTests
    {
        private CamProject _camproj;

        [TestInitialize]
        public void Initialize()
        {
            _camproj = CamProject.Load(@"c:\Recording\test - Before.camproj");
        }

        [TestMethod]
        public void CanLoadCamproj()
        {
            _camproj.Write(@"c:\Recording\test_clipped.camproj");
        }

        [TestMethod]
        public void CanFindTimeline()
        {
            var timeline = _camproj.GetTimeline();
            
            timeline.Src.Should().Be(@"C:\Recording\test_time.wav");
            timeline.Id.Should().Be(6);
        }

        [TestMethod]
        public void CanLoadLog()
        {
            var timelineFilename = _camproj.GetTimeline().Src;
            string logFilename = timelineFilename.Substring(0, timelineFilename.Length - "_time.wav".Length) + ".log";
            TimeLog log = TimeLog.Load(logFilename);

            log.Segments.Count().Should().Be(3);
        }

        [TestMethod]
        public void CanReadIds()
        {
            int maxId = _camproj.GetNextId();

            maxId.Should().Be(97);
        }

        [TestMethod]
        public void CanFindTimelineOffset()
        {
            var timeline = _camproj.GetTimeline();
            TimeSpan offset = _camproj.GetOffset(timeline.Id);
            offset.TotalSeconds.Should().BeApproximately(238.0 / 30.0, 0.001);
        }

        [TestMethod]
        public void CanTrimSegments()
        {
            var timeline = _camproj.GetTimeline();
            TimeSpan offset = _camproj.GetOffset(timeline.Id);
            var timelineFilename = timeline.Src;
            string logFilename = timelineFilename.Substring(0, timelineFilename.Length - "_time.wav".Length) + ".log";
            TimeLog log = TimeLog.Load(logFilename);

            var offsetSegments = log.Segments
                .Select(s => new Segment(s.Start + offset, s.Duration));
            _camproj.Trim(offsetSegments);
            _camproj.Write(@"c:\Recording\test_clipped.camproj");
        }
    }
}

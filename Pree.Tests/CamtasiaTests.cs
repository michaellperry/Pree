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
            _camproj = CamProject.Load(@"C:\Recording\Distributed Systems\2-Designing Reliable Applications\10-Migrating to Immutable Records.tscproj");
        }

        [TestMethod]
        public void CanLoadCamproj()
        {
            _camproj.Write(@"C:\Recording\Distributed Systems\2-Designing Reliable Applications\10-Migrating to Immutable Records_passthroug.tscproj");
        }

        [TestMethod]
        public void CanFindTimeline()
        {
            var timeline = _camproj.GetTimeline();
            
            timeline.Src.Should().Be(@"C:\Recording\Distributed Systems\2-Designing Reliable Applications\10-Migrating to Immutable Records_time.wav");
            timeline.Id.Should().Be(2);
        }

        [TestMethod]
        public void CanLoadLog()
        {
            var timelineFilename = _camproj.GetTimeline().Src;
            string logFilename = timelineFilename.Substring(0, timelineFilename.Length - "_time.wav".Length) + ".log";
            TimeLog log = TimeLog.Load(logFilename);

            log.Segments.Count().Should().Be(50);
        }

        [TestMethod]
        public void CanReadIds()
        {
            int maxId = _camproj.GetNextId();

            maxId.Should().Be(8);
        }

        [TestMethod]
        public void CanFindTimelineOffset()
        {
            var timeline = _camproj.GetTimeline();
            TimeSpan offset = _camproj.GetOffset(timeline.Id);
            offset.TotalSeconds.Should().BeApproximately(-20.2, 0.001);
        }

        [TestMethod]
        public void CanTrimSegments()
        {
            string targetFilename = @"C:\Recording\Distributed Systems\2-Designing Reliable Applications\10-Migrating to Immutable Records_clipped.tscproj";
            _camproj.TrimAndWrite(targetFilename);
        }
    }
}

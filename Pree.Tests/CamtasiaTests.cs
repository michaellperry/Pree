using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pree.Camtasia;
using FluentAssertions;

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
    }
}

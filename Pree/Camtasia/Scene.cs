using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pree.Camtasia
{
    class Scene
    {
        private ImmutableList<Segment> audio;
        private ImmutableList<Segment> video;

        public Scene(ImmutableList<Segment> audio, ImmutableList<Segment> video, int position)
        {
            this.audio = audio;
            this.video = video;
            Position = position;
        }

        public IEnumerable<Segment> AudioSegments => audio;
        public IEnumerable<Segment> VideoSegments => video;

        public int Duration => Math.Max(
            audio.Sum(a => a.CamDuration),
            video.Sum(a => a.CamDuration)
        );

        public int Position { get; }
    }
}

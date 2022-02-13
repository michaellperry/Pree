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
        private ImmutableList<CamSegment> audio;
        private ImmutableList<CamSegment> video;

        public Scene(ImmutableList<CamSegment> audio, ImmutableList<CamSegment> video, int position)
        {
            this.audio = audio;
            this.video = video;
            Position = position;
        }

        public IEnumerable<CamSegment> AudioSegments => audio;
        public IEnumerable<CamSegment> VideoSegments => video;

        public int Duration => Math.Max(
            audio.Sum(a => a.CamDuration),
            video.Sum(a => a.CamDuration)
        );

        public int Position { get; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pree.Streams;

namespace Pree.Models
{
    class AudioFilter
    {
        private readonly RecordingSettings _recordingSettings;

        public AudioFilter(RecordingSettings recordingSettings)
        {
            _recordingSettings = recordingSettings;
        }

        public Stream OpenStream(Stream targetStream, long bytesAvailable)
        {
            return new TruncatingFilter(targetStream,
                _recordingSettings.TrimMilliseconds * _recordingSettings.SampleRate / 250,
                bytesAvailable);
        }
    }
}

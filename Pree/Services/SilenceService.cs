using NAudio.Wave;
using System;
using System.Linq;

namespace Pree.Services
{
    class SilenceService
    {
        private AudioFileReader reader;

        public SilenceService(string timelineFilename)
        {
            reader = new AudioFileReader(timelineFilename);
        }

        public bool IsSilent(DateTime start, DateTime duration)
        {
            long chunkSize = reader.WaveFormat.SampleRate * reader.WaveFormat.Channels;
            long sampleStart = (long)start.TimeOfDay.TotalMilliseconds * chunkSize / 1000;
            long sampleDuration = (long)duration.TimeOfDay.TotalMilliseconds * chunkSize / 1000;
            int chunkCount = (int)((sampleDuration-1) / chunkSize + 1);

            reader.Seek(sampleStart * 4, System.IO.SeekOrigin.Begin);
            float[] samples = new float[chunkSize];
            for (long chunk = 0; chunk < chunkCount; chunk++)
            {
                long chunkOffset = chunk * chunkSize;
                long nextChunkOffset = Math.Min((chunk + 1) * chunkSize, sampleDuration);
                int sampleCount = (int)(nextChunkOffset - chunkOffset);

                int read = reader.Read(samples, 0, sampleCount);

                if (read > 50)
                {
                    double maxAmplitude = samples.Skip(50).Take(read).Max(s => Math.Abs(s));
                    if (maxAmplitude > 0.5)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

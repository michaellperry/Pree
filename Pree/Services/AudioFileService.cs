using NAudio.Wave;
using Pree.CSP;
using Pree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Pree.Services
{
    class AudioFileService : Process
    {
        private readonly AudioFilter _audioFilter;

        private int _sampleRate;
        private int _channels;
        private WaveFileWriter _writer = null;

        public AudioFileService(AudioFilter audioFilter)
        {
            _audioFilter = audioFilter;
        }

        public void OpenFile(string destination, RecordingSettings recordingSettings)
        {
            _sampleRate = recordingSettings.SampleRate;
            _channels = recordingSettings.Channels;

            Enqueue(delegate
            {
                OnOpenFile(destination, _sampleRate, _channels);
            });
        }

        public void CloseFile()
        {
            Enqueue(delegate
            {
                OnCloseFile();
            });
        }

        public void WriteClip(Clip clip)
        {
            MemoryStream content = clip.Content;

            Enqueue(delegate
            {
                OnWriteClip(content);
            });
        }

        public void WriteTone(TimeSpan duration)
        {
            Enqueue(delegate
            {
                OnWriteTone(duration);
            });
        }

        private void OnOpenFile(string destination, int sampleRate, int channels)
        {
            if (_writer == null)
            {
                WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
                    sampleRate,
                    channels);

                _writer = new WaveFileWriter(destination, waveFormat);
            }
        }

        private void OnCloseFile()
        {
            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        private void OnWriteClip(MemoryStream content)
        {
            long bytesAvailable = content.Length;
            using (var filterStream = _audioFilter.OpenStream(_writer, bytesAvailable))
            {
                content.WriteTo(filterStream);
            }
        }

        private void OnWriteTone(TimeSpan duration)
        {
            long sampleCount = (long)(duration.TotalSeconds * _sampleRate);
            double phaseAngle = 0.0;
            for (long index = 0; index < sampleCount; index++)
            {
                float sample = (float)(0.5 * Math.Sin(phaseAngle));
                _writer.WriteSample(sample);
                phaseAngle +=
                    2 * Math.PI * 220.0 / _sampleRate;

                if (phaseAngle > 2 * Math.PI)
                    phaseAngle -= 2 * Math.PI;
            }
        }
    }
}

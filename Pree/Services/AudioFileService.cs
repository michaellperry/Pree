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

        private WaveFormat _waveFormat;
        private WaveFileWriter _writer = null;

        public AudioFileService(AudioFilter audioFilter)
        {
            _audioFilter = audioFilter;
        }

        public void OpenFile(string destination, RecordingSettings recordingSettings)
        {
            _waveFormat = recordingSettings.CreateWaveFormat();

            Enqueue(delegate
            {
                OnOpenFile(destination, _waveFormat);
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

        private void OnOpenFile(string destination, WaveFormat waveFormat)
        {
            if (_writer == null)
            {
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
            long sampleCount = (long)(duration.TotalSeconds * _waveFormat.SampleRate) * _waveFormat.Channels;
            double phaseAngle = 0.0;
            for (long index = 0; index < sampleCount; index++)
            {
                float sample = (float)(0.5 * Math.Sin(phaseAngle));
                _writer.WriteSample(sample);
                phaseAngle +=
                    2 * Math.PI * 220.0 / _waveFormat.SampleRate / _waveFormat.Channels;

                if (phaseAngle > 2 * Math.PI)
                    phaseAngle -= 2 * Math.PI;
            }
        }
    }
}

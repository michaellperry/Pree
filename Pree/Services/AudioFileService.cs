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

        private WaveFileWriter _writer = null;

        public AudioFileService(AudioFilter audioFilter)
        {
            _audioFilter = audioFilter;
        }

        public void OpenFile(string destination, RecordingSettings recordingSettings)
        {
            int sampleRate = recordingSettings.SampleRate;
            int bitsPerSample = recordingSettings.BitsPerSample;
            int channels = recordingSettings.Channels;

            Enqueue(delegate
            {
                OnOpenFile(destination, sampleRate, bitsPerSample, channels);
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

        private void OnOpenFile(string destination, int sampleRate, int bitsPerSample, int channels)
        {
            if (_writer == null)
            {
                using (var waveIn = new WaveIn())
                {
                    WaveFormat waveFormat = new WaveFormat(
                        sampleRate,
                        bitsPerSample,
                        channels);

                    _writer = new WaveFileWriter(destination, waveFormat);
                }
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
    }
}

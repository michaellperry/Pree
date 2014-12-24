using NAudio.Wave;
using System.Diagnostics.Contracts;
using System.IO;
using UpdateControls.Fields;
using System;
using Pree.Services;

namespace Pree.Models
{
    class AudioTarget
    {
        private readonly AudioFileService _audioFileService;
        private readonly AudioFileService _timelineFileService;

        private TimeSpan _lastClipEnd = TimeSpan.Zero;
        private Independent<bool> _isOpen = new Independent<bool>();

        public AudioTarget(AudioFileService audioFileService, AudioFileService timelineFileService)
        {
            _audioFileService = audioFileService;
            _timelineFileService = timelineFileService;
        }

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public void OpenFile(string destination, RecordingSettings recordingSettings)
        {
            Contract.Requires(!IsOpen);
            Contract.Ensures(IsOpen);

            var timelineFileName = destination.Substring(0, destination.LastIndexOf('.')) +
                "_time.wav";

            _audioFileService.OpenFile(destination, recordingSettings);
            _timelineFileService.OpenFile(timelineFileName, recordingSettings);

            _isOpen.Value = true;
        }

        public async void CloseFile()
        {
            Contract.Requires(IsOpen);

            _audioFileService.CloseFile();
            _timelineFileService.CloseFile();
            await _audioFileService.JoinAsync();
            await _timelineFileService.JoinAsync();

            _isOpen.Value = false;
        }

        public void WriteClip(Clip clip)
        {
            _audioFileService.WriteClip(clip);
            _timelineFileService.WriteTone(clip.StartTime - _lastClipEnd + TimeSpan.FromSeconds(0.4));
            _timelineFileService.WriteClip(clip);

            _lastClipEnd = clip.StartTime + clip.Duration;
        }
    }
}

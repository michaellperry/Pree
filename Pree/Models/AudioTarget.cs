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
        private readonly LogService _logService;

        private TimeSpan _lastClipEnd;
        private Independent<bool> _isOpen = new Independent<bool>();
        
        public AudioTarget(
            AudioFileService audioFileService,
            AudioFileService timelineFileService,
            LogService logService)
        {
            _audioFileService = audioFileService;
            _timelineFileService = timelineFileService;
            _logService = logService;
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
            var logFileName = destination.Substring(0, destination.LastIndexOf('.')) +
                ".log";

            _audioFileService.OpenFile(destination, recordingSettings);
            _timelineFileService.OpenFile(timelineFileName, recordingSettings);
            _logService.OpenFile(logFileName);
            _lastClipEnd = TimeSpan.Zero;

            _isOpen.Value = true;
        }

        public async void CloseFile()
        {
            Contract.Requires(IsOpen);

            _audioFileService.CloseFile();
            _timelineFileService.CloseFile();
            _logService.CloseFile();
            await _audioFileService.JoinAsync();
            await _timelineFileService.JoinAsync();
            await _logService.JoinAsync();

            _isOpen.Value = false;
        }

        public void WriteClip(Clip clip)
        {
            _audioFileService.WriteClip(clip);
            _timelineFileService.WriteTone(clip.StartTime - _lastClipEnd);
            _timelineFileService.WriteClip(clip);
            _logService.WriteSegment(clip.StartTime, clip.Duration);

            _lastClipEnd = clip.StartTime + clip.Duration;
        }
    }
}

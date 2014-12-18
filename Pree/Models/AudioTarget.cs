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

        private Independent<bool> _isOpen = new Independent<bool>();
        
        public AudioTarget(AudioFileService audioFileService)
        {
            _audioFileService = audioFileService;
        }

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public void OpenFile(string destination, RecordingSettings recordingSettings)
        {
            Contract.Requires(!IsOpen);
            Contract.Ensures(IsOpen);

            _audioFileService.OpenFile(destination, recordingSettings);

            _isOpen.Value = true;
        }

        public async void CloseFile()
        {
            Contract.Requires(IsOpen);

            _audioFileService.CloseFile();
            await _audioFileService.JoinAsync();

            _isOpen.Value = false;
        }

        public void WriteClip(Clip clip)
        {
            _audioFileService.WriteClip(clip);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdateControls.Fields;
using System.Diagnostics.Contracts;

namespace Pree.Models
{
    class RecordingSession
    {
        private readonly AudioSource _audioSource;
        private readonly AudioTarget _audioTarget;
        private readonly RecordingSettings _recordingSettings;
        private readonly Timer _timer;

        private Independent<bool> _shouldKeep = new Independent<bool>(true);
        private Independent<TimeSpan> _elapsedTime = new Independent<TimeSpan>(TimeSpan.Zero);
        private Independent<Clip> _clip = new Independent<Clip>();

        public RecordingSession(
            AudioSource audioSource,
            AudioTarget audioTarget,
            RecordingSettings recordingSettings,
            Timer timer)
        {
            _audioSource = audioSource;
            _audioTarget = audioTarget;
            _recordingSettings = recordingSettings;
            _timer = timer;
        }

        public bool IsActive
        {
            get { return _audioTarget.IsOpen; }
        }

        public bool Recording
        {
            get { return _audioSource.Recording; }
        }

        public bool ShouldKeep
        {
            get { return _shouldKeep.Value; }
            set { _shouldKeep.Value = value; }
        }

        public TimeSpan ElapsedTime
        {
            get
            {
                TimeSpan time = _elapsedTime.Value;

                if (_audioSource.Recording)
                    time += _audioSource.RecordingTime;

                if (_clip.Value != null && ShouldKeep)
                    time += _clip.Value.Duration;

                return time;
            }
        }

        public void BeginSession(string fileName)
        {
            Contract.Requires(!IsActive);
            Contract.Ensures(IsActive);

            _audioTarget.OpenFile(fileName, _recordingSettings);
        }

        public void EndSession()
        {
            Contract.Requires(IsActive);
            Contract.Ensures(!IsActive);

            if (_audioSource.Recording)
                StopRecording();

            CloseClip();
            _elapsedTime.Value = TimeSpan.Zero;

            _audioTarget.CloseFile();
        }

        public void StartRecording()
        {
            Contract.Requires(IsActive);
            Contract.Requires(!Recording);
            Contract.Ensures(Recording);

            CloseClip();

            ShouldKeep = true;
            _audioSource.StartRecording(_recordingSettings);
            _timer.Start();
        }

        public void StopRecording()
        {
            Contract.Requires(IsActive);
            Contract.Requires(Recording);
            Contract.Ensures(!Recording);

            _clip.Value = _audioSource.StopRecording();
            _timer.Stop();
        }

        private void CloseClip()
        {
            Clip clip = _clip.Value;
            _clip.Value = null;
            if (clip != null)
            {
                if (ShouldKeep)
                    KeepClip(clip);
                else
                    clip.Content.Close();
            }
        }

        private void KeepClip(Clip clip)
        {
            Contract.Assert(clip != null);

            _elapsedTime.Value += clip.Duration;

            _audioTarget.WriteClip(clip);
        }
    }
}

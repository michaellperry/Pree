using Microsoft.Win32;
using NAudio.Wave;
using Pree.Models;
using System.Collections.Generic;
using System.Windows.Input;
using UpdateControls.XAML;
using System.IO;
using System.Windows;
using System;
using Pree.Views;

namespace Pree.ViewModels
{
    class StartViewModel : IContentViewModel
    {
        private readonly RecordingSettings _recordingSettings;
        private readonly RecordingSession _recordingSession;
        
        public StartViewModel(
            RecordingSettings recordingSettings,
            RecordingSession recordingSession)
        {
            _recordingSettings = recordingSettings;
            _recordingSession = recordingSession;
        }

        public IEnumerable<DeviceViewModel> Devices
        {
            get
            {
                for (int deviceIndex = 0; deviceIndex < WaveIn.DeviceCount; deviceIndex++)
                {
                    yield return GetDeviceViewModel(deviceIndex);
                }
            }
        }

        public DeviceViewModel SelectedDevice
        {
            get { return GetDeviceViewModel(_recordingSettings.DeviceIndex); }
            set
            {
                _recordingSettings.DeviceIndex = value.DeviceIndex;
                _recordingSettings.Channels = 1;
            }
        }

        public int SampleRate
        {
            get { return _recordingSettings.SampleRate; }
            set { _recordingSettings.SampleRate = value; }
        }

        public ICommand Start
        {
            get
            {
                return MakeCommand
                    .When(() => !_recordingSession.IsActive)
                    .Do(() => StartSession());
            }
        }

        public ICommand Camproj
        {
            get
            {
                return MakeCommand
                    .Do(() => ProcessCamproj());
            }
        }

        public void Help()
        {
            new HelpView().ShowDialog();
        }

        private static DeviceViewModel GetDeviceViewModel(int deviceIndex)
        {
            return new DeviceViewModel(deviceIndex, WaveIn.GetCapabilities(deviceIndex));
        }

        private void StartSession()
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                OverwritePrompt = true,
                DefaultExt = "wav",
                Filter = "Wave files (*.wav)|*.wav|All files (*.*)|*.*"
            };
            bool? result = dialog.ShowDialog();

            if (result ?? false)
            {
                _recordingSession.BeginSession(dialog.FileName);
            }
        }

        private void ProcessCamproj()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                DefaultExt = "camproj",
                Filter = "Camtasia projects (*.camproj)|*.camproj|All files (*.*)|*.*"
            };
            bool? result = dialog.ShowDialog();

            if (result ?? false)
            {
                try
                {
                    string inputFilename = dialog.FileName;
                    var camproj = Camtasia.CamProject.Load(inputFilename);
                    string outputFilename =
                        Path.Combine(
                            Path.GetDirectoryName(inputFilename),
                            Path.GetFileNameWithoutExtension(inputFilename) +
                            "_trimmed.camproj");
                    camproj.TrimAndWrite(outputFilename);
                    MessageBox.Show("Finished processing");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}

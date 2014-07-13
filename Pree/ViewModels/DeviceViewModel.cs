using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pree.ViewModels
{
    public class DeviceViewModel
    {
        private readonly int _deviceIndex;
        private readonly WaveInCapabilities _capabilities;

        public DeviceViewModel(int deviceIndex, WaveInCapabilities capabilities)
        {
            _deviceIndex = deviceIndex;
            _capabilities = capabilities;
        }

        public int DeviceIndex
        {
            get { return _deviceIndex; }
        }

        public string Name
        {
            get { return _capabilities.ProductName; }
        }

        public int Channels
        {
            get { return _capabilities.Channels; }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            var that = obj as DeviceViewModel;
            if (that == null)
                return false;

            return this._deviceIndex == that._deviceIndex;
        }

        public override int GetHashCode()
        {
            return _deviceIndex;
        }
    }
}

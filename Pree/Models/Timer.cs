using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using UpdateControls.Fields;

namespace Pree.Models
{
    class Timer
    {
        private Independent<DateTime> _now = new Independent<DateTime>(DateTime.Now);
        private DispatcherTimer _timer;

        public Timer()
        {
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1.0)
            };
            _timer.Tick += TimerTick;
        }

        public void Dispose()
        {
            _timer.Tick -= TimerTick;
        }

        public void Start()
        {
            _now.Value = DateTime.Now;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _now.Value = DateTime.Now;
        }
    }
}

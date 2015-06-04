using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RNTi.App.GUI.Classes
{
    class WorkTimer
    {
        public delegate void TimerTick(WorkTimerEventArgs args);
        public event TimerTick OnTimerTick;
        public int Time;
        TimeSpan _span;
        public WorkTimer()
        {
            _timer.Tick += t_Tick;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        readonly Timer _timer = new Timer();


        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        

        public void Enable()
        {
            Time = 0;
            _timer.Interval = 1000;
            _timer.Enabled = true;
            _timer.Start();
        }

        void t_Tick(object sender, EventArgs e)
        {
            int idleTick = 0;
            int lastInputTick = 0;
            int systemUptime = Environment.TickCount;
            LASTINPUTINFO l = new LASTINPUTINFO();
            l.cbSize = (uint)Marshal.SizeOf(l);
            l.dwTime = 0;
            if (GetLastInputInfo(ref l))
            {
                lastInputTick = (int)l.dwTime;
                idleTick = systemUptime - lastInputTick;
                if ((idleTick / 1000) <= 60) //если бездействие меньше 60 секунд, то таймер не стопится
                    _span += TimeSpan.FromSeconds(1);
            }
            if (OnTimerTick != null)
            {
                OnTimerTick(new WorkTimerEventArgs(lastInputTick, (idleTick / 1000 % 60), _span));
            }
        }

        public void Disable()
        {
            _timer.Enabled = false;
            Time += (_span.Minutes * 60) + _span.Seconds;
            _span = TimeSpan.Zero;
        }
        public void Stop()
        {
            _timer.Stop();
        }
    }

    public class WorkTimerEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastInputTick"></param>
        /// <param name="idleTick"></param>
        /// <param name="time"></param>
        public WorkTimerEventArgs(int lastInputTick, int idleTick, TimeSpan time)
        {
            LastInputTick = lastInputTick;
            IdleTick = idleTick;
            Time = time;
        }
        public int LastInputTick { get; private set; }
        public int IdleTick { get; private set; }
        public TimeSpan Time { get; private set; }
    }
}
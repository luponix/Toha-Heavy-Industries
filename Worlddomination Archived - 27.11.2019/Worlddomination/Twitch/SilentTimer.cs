using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Worlddomination.Twitch
{
    class SilentTimer
    {
        private Timer _timer;
        private int _timer_intervall;

        public SilentTimer(int intervall)
        {
            _timer_intervall = intervall;
            Start();
        }
      

       
        public void Start()
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Timer       Setup pull streams timer");

           

            //Setup timer for 2 minutes
            var timer = new Timer(_timer_intervall);
            timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            timer.Enabled = true;
            _timer = timer;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            Program.silent_on_streamstart_event = false;
            int min = _timer_intervall / 60000;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Silent      "+min+" min timer ran out, toggling silent flag");
            _timer.Stop();
        }

        public void RestartTimer()
        {
            _timer.Stop();
            Start();
        }
    }
}

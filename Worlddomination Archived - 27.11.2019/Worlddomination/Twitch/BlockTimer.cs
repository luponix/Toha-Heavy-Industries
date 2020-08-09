using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Worlddomination.Twitch
{
    class BlockTimer
    {
        private Timer _timer;
        
        private int _timer_intervall = 15000;
        private string channel_id;
        private long _event_counter = 0;

        public BlockTimer( string _channel_id )
        {
            
            channel_id = _channel_id; 
        }

        // succesfully mined with Logs

        public void Start()
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Timer       Setup Block Timer for : "+channel_id);

            var timer = new Timer(_timer_intervall);
            timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            timer.Enabled = true;
            _timer = timer;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _event_counter++;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Timer    Undid the Block for [" + channel_id + "]");
            LiveMonitor.blocked.Remove(channel_id);

            _timer.Stop();

        }
        
    }
}
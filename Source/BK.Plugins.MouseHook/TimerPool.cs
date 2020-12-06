using System;
using System.Collections.Generic;
using System.Timers;

namespace BK.Plugins.MouseHook
{
	internal class TimerPool : IDisposable
	{
		private readonly List<Timer> _timers = new List<Timer>();
		private readonly bool _autoReset;
		private readonly int _interval;

		public TimerPool(bool autoReset, int interval)
		{
			_autoReset = autoReset;
			_interval = interval;
		}
		
		public event ElapsedEventHandler Elapsed;

		public void Start()
		{
			var timer = _timers.Find(t => !t.Enabled);
			if (timer != null)
			{
				timer.Start();
			}
			else
			{
				var t = new Timer(_interval);
				t.Elapsed += Elapsed;
				t.AutoReset = _autoReset;
				t.Start();
				_timers.Add(t);
			}
		}

		public void Stop()
		{
			foreach (var timer in _timers)
			{
				timer.Stop();
			}
		}

		public void Dispose()
		{
			foreach (var timer in _timers)
			{
				timer.Stop();
				timer.Elapsed -= Elapsed;
				timer.Dispose();
			}
		}
	}
}
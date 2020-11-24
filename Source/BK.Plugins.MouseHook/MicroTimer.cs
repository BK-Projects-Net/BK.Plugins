using System.Threading;

namespace BK.Plugins.MouseHook
{
	/// <summary>
	/// MicroTimer class
	/// </summary>
	internal class MicroTimer
	{
		public delegate void MicroTimerElapsedEventHandler(object sender, in MicroTimerEventArgs timerEventArgs);
		public event MicroTimerElapsedEventHandler MicroTimerElapsed;

		Thread _threadTimer = null;
		long _ignoreEventIfLateBy = long.MaxValue;
		long _timerIntervalInMicroSec = 0;
		bool _stopTimer = true;

		public MicroTimer() { }
		public MicroTimer(long timerIntervalInMicroseconds) => Interval = timerIntervalInMicroseconds;

		public long Interval
		{
			get => Interlocked.Read(ref _timerIntervalInMicroSec);
			set => Interlocked.Exchange(ref _timerIntervalInMicroSec, value);
		}

		public long IgnoreEventIfLateBy
		{
			get => Interlocked.Read(ref _ignoreEventIfLateBy);
			set => Interlocked.Exchange(
				ref _ignoreEventIfLateBy, value <= 0 ? long.MaxValue : value);
		}

		public bool Enabled
		{
			get => _threadTimer != null && _threadTimer.IsAlive;
			set { if (value) Start(); else Stop(); }
		}

		public void Start()
		{
			if (Enabled || Interval <= 0) return;

			_stopTimer = false;

			void ThreadStart() =>
				NotificationTimer(ref _timerIntervalInMicroSec, ref _ignoreEventIfLateBy, ref _stopTimer);

			_threadTimer = new Thread(ThreadStart) { Priority = ThreadPriority.Highest };
			_threadTimer.Start();
		}

		public void Stop() => _stopTimer = true;
		public void StopAndWait() => StopAndWait(Timeout.Infinite);

		public bool StopAndWait(int timeoutInMilliSec)
		{
			_stopTimer = true;

			if (!Enabled || _threadTimer.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
				return true;

			return _threadTimer.Join(timeoutInMilliSec);
		}

		public void Abort()
		{
			_stopTimer = true;

			if (Enabled) _threadTimer.Abort();
		}

		void NotificationTimer(ref long timerIntervalInMicroSec,
			ref long ignoreEventIfLateBy,
			ref bool stopTimer)
		{
			var timerCount = 0;
			long nextNotification = 0;

			MicroStopwatch microStopwatch = new MicroStopwatch();
			microStopwatch.Start();

			while (!stopTimer)
			{
				var callbackFunctionExecutionTime =
					microStopwatch.ElapsedMicroseconds - nextNotification;

				var timerIntervalInMicroSecCurrent = Interlocked.Read(ref timerIntervalInMicroSec);
				var ignoreEventIfLateByCurrent = Interlocked.Read(ref ignoreEventIfLateBy);

				nextNotification += timerIntervalInMicroSecCurrent;
				timerCount++;
				long elapsedMicroseconds = 0;

				while ((elapsedMicroseconds = microStopwatch.ElapsedMicroseconds) < nextNotification)
					Thread.SpinWait(10);

				var timerLateBy = elapsedMicroseconds - nextNotification;

				if (timerLateBy >= ignoreEventIfLateByCurrent) continue;

				var microTimerEventArgs =
					new MicroTimerEventArgs(timerCount,
						elapsedMicroseconds,
						timerLateBy,
						callbackFunctionExecutionTime);
				MicroTimerElapsed?.Invoke(this, microTimerEventArgs);
			}

			microStopwatch.Stop();
		}
	}
}
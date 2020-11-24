namespace BK.Plugins.MouseHook
{
	/// <summary>
	/// MicroTimer Event Argument class
	/// </summary>
	internal readonly struct MicroTimerEventArgs
	{
		// Simple counter, number times timed event (callback function) executed
		public int TimerCount { get; }

		// Time when timed event was called since timer started
		public long ElapsedMicroseconds { get; }

		// How late the timer was compared to when it should have been called
		public long TimerLateBy { get; }

		// Time it took to execute previous call to callback function (OnTimedEvent)
		public long CallbackFunctionExecutionTime { get; }

		public MicroTimerEventArgs(int timerCount,
			long elapsedMicroseconds,
			long timerLateBy,
			long callbackFunctionExecutionTime)
		{
			TimerCount = timerCount;
			ElapsedMicroseconds = elapsedMicroseconds;
			TimerLateBy = timerLateBy;
			CallbackFunctionExecutionTime = callbackFunctionExecutionTime;
		}

	}
}
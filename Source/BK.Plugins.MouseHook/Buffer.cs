using System.Collections.Concurrent;

namespace BK.Plugins.MouseHook
{
	internal class Buffer<T>
	{
		protected readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		private readonly int _maxCount;
		private readonly MicroTimer _timer;

		public delegate void BufferThresholdReached(T[] buffer);
		public event BufferThresholdReached ThresholdReached;

		public Buffer(int maxCount, int maxDelayMs)
		{
			_maxCount = maxCount;
			_timer = new MicroTimer(maxDelayMs * 1000);
			_timer.MicroTimerElapsed += TimerOnMicroTimerElapsed;
		}

		public virtual void Enqueue(in T item)
		{
			_queue.Enqueue(item);
			_timer.Start();
			if (_queue.Count >= _maxCount)
			{
				_timer.Stop();
				ThresholdReached?.Invoke(_queue.ToArray());
				while (_queue.TryDequeue(out _)) { }
			}
		}

		private void TimerOnMicroTimerElapsed(object sender, in MicroTimerEventArgs args)
		{
			_timer.Stop();
			ThresholdReached?.Invoke(_queue.ToArray());
			while (_queue.TryDequeue(out _)) { }
		}
	}
}
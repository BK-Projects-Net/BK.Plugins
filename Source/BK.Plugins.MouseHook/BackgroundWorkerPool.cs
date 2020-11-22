using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BK.Plugins.MouseHook
{
	internal class BackgroundWorkerPool : IDisposable
	{
		private readonly List<BackgroundWorker> _workers = new List<BackgroundWorker>();
		private bool _isDisposed;

		public event DoWorkEventHandler DoWork;
		public event RunWorkerCompletedEventHandler RunWorkerCompleted;
		public event ProgressChangedEventHandler ProgressChanged;

		public bool WorkerReportsProgress { get; set; }
		public bool WorkerSupportsCancellation { get; set; }

		public bool IsAnyBusy => _workers.Any(w => w.IsBusy);
		public bool IsAnyCancellationPending => _workers.Any(w => w.CancellationPending);

		public void RunWorkerAsync() => GetNonBusyOrCreateWorker().RunWorkerAsync();

		public void CancelWorkersAsync()
		{
			foreach (var worker in _workers)
			{
				worker.CancelAsync();
			}
		}

		private BackgroundWorker GetNonBusyOrCreateWorker()
		{
			var first = _workers.Find(w => !w.IsBusy);
			if (first != null)
				return first;

			var worker = CreateWorker();
			_workers.Add(worker);
			return worker;
		}

		private BackgroundWorker CreateWorker()
		{
			var w = new BackgroundWorker();

			w.DoWork += DoWork;
			w.RunWorkerCompleted += RunWorkerCompleted;
			w.ProgressChanged += ProgressChanged;

			w.WorkerSupportsCancellation = WorkerSupportsCancellation;
			w.WorkerReportsProgress = WorkerReportsProgress;
			return w;
		}

		public void Dispose()
		{
			if (_isDisposed) return;

			foreach (var worker in _workers)
			{
				worker.DoWork -= DoWork;
				worker.ProgressChanged -= ProgressChanged;
				worker.RunWorkerCompleted -= RunWorkerCompleted;
				worker.Dispose();
			}

			_isDisposed = true;
		}
	}
}
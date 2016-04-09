using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Grammophone.Windows
{
	/// <summary>
	/// A window for waiting a non-cancellable task to complete while showing the 
	/// <see cref="Trace"/> messages.
	/// </summary>
	public partial class TaskWindow : Window
	{
		private Task task;

		private TextBoxTraceListener traceListener;

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="caption">The caption of the window.</param>
		/// <param name="task">The task to start.</param>
		public TaskWindow(string caption, Task task)
		{
			if (caption == null) throw new ArgumentNullException("caption");
			if (task == null) throw new ArgumentNullException("task");

			this.task = task;

			InitializeComponent();

			this.Title = caption;

			this.traceListener = new TextBoxTraceListener(traceTextBox);

			Trace.Listeners.Add(traceListener);

			StartTask();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			// Make all the window transparent.
			GlassHelper.RegisterGlassHandling(this, GlassHelper.Margins.Full);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (e.Cancel) return;

			e.Cancel = !CanClose();
		}

		protected override void OnClosed(EventArgs e)
		{
			Trace.Listeners.Remove(traceListener);

			base.OnClosed(e);
		}

		private void StartTask()
		{
			var stopwatch = new Stopwatch();

			task.ContinueWith(
				t => TraceException(t.Exception),
				TaskContinuationOptions.OnlyOnFaulted
			);

			task.ContinueWith(t =>
				{
					stopwatch.Stop();
					Trace.WriteLine(String.Format("Elapsed time: {0} ({1} seconds).", stopwatch.Elapsed, stopwatch.Elapsed.TotalSeconds));

					this.Dispatcher.Invoke((Action)delegate
					{
						CommandManager.InvalidateRequerySuggested();
					});
				},
				TaskContinuationOptions.ExecuteSynchronously
			);

			stopwatch.Start();

			task.Start();
		}

		private void TraceException(Exception exception)
		{
			if (exception == null) throw new ArgumentNullException("exception");

			var aggregateException = exception as AggregateException;

			if (aggregateException != null)
			{
				aggregateException.Flatten();

				foreach (var innerException in aggregateException.InnerExceptions)
				{
					TraceException(innerException);
				}
			}

			if (exception.Source != null && exception.Source.Length > 0)
				Trace.WriteLine(String.Format("Exception: {0} (Source: {1})", exception.Message, exception.Source));
			else
				Trace.WriteLine(String.Format("Exception: {0}", exception.Message));
		}

		private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CanClose();
		}

		private void CloseCommand_Execute(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private bool CanClose()
		{
			return task.IsCanceled || task.IsCompleted || task.IsFaulted;
		}

	}
}

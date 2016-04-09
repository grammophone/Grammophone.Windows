using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;

namespace Grammophone.Windows
{
	public class TextBoxTraceListener : TraceListener
	{
		private TextBox textBox;

		public TextBoxTraceListener(TextBox textBox)
		{
			if (textBox == null) throw new ArgumentNullException("textBox");

			this.textBox = textBox;
		}

		public override void Write(string message)
		{
			textBox.Dispatcher.BeginInvoke((Action)delegate
			{
				textBox.AppendText(message);
				textBox.ScrollToEnd();
			});
		}

		public override void WriteLine(string message)
		{
			textBox.Dispatcher.BeginInvoke((Action)delegate
			{
				textBox.AppendText(message + "\n");
				textBox.ScrollToEnd();
			});
		}
	}
}

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Gramma.Windows
{
	/// <summary>
	/// Helper for extending glass background into the client area of a window.
	/// </summary>
	public static class GlassHelper
	{
		#region Private fields

		private const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

		#endregion

		#region Auxilliary types

		/// <summary>
		/// Margins for defining inset glass regions inside the client area.
		/// If at least one margin is negative, then the whole client area will lie on glass.
		/// </summary>
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct Margins
		{
			public int LeftWidth;
			public int RightWidth;
			public int TopHeight;
			public int BottomHeight;

			/// <summary>
			/// Specifies that all client area will be transparent.
			/// </summary>
			public static readonly Margins Full = new Margins { LeftWidth = -1, RightWidth = -1, TopHeight = -1, BottomHeight = -1 };
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Register this window to extend its transparency with static <paramref name="margins"/>
		/// whenever glass composition is available.
		/// </summary>
		/// <param name="window">The window whose to extend transparency.</param>
		/// <param name="margins">
		/// The sizes of glass margins inside the client area.
		/// If at least one margin is negative, then the whole client area will lie on glass.
		/// </param>
		public static void RegisterGlassHandling(Window window, Margins margins)
		{
			if (window == null) throw new ArgumentNullException("window");

			UpdateGlassMargins(window, ref margins);

			var windowInteropHelper = new WindowInteropHelper(window);

			HwndSource wpfWindowSource = HwndSource.FromHwnd(windowInteropHelper.Handle);

			wpfWindowSource.AddHook(delegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				switch (msg)
				{
					case WM_DWMCOMPOSITIONCHANGED:
						UpdateGlassMargins(window, ref margins);
						handled = true;
						break;
				}

				return IntPtr.Zero;
			});

		}

		/// <summary>
		/// Register this window to extend its transparency with dynamic margins
		/// whenever glass composition is available.
		/// </summary>
		/// <typeparam name="W">The type of the window.</typeparam>
		/// <param name="window">The window whose to extend transparency.</param>
		/// <param name="handler">
		/// The delegate being called whenever margins must be specified.
		/// This is called initially if composition is available, during window resize 
		/// and whenever composition becomes available.
		/// </param>
		public static void RegisterGlassHandling<W>(W window, Func<W, Margins> handler)
			where W : Window
		{
			if (window == null) throw new ArgumentNullException("window");
			if (handler == null) throw new ArgumentNullException("handler");

			UpdateGlassMargins(window, handler);

			var windowInteropHelper = new WindowInteropHelper(window);

			HwndSource wpfWindowSource = HwndSource.FromHwnd(windowInteropHelper.Handle);

			wpfWindowSource.AddHook(delegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				switch (msg)
				{
					case WM_DWMCOMPOSITIONCHANGED:
						UpdateGlassMargins(window, handler);
						handled = true;
						break;
				}

				return IntPtr.Zero;
			});

			window.SizeChanged += delegate(object sender, SizeChangedEventArgs eventArgs)
			{
				UpdateGlassMargins(window, handler);
			};

		}

		/// <summary>
		/// Define the glass client area for a window. The window must have a backgound with transparency, possibly varying.
		/// </summary>
		/// <param name="window">The window whose client to extend with glass.</param>
		/// <param name="margins">
		/// The sizes of glass margins inside the client area.
		/// If at least one margin is negative, then the whole client area will lie on glass.
		/// </param>
		public static void ExtendFrameIntoClientArea(this Window window, ref Margins margins)
		{
			try
			{
				var windowInteropHelper = new WindowInteropHelper(window);

				HwndSource wpfWindowSource = HwndSource.FromHwnd(windowInteropHelper.Handle);

				wpfWindowSource.CompositionTarget.BackgroundColor = Colors.Transparent;

				int hr = DwmExtendFrameIntoClientArea(wpfWindowSource.Handle, ref margins);

				if (hr != 0)
				{
					System.Diagnostics.Trace.WriteLine("DwmExtendFrameIntoClientArea failed.");
				}
			}
			catch (DllNotFoundException)
			{
				System.Diagnostics.Trace.WriteLine("The desktop composition API 'dwmapi.dll' was not found in the system.");

				SetWhiteCompositionTarget(window);
			}
		}

		/// <summary>
		/// Query if composition for glass effect is enabled.
		/// </summary>
		/// <returns>Returns true if glass effect is possible.</returns>
		public static bool IsCompositionEnabled()
		{
			try
			{
				int isEnabled;

				DwmIsCompositionEnabled(out isEnabled);

				return isEnabled != 0;
			}
			catch (DllNotFoundException)
			{
				return false;
			}
		}

		#endregion

		#region Private methods

		private static void UpdateGlassMargins<W>(W window, Func<W, Margins> handler)
			where W : Window
		{
			if (IsCompositionEnabled())
			{
				Margins margins = handler(window);

				window.ExtendFrameIntoClientArea(ref margins);
			}
			else
			{
				SetWhiteCompositionTarget(window);
			}
		}

		private static void UpdateGlassMargins(Window window, ref Margins margins)
		{
			if (IsCompositionEnabled())
			{
				window.ExtendFrameIntoClientArea(ref margins);
			}
			else
			{
				SetWhiteCompositionTarget(window);
			}
		}

		private static void SetWhiteCompositionTarget(Window window)
		{
			var windowInteropHelper = new WindowInteropHelper(window);

			HwndSource wpfWindowSource = HwndSource.FromHwnd(windowInteropHelper.Handle);

			wpfWindowSource.CompositionTarget.BackgroundColor = Colors.LightGray;
		}

		#endregion

		#region DWMAPI.DLL imports

		[DllImport("dwmapi.dll")]
		extern static int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

		[DllImport("dwmapi.dll")]
		extern static int DwmIsCompositionEnabled(out int isEnabled);

		#endregion
	}
}
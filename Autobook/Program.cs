using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Threading;

namespace Autobook
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main ()
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
            Application.SetUnhandledExceptionMode
              (UnhandledExceptionMode.CatchException);
			FormMain fm = new FormMain ();
			Application.ThreadException +=
				new ThreadExceptionEventHandler (fm.Application_ThreadException);
			AppDomain.CurrentDomain.UnhandledException +=
				new UnhandledExceptionEventHandler (fm.CurrentDomain_UnhandledException);
			Application.Run (fm);
		}
/*
		static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
		{
			ShowUnhandledExceptionDlg ((Exception) e.ExceptionObject);
		}

		static void Application_ThreadException (object sender, ThreadExceptionEventArgs e)
		{
			//ShowUnhandledExceptionDlg (e.Exception);
			fm.Application_ThreadException(sender, e);
		}

		static private void ShowUnhandledExceptionDlg (Exception e)
		{
			MessageBox.Show (e.Message + "\n" + e.StackTrace + "\n",
			  "MomentumTrader - Error Reporting", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Application.Exit ();
		}
*/
	}
}
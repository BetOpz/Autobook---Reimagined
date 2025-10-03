using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Autobook
{
  public partial class BookView : UserControl
  {
    private delegate void UpdateBookDelegate (double backBook, double layBook, bool isBackValid);

    public BookView ()
    {
      InitializeComponent ();
      buttonBack.BackColor = Globals.BackColor;
      buttonLay.BackColor = Globals.LayColor;
    }

    public void UpdateBook (double backBook, double layBook, bool isBackAvailable)
    {
      if (InvokeRequired)
      {
        BeginInvoke (new UpdateBookDelegate (UpdateBook), new object[] { backBook, layBook, isBackAvailable });
      }
      else
      {
        //if (isBackAvailable)
          buttonBack.Text=backBook.ToString ("###.0");
        //else
        //  buttonBack.Text ="NA";
        buttonLay.Text = layBook.ToString ("###.0");
      }
    }
  }
}

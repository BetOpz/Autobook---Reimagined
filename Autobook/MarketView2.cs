using System;
using System.Drawing;
using System.Windows.Forms;

namespace Autobook
{
    public enum RunnerStatusChange
    {
        Enabled,
        Disabled
    }

    public partial class MarketView2 : UserControl
  {
    private delegate void DisplayPricesDelegate ();

      public delegate void OnRunnerStatusChangedDelegate(long selectionId, RunnerStatusChange statusChange);
      public event OnRunnerStatusChangedDelegate OnRunnerStatusChanged;

    //public delegate void OnBetSelectedDelegate (Odds odds, string parentName);
    //public event OnBetSelectedDelegate OnBetSelected;

    private Runner runnerPrices;
    private double profitLiability;
    private bool isFiltered;
    private string baseRunnerName;
    private int betCount = 0;
    private int cancelledCount = 0; // Changed from repCount to track cancelled bets
    private int matchedCount = 0;
    private int unmatchedCount = 0;
    private double totalPlacedPayout = 0;
    private double totalMatchedPayout = 0;
    private double totalUnmatchedPayout = 0;
    private double totalCancelledPayout = 0; // Track stake * price cancelled
    //private bool _isTradeOutBetPlaced = false;
    //private int nbpIndex = 0;

    #region Properties

    public double ProfitLiability
    {
      get { return profitLiability; }
      set { profitLiability = value; }
    }

    public int BetCount
    {
      get { return betCount; }
    }

    public double TotalPlacedPayout
    {
      get { return totalPlacedPayout; }
    }

    public double TotalCancelledPayout
    {
      get { return totalCancelledPayout; }
    }

    #endregion Properties

    //public MarketView2 (BetfairE.Runner runner, Runner runnerPrices, int nbpIndex)
    public MarketView2 (BetfairNgClient.Json.Runner runner, Runner runnerPrices, string runnerName)
    {
      InitializeComponent ();
      buttonBack.BackColor = Color.FromArgb(91, 192, 222);
      buttonBack.ForeColor = Color.White;
      buttonLay.BackColor = Color.FromArgb(250, 176, 215);
      buttonLay.ForeColor = Color.FromArgb(50, 50, 50);
      this.runnerPrices = runnerPrices;
            //this.nbpIndex = nbpIndex;
      baseRunnerName = runnerName;
      labelRunnerName.Text = runnerName;

      // Bring labels to front so they're visible
      labelExc.BringToFront();
      labelBets.BringToFront();
      labelReps.BringToFront();
      labelMatched.BringToFront();
      labelUnmatched.BringToFront();

      UpdateView ();
    }

    public void UpdateOdds (Runner runnerPrices)
    {
      this.runnerPrices = runnerPrices;
      UpdateView ();
    }

    public void SetFiltered(bool filtered)
    {
      isFiltered = filtered;
      UpdateExcLabel();
    }

    public void IncrementBetCount()
    {
      betCount++;
      UpdateBetsLabel();
    }

    public void IncrementBetCount(double payout)
    {
      betCount++;
      totalPlacedPayout += payout;
      UpdateBetsLabel();
    }

    public void IncrementCancelledCount()
    {
      cancelledCount++;
      UpdateCancelledLabel();
    }

    public void IncrementCancelledPayout(double payout)
    {
      totalCancelledPayout += payout;
      cancelledCount++;
      UpdateCancelledLabel();
    }

    public void SetMatchedUnmatched(int matched, int unmatched)
    {
      matchedCount = matched;
      unmatchedCount = unmatched;
      UpdateMatchedLabel();
      UpdateUnmatchedLabel();
    }

    public void SetMatchedUnmatched(int matched, int unmatched, double matchedPayout, double unmatchedPayout)
    {
      matchedCount = matched;
      unmatchedCount = unmatched;
      totalMatchedPayout = matchedPayout;
      totalUnmatchedPayout = unmatchedPayout;
      UpdateMatchedLabel();
      UpdateUnmatchedLabel();
    }

    private void UpdateExcLabel()
    {
      if (InvokeRequired)
      {
        BeginInvoke(new MethodInvoker(UpdateExcLabel));
      }
      else
      {
        labelExc.Text = isFiltered ? "X" : "";
      }
    }

    private void UpdateBetsLabel()
    {
      if (InvokeRequired)
      {
        BeginInvoke(new MethodInvoker(UpdateBetsLabel));
      }
      else
      {
        if (betCount > 0)
        {
          labelBets.Text = totalPlacedPayout > 0
            ? $"{betCount} (£{totalPlacedPayout:F0})"
            : betCount.ToString();
        }
        else
        {
          labelBets.Text = "";
        }
      }
    }

    private void UpdateCancelledLabel()
    {
      if (InvokeRequired)
      {
        BeginInvoke(new MethodInvoker(UpdateCancelledLabel));
      }
      else
      {
        labelReps.Text = cancelledCount > 0 ? cancelledCount.ToString() : "";
      }
    }

    private void UpdateMatchedLabel()
    {
      if (InvokeRequired)
      {
        BeginInvoke(new MethodInvoker(UpdateMatchedLabel));
      }
      else
      {
        if (matchedCount > 0 || totalMatchedPayout > 0)
        {
          if (matchedCount > 0 && totalMatchedPayout > 0)
            labelMatched.Text = $"{matchedCount} (£{totalMatchedPayout:F0})";
          else if (matchedCount > 0)
            labelMatched.Text = matchedCount.ToString();
          else
            labelMatched.Text = $"£{totalMatchedPayout:F0}";
        }
        else
        {
          labelMatched.Text = "";
        }
      }
    }

    private void UpdateUnmatchedLabel()
    {
      if (InvokeRequired)
      {
        BeginInvoke(new MethodInvoker(UpdateUnmatchedLabel));
      }
      else
      {
        if (unmatchedCount > 0)
        {
          labelUnmatched.Text = totalUnmatchedPayout > 0
            ? $"{unmatchedCount} (£{totalUnmatchedPayout:F0})"
            : unmatchedCount.ToString();
        }
        else
        {
          labelUnmatched.Text = "";
        }
      }
    }

    private void UpdateView ()
    {
      if (InvokeRequired)
      {
        BeginInvoke (new DisplayPricesDelegate (UpdateView));
      }
      else
      {
        string price = (double.IsNaN(runnerPrices.BackPrice))
          ? ""
					: runnerPrices.BackPrice.ToString ();
        string size = FormatSize (runnerPrices.BackAmount);
        buttonBack.Text = price + Environment.NewLine + size;

        price = (double.IsNaN( runnerPrices.LayPrice ))
					? ""
          : runnerPrices.LayPrice.ToString ();
        size = FormatSize (runnerPrices.LayAmount);
        buttonLay.Text = price + Environment.NewLine + size;

        labelRunnerPL.ForeColor = profitLiability >= 0 ? Color.Green : Color.Red;
        labelRunnerPL.Text = Math.Round (profitLiability, 2).ToString ();
      }
    }

    private static string FormatSize (double size)
    {
			if (double.IsNaN (size))
				return string.Empty;

      string separator = System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator;

      string formatted = size.ToString ("c");
      int index = formatted.LastIndexOf (separator);
      if (index != -1)
      {
        formatted = formatted.Substring (0, index);
      }
      return formatted;
    }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disableToolStripMenuItem.Enabled = false;
            enableToolStripMenuItem.Enabled = true;
            labelRunnerPL.Enabled = false;
            labelRunnerName.Enabled = false;
            buttonBack.BackColor = Color.FromArgb(189, 189, 189);
            buttonBack.ForeColor = Color.FromArgb(120, 120, 120);
            buttonLay.BackColor = Color.FromArgb(189, 189, 189);
            buttonLay.ForeColor = Color.FromArgb(120, 120, 120);
            if (OnRunnerStatusChanged != null)
                OnRunnerStatusChanged(runnerPrices.SelectionId, RunnerStatusChange.Disabled);
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableToolStripMenuItem.Enabled = false;
            disableToolStripMenuItem.Enabled = true;
            labelRunnerPL.Enabled = true;
            labelRunnerName.Enabled = true;
            buttonBack.BackColor = Color.FromArgb(91, 192, 222);
            buttonBack.ForeColor = Color.White;
            buttonLay.BackColor = Color.FromArgb(250, 176, 215);
            buttonLay.ForeColor = Color.FromArgb(50, 50, 50);
            buttonLay.UseVisualStyleBackColor = false;
            if (OnRunnerStatusChanged != null)
                OnRunnerStatusChanged(runnerPrices.SelectionId, RunnerStatusChange.Enabled);
        }
  }
}
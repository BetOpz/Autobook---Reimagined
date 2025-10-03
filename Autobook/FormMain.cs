using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PthLog;
using System.Threading;
using BetfairNgClient.Json;
using BetfairNgClient.Json.Enums;

namespace Autobook
{
  public partial class FormMain : Form
  {
    #region Private Members

    private delegate void SetErrorStatusDelegate (string errorMsg);
    private Logger _logger;
    private bool isClosing = false;
    private Dictionary<string, MarketTabPage> marketPages = new Dictionary<string, MarketTabPage>();
    private Dictionary<string, List<Control>> marketControls = new Dictionary<string, List<Control>>();
    private MarketTabPage currentMarketPage = null;
    private string currentMarketId = null;
    private List<MarketInfo> availableMarkets = new List<MarketInfo>();
    private HashSet<string> autoStoppedMarkets = new HashSet<string>(); // Track which markets have been auto-stopped

    #endregion Private Members

    public FormMain ()
    {
      _logger = LogManager.Current.AddLogger ("Autobook", Globals.MakeMainLogName ("Autobook.txt"), "%date [%-16thread] %-5level - %message%n");
      Globals.SetLogger (_logger);
      try
      {
        InitializeComponent ();

        // Enable double buffering for smoother rendering
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

        // Move the tab control into the market view panel
        tabControlMarkets.Dock = DockStyle.Fill;
        tabControlMarkets.Appearance = TabAppearance.FlatButtons;
        tabControlMarkets.ItemSize = new Size(0, 1);
        tabControlMarkets.SizeMode = TabSizeMode.Fixed;
        tabControlMarkets.SelectedIndexChanged += TabControlMarkets_SelectedIndexChanged;
        panelMarketView.Controls.Add(tabControlMarkets);
        tabControlMarkets.BringToFront();

        // Start the timer to check for markets to auto-load
        timerCheckMarkets.Start();
      }
      catch (Exception ex)
      {
        _logger.Error (ex.ToString ());
      }
    }

    private void FormMain_FormClosing (object sender, FormClosingEventArgs e)
    {
      if (!isClosing)
      {
        _logger.Info ("Autobook stopped");
        isClosing = true;
        foreach (var kvp in marketPages)
        {
          if (kvp.Value != null)
            kvp.Value.Stop ();
        }
        Globals.StopKeepAlive();
        marketPages.Clear ();
        Application.Exit ();
      }
    }

    private void SetErrorStatus (string errorMsg)
    {
      if (InvokeRequired)
      {
        Invoke (new SetErrorStatusDelegate (SetErrorStatus), new object[] { errorMsg });
        return;
      }

      if (Globals.IsNetworkOnline)
      {
        toolStripButtonNetworkStatus.Image = Properties.Resources.Knob_Green;
        toolStripButtonNetworkStatus.Text = "Connected";
      }
      else
      {
        toolStripButtonNetworkStatus.Image = Properties.Resources.Knob_Red;
        toolStripButtonNetworkStatus.Text = "Disconnected";
      }
      toolStripLabelApiFailCount.Text = "API errors = " + Globals.ApiFailCount.ToString ();
      toolStripLabelApiErrorMsg.Text = "Last error = " + errorMsg;
    }

    private void FormMain_Load (object sender, EventArgs e)
    {
      try
      {
        _logger.Info ("Autobook started, Modern UI version");
        this.WindowState = FormWindowState.Maximized;
        SetErrorStatus (string.Empty);
        string result = AppSettings.ReadSettings ();
        if (!String.IsNullOrEmpty(result))
        {
            MessageBox.Show(this, result, "Load Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        FormMainLogin fml = new FormMainLogin();
        DialogResult dr = fml.ShowDialog (this);
        if (dr == DialogResult.Cancel)
        {
          _logger.Info ("Autobook stopping, login failed");
          Application.Exit ();
          return;
        }

        Globals.OnKeepAliveError += new Globals.OnKeepAliveErrorDelegate (Globals_OnKeepAliveError);
        SetErrorStatus (string.Empty);

        UpdateWallets ();

        // Auto-load UK & IRE races for today
        AutoLoadTodaysRaces();
      }
      catch (Exception ex)
      {
        MessageBox.Show (ex.Message, "Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
        _logger.Error (ex.ToString ());
      }
    }

    private void Globals_OnKeepAliveError (string errorMsg)
    {
      SetErrorStatus (errorMsg);
    }

    private void UpdateWallets ()
    {
      try
      {
          var ukAccountFunds = Globals.Exchange.GetAccountFunds(Globals.BETFAIR_EXCHANGE_UK);
        Globals.UkWallet = ukAccountFunds.AvailableToBetBalance;
        toolStripLabelUKWallet.Text = string.Format
          ("UK: {0} {1}", Globals.UkWallet.ToString ("F2"), Globals.Exchange.Currency);
          toolStripLabelPoints.Text = string.Format ("Points: {0}", Globals.Exchange.PointsBalance);
        Globals.CheckNetworkStatusIsConnected ();
      }
      catch (Exception ex)
      {
        Globals.UnhandledException (ex);
      }
    }

    #region Auto-Load Markets

    private void AutoLoadTodaysRaces()
    {
      try
      {
        _logger.Info("Auto-loading today's UK & IRE races...");

        // Get today's racing markets
        var filter = new MarketFilter
        {
          EventTypeIds = new HashSet<string> { "7" }, // Horse Racing
          MarketCountries = new HashSet<string> { "GB", "IE" },
          MarketTypeCodes = new HashSet<string> { "WIN" },
          MarketStartTime = new TimeRange
          {
            From = DateTime.Today,
            To = DateTime.Today.AddDays(1)
          }
        };

        var marketCatalogue = Globals.Exchange.ListMarketCatalogue(
          Globals.BETFAIR_EXCHANGE_UK,
          filter,
          new HashSet<MarketProjectionEnum>
          {
            MarketProjectionEnum.EVENT,
            MarketProjectionEnum.MARKET_START_TIME,
            MarketProjectionEnum.COMPETITION
          },
          MarketSortEnum.FIRST_TO_START,
          1000
        );

        if (marketCatalogue != null && marketCatalogue.Count > 0)
        {
          _logger.Info($"Found {marketCatalogue.Count} markets");

          // Sort by start time
          var sortedMarkets = marketCatalogue
            .Where(m => m.MarketStartTime != null)
            .OrderBy(m => m.MarketStartTime)
            .ToList();

          foreach (var market in sortedMarkets)
          {
            if (market.MarketStartTime != null)
            {
              string marketName = market.Event != null ? market.Event.Name : market.MarketName;
              // Convert UTC to local time
              DateTime localTime = market.MarketStartTime.ToLocalTime();
              AddMarketToList(market.MarketId, localTime, marketName);
            }
          }

          _logger.Info($"Auto-loaded {sortedMarkets.Count} markets");
        }
        else
        {
          _logger.Info("No markets found for today");
        }
      }
      catch (Exception ex)
      {
        _logger.Error($"Error auto-loading races: {ex.ToString()}");
        MessageBox.Show(this, "Failed to auto-load races: " + ex.Message, "Auto-Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private void AddMarketToList(string marketId, DateTime startTime, string marketName)
    {
      if (InvokeRequired)
      {
        Invoke(new Action<string, DateTime, string>(AddMarketToList), marketId, startTime, marketName);
        return;
      }

      var item = new ListViewItem(new[] { startTime.ToString("HH:mm"), marketName });
      var marketInfo = new MarketInfo { MarketId = marketId, StartTime = startTime, MarketName = marketName };
      item.Tag = marketInfo;
      item.ForeColor = Color.White;

      listViewMarkets.Items.Add(item);
      availableMarkets.Add(marketInfo);
    }

    #endregion

    #region Auto-Start Markets

    private void timerCheckMarkets_Tick(object sender, EventArgs e)
    {
      try
      {
        // Auto-load next race if needed
        AutoLoadNextRace();

        // Check loaded markets and auto-start them X minutes before race start time
        double minutesBeforeStart = AppSettings.BeforeStart;

        foreach (var kvp in marketPages.ToList())
        {
          var marketPage = kvp.Value;

          // Check if market should be started based on start time
          TimeSpan timeUntilStart = marketPage.StartTime - DateTime.Now;

          // Start if within the configured minutes before start and not already started
          if (timeUntilStart.TotalMinutes <= minutesBeforeStart && timeUntilStart.TotalMinutes > -60 && !marketPage.IsStarted)
          {
            _logger.Info($"Auto-starting market {marketPage.Text} - {timeUntilStart.TotalMinutes:F1} minutes until start");
            marketPage.Start();
          }

          // Auto-stop finished races (in-play AND suspended = race finished)
          if (marketPage.IsRaceFinished && marketPage.IsStarted && !autoStoppedMarkets.Contains(kvp.Key))
          {
            _logger.Info($"Auto-stopping finished market {marketPage.Text} (in-play={marketPage.IsInPlay}, status={marketPage.MarketStatus})");
            marketPage.Stop();
            autoStoppedMarkets.Add(kvp.Key); // Mark as stopped so we don't stop it again
          }

          // Auto-close markets with CLOSED status (result settled, can be removed)
          if (marketPage.MarketStatus == MarketStatusEnum.CLOSED && !marketPage.IsStarted)
          {
            _logger.Info($"Auto-closing settled market {marketPage.Text} - removing from display");

            // Remove tab
            if (tabControlMarkets.TabPages.Contains(marketPage))
            {
              tabControlMarkets.TabPages.Remove(marketPage);
            }

            // Remove from tracking
            marketPages.Remove(kvp.Key);
            autoStoppedMarkets.Remove(kvp.Key);

            // If this was the current market, clear it
            if (currentMarketId == kvp.Key)
            {
              currentMarketPage = null;
              currentMarketId = null;
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error($"Error in timerCheckMarkets_Tick: {ex.ToString()}");
      }
    }

    private void AutoLoadNextRace()
    {
      try
      {
        // If no markets loaded or all are started, load the next upcoming race
        if (marketPages.Count == 0 || marketPages.All(m => m.Value.IsStarted))
        {
          // Find the next race that hasn't been loaded yet
          foreach (ListViewItem item in listViewMarkets.Items)
          {
            var marketInfo = item.Tag as MarketInfo;
            if (marketInfo != null && !marketPages.ContainsKey(marketInfo.MarketId))
            {
              // Check if this race is upcoming (within next 2 hours)
              TimeSpan timeUntilStart = marketInfo.StartTime - DateTime.Now;
              if (timeUntilStart.TotalHours <= 2 && timeUntilStart.TotalMinutes > -60)
              {
                _logger.Info($"Auto-loading next race: {marketInfo.MarketName} ({marketInfo.MarketId})");
                LoadMarket(marketInfo);
                return;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error($"Error in AutoLoadNextRace: {ex.ToString()}");
      }
    }

    #endregion

    #region Results

    private void toolStripButtonResults_Click(object sender, EventArgs e)
    {
      try
      {
        string resultsLogPath = Path.Combine(
          Application.StartupPath,
          "RaceResults.log"
        );

        if (File.Exists(resultsLogPath))
        {
          // Open results file in default text editor
          System.Diagnostics.Process.Start(resultsLogPath);
        }
        else
        {
          MessageBox.Show("No race results found yet. Results will be saved after races finish.",
            "Race Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      catch (Exception ex)
      {
        _logger.Error($"Error opening results: {ex.Message}");
        MessageBox.Show($"Error opening results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion

    #region Market List Navigation

    private void listViewMarkets_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewMarkets.SelectedItems.Count == 0)
        return;

      var selectedItem = listViewMarkets.SelectedItems[0];
      var marketInfo = selectedItem.Tag as MarketInfo;

      if (marketInfo == null)
      {
        _logger.Error("listViewMarkets_SelectedIndexChanged: marketInfo is null");
        return;
      }

      _logger.Info($"User selected market: {marketInfo.MarketName} ({marketInfo.MarketId})");
      LoadMarket(marketInfo);
    }

    private void LoadMarket(MarketInfo marketInfo)
    {
      try
      {
        _logger.Info($"Loading market: {marketInfo.MarketName} ({marketInfo.MarketId})");

        // Check if already loaded
        if (marketPages.ContainsKey(marketInfo.MarketId))
        {
          _logger.Info("Market already loaded - switching to existing tab");
          // Just switch to it
          currentMarketPage = marketPages[marketInfo.MarketId];
          currentMarketId = marketInfo.MarketId;

          if (currentMarketPage != null && tabControlMarkets.TabPages.Contains(currentMarketPage))
          {
            tabControlMarkets.SelectTab(currentMarketPage);

            // Clear and refresh the external bets list for this market
            listViewBets.Items.Clear();

            // Try to display bets - if market is settled, this might fail gracefully
            try
            {
              currentMarketPage.DisplayBets();
            }
            catch (Exception ex)
            {
              _logger.Warn($"Could not refresh bets for settled market: {ex.Message}");
            }
          }
          else
          {
            _logger.Error("Market page exists in dictionary but not in tab control - removing");
            marketPages.Remove(marketInfo.MarketId);
            // Fall through to create new market page
          }

          return;
        }

        _logger.Info("Creating new market page");

        // Create new market page - MarketTabPage is a TabPage, so we need to use the tab control
        MarketTabPage mtp = new MarketTabPage(marketInfo.MarketId, marketInfo.StartTime, marketInfo.MarketName);

        if (mtp == null)
        {
          _logger.Error("Failed to create market page - constructor returned null");
          return;
        }

        // Store it
        marketPages[marketInfo.MarketId] = mtp;
        currentMarketId = marketInfo.MarketId;
        currentMarketPage = mtp;

        // Set our external bets panel
        if (listViewBets != null)
        {
          mtp.SetExternalBetsListView(listViewBets);
        }

        // Add to the tab control and switch to it
        tabControlMarkets.TabPages.Add(mtp);
        tabControlMarkets.SelectedTab = mtp;

        // Force handle creation
        var handle = mtp.Handle;

        _logger.Info("Filling market prices");
        try
        {
          mtp.FillMarketPrices();
        }
        catch (Exception ex)
        {
          _logger.Error($"Error filling market prices: {ex.Message}");
          // Continue anyway - market might be settled
        }

        _logger.Info("Market page created successfully - waiting for auto-start");

#if !SIMULATION
        try
        {
          mtp.DisplayBets();
        }
        catch (Exception ex)
        {
          _logger.Warn($"Could not display bets: {ex.Message}");
        }
#endif
      }
      catch (Exception ex)
      {
        _logger.Error("Error loading market: " + ex.ToString());
        MessageBox.Show(this, "Error loading market: " + ex.Message + "\n\nSee log for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void TabControlMarkets_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        if (tabControlMarkets.SelectedTab is MarketTabPage selectedMarket)
        {
          _logger.Info($"Tab changed to market: {selectedMarket.Text}");

          // Find the marketId from the dictionary
          string foundMarketId = null;
          foreach (var kvp in marketPages)
          {
            if (kvp.Value == selectedMarket)
            {
              foundMarketId = kvp.Key;
              break;
            }
          }

          if (foundMarketId != null)
          {
            // Update current references
            currentMarketPage = selectedMarket;
            currentMarketId = foundMarketId;

            // Clear and refresh the external bets list for this market
            listViewBets.Items.Clear();
            currentMarketPage.DisplayBets();
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error($"Error in TabControlMarkets_SelectedIndexChanged: {ex}");
      }
    }

    private void ShowMarket(string marketId)
    {
      _logger.Info($"ShowMarket: {marketId}");

      currentMarketId = marketId;
      if (marketPages.ContainsKey(marketId))
      {
        currentMarketPage = marketPages[marketId];
        tabControlMarkets.SelectTab(currentMarketPage);

        // Clear and refresh the external bets list for this market
        listViewBets.Items.Clear();
        currentMarketPage.DisplayBets();

        _logger.Info($"Selected tab for market {marketId}");
      }
      else
      {
        _logger.Error($"Market {marketId} not found in marketPages");
      }
    }

    #endregion

    #region Custom Drawing for Modern Look

    private void listViewMarkets_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
      e.DrawDefault = true;
    }

    private void listViewMarkets_DrawItem(object sender, DrawListViewItemEventArgs e)
    {
      // Let DrawSubItem handle the drawing
      e.DrawDefault = false;
    }

    private void listViewMarkets_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
      var textColor = e.Item.Selected
        ? Color.White
        : Color.FromArgb(60, 60, 60);

      var backColor = e.Item.Selected
        ? Color.FromArgb(0, 120, 215)
        : Color.FromArgb(250, 250, 250);

      using (var brush = new SolidBrush(backColor))
      {
        e.Graphics.FillRectangle(brush, e.Bounds);
      }

      var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;

      var font = e.ColumnIndex == 0
        ? new Font("Segoe UI", 8.5f, FontStyle.Bold)
        : e.Item.Font;

      TextRenderer.DrawText(e.Graphics, e.SubItem.Text, font, e.Bounds, textColor, flags);

      if (e.ColumnIndex == 0)
        font.Dispose();
    }

    private void listViewBets_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
      using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
      {
        e.Graphics.FillRectangle(brush, e.Bounds);
      }

      TextRenderer.DrawText(
        e.Graphics,
        e.Header.Text,
        new Font("Segoe UI", 8.5f, FontStyle.Bold),
        e.Bounds,
        Color.FromArgb(200, 200, 200),
        TextFormatFlags.Left | TextFormatFlags.VerticalCenter
      );
    }

    private void listViewBets_DrawItem(object sender, DrawListViewItemEventArgs e)
    {
      // Let DrawSubItem handle the drawing
      e.DrawDefault = false;
    }

    private void listViewBets_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
      var backColor = e.ItemIndex % 2 == 0
        ? Color.FromArgb(250, 250, 250)
        : Color.FromArgb(245, 245, 245);

      // Color code by bet type
      Color textColor = Color.Black;
      if (e.ColumnIndex == 1) // Type column
      {
        if (e.SubItem.Text == "Back")
          textColor = Color.FromArgb(0, 102, 204); // Blue
        else if (e.SubItem.Text == "Lay")
          textColor = Color.FromArgb(220, 53, 69); // Red
      }
      else if (e.ColumnIndex == 2) // Status column
      {
        if (e.SubItem.Text == "Matched")
          textColor = Color.FromArgb(40, 167, 69); // Green
        else if (e.SubItem.Text == "Unmatched")
          textColor = Color.FromArgb(255, 165, 0); // Orange
        else if (e.SubItem.Text == "Replaced")
          textColor = Color.FromArgb(108, 117, 125); // Grey
      }
      else
      {
        textColor = Color.FromArgb(60, 60, 60);
      }

      using (var brush = new SolidBrush(backColor))
      {
        e.Graphics.FillRectangle(brush, e.Bounds);
      }

      var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
      TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, textColor, flags);
    }

    #endregion

    #region Add Market

    private void toolStripButtonAddMarket_Click (object sender, EventArgs e)
    {
      try
      {
        FormMarkets fm = new FormMarkets ();
        fm.OnMarketSelected += fm_OnMarketSelected;
        fm.ShowDialog (this);
      }
      catch (Exception ex)
      {
        MessageBox.Show (ex.Message, "Add Market", MessageBoxButtons.OK, MessageBoxIcon.Error);
        _logger.Error (ex.ToString ());
      }
    }

    private void fm_OnMarketSelected (string marketId, DateTime startTime, string marketName)
    {
      try
      {
        AddMarketToList(marketId, startTime, marketName);
      }
      catch (Exception ex)
      {
        _logger.Error (ex.ToString ());
      }
    }

    private void DisplayWallets ()
    {
      if (InvokeRequired)
      {
        Invoke (new MethodInvoker (DisplayWallets));
        return;
      }

      UpdateWallets ();
    }

    #endregion Add Market

    private void toolStripButtonSettings_Click (object sender, EventArgs e)
    {
      try
      {
        FormSettings fs = new FormSettings ();
        fs.ShowDialog (this);
      }
      catch (Exception ex)
      {
        MessageBox.Show (ex.Message, "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        _logger.Error (ex.ToString ());
      }
    }

    private void toolStripButtonWalletsRefresh_Click (object sender, EventArgs e)
    {
      DisplayWallets ();
    }

    private void toolStripButtonAddMarket2_Click (object sender, EventArgs e)
    {
      try
      {
        // Show the auto-market selector dialog, but don't auto-add - user chooses
        FormMarket2 fm = new FormMarket2 (null);
        if (fm.ShowDialog(this) == DialogResult.OK)
        {
          // Markets will be added via the OnMarketSelected event
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show (ex.Message, "Add Auto Market", MessageBoxButtons.OK, MessageBoxIcon.Error);
        _logger.Error (ex.ToString ());
      }
    }

    public void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
    {
      Globals.UnhandledException ((Exception) e.ExceptionObject);
    }

    public void Application_ThreadException (object sender, ThreadExceptionEventArgs e)
    {
      Globals.UnhandledException (e.Exception);
    }

    #region Backward Compatibility Support

    // Keep these for backward compatibility with other code that might reference them
    private TabControl tabControlMarkets = new TabControl();
    private Panel panelRunners = new Panel();

    #endregion
  }

  // Helper class to store market information
  internal class MarketInfo
  {
    public string MarketId { get; set; }
    public DateTime StartTime { get; set; }
    public string MarketName { get; set; }
  }
}

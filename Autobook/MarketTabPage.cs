using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;

using System.Threading;
using BetfairNgClient.Json;
using BetfairNgClient.Json.Enums;
using PthLog;

namespace Autobook
{
    public class MarketTabPage : TabPage
    {
        // wait for cross matching to be set to true (IP RefreshInterval 200 * 5 = 1 sec * 10 sec = 50 
        private const int MaxCrossMatchingCounter = 3;
        //private int MinutesToStart = AppSettings.BeforeStart;
        private const int checkStartTimeRate = 60; // in seconds

        private readonly string marketId;
        private readonly int exchangeId;
        private readonly List<MarketCatalogue> market;
        private readonly string marketName;

        #region UI

        private Panel panelRunners;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButtonStart;
        private ToolStripButton toolStripButtonStop;
        private ToolStripButton toolStripButtonClose;
        private ListView listViewBets;
        private ListView externalListViewBets = null; // External bets list view from FormMain
        //private System.Windows.Forms.ColumnHeader columnHeaderBetType;
        private ColumnHeader columnHeaderBetType;
        private ColumnHeader columnHeaderBetOdds;
        private ColumnHeader columnHeaderBetSize;
        private ColumnHeader columnHeaderBetTime;
        private BookView bookView;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ColumnHeader columnHeaderName;
        private ColumnHeader columnHeaderStatus;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton toolStripButtonDebug;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton toolStripButtonXMatching;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripLabel toolStripLabelLiability;

        #endregion UI

        private readonly Dictionary<long, MarketView2> views = new Dictionary<long, MarketView2>();
        private readonly List<long> tradeoutBetsPlaced = new List<long>();
        private readonly ManualResetEvent stopHandling = new ManualResetEvent(false);
        //private ManualResetEvent stopRefreshHandling = new ManualResetEvent (false);
        private Thread handlingThread;
        private bool isInPlay;
        private readonly Dictionary<long, double> smallLayAmounts = new Dictionary<long, double>();
        private readonly Dictionary<long, double> smallBackAmounts = new Dictionary<long, double>();
        private HashSet<long> filteredRunnerIds = new HashSet<long>();
        private readonly Dictionary<string, DateTime> betPlacedTime = new Dictionary<string, DateTime>(); // Track when bets were placed
        private DateTime lastBetPlacementTime = DateTime.MinValue; // Track when last bet placement occurred
        private readonly Dictionary<long, double> totalPayoutExposure = new Dictionary<long, double>(); // Track total payout actually placed per selection
        private readonly Dictionary<long, double> intendedPayoutTotal = new Dictionary<long, double>(); // Track total payout we intended to place per selection
        private readonly Dictionary<long, double> matchedByRunner = new Dictionary<long, double>(); // Track matched payout per selection for catch-up logic
        private readonly object matchedByRunnerLock = new object(); // Thread-safe access to matchedByRunner
        private bool isCatchupMode = false; // True when P&L < liability triggers catch-up betting
        private volatile bool isPlacingBets = false; // Flag to prevent multiple PlaceBet threads running simultaneously
        private const double PRICE_DRIFT_MULTIPLIER = 1.2; // Multiplier for (price-1) to determine cancel threshold (~10 ticks, ~5% probability change)
        //private int totalBets = 0;
        //private Thread _refreshThread;
        private readonly Logger logger;
        //private object _lockDisplayBets = new object ();
        //public event MethodInvoker OnDisplayWallet;
        //private int RefreshRate = 1000;
        private MarketProfitAndLoss pandls;
        private bool isPrProfitReached;
        private bool isIpProfitReached;
        private readonly DateTime startTime;
        private readonly System.Threading.Timer startTimer;
        private bool isStarted;

        public DateTime StartTime => startTime;
        public bool IsStarted => isStarted;
        public bool IsInPlay => isInPlay;
        public MarketStatusEnum MarketStatus => marketStatus;
        public bool IsRaceFinished => isInPlay && marketStatus == MarketStatusEnum.SUSPENDED;
        private readonly object forLocking = new object();
        private readonly object forLockingPAndL = new object();
        private readonly object _forLockingStatusChanges = new object();
        //private BetfairE.MarketStatusEnum _previousMarketStatus = BetfairE.MarketStatusEnum.INACTIVE;
        private MarketStatusEnum marketStatus = MarketStatusEnum.INACTIVE;

        //private delegate void PlaceBetDelegate (RunnerPrices[] runnerPrices, BetType type, double amount);
        //private int nbpIndex = 0;
        private double dynamicLiability = -1000000000000.0D;
        private double usedLiability;
        private FormMarketDebug formMarketDebug;
        private bool hasLiabilityBeenExceeded;
        private bool isLiabilityInitialized;
        private double bestAllGreenLowest = 0.0D;  // Tracks the best (highest) lowest P&L when all selections are green
        private readonly List<long> runnersStatusChangeList;
        private int crossMatchingCounter;
        private DateTime _lastSetLiability = DateTime.Now;
        private bool _isCrossMatchingEnabled = true;

        public void SetExternalBetsListView(ListView externalBetsView)
        {
            externalListViewBets = externalBetsView;
            logger.Info("External bets ListView set");
        }

        public MarketTabPage(string marketId, DateTime startTime, string marketName)
        {
            try
            {
                InitializeComponent();
                this.marketId = marketId;
                exchangeId = Globals.BETFAIR_EXCHANGE_UK;
                this.startTime = startTime;

                var marketFilter = new MarketFilter { MarketIds = new HashSet<string> { marketId } };
                var marketProjections = new HashSet<MarketProjectionEnum>
              {
                  MarketProjectionEnum.EVENT,
                  MarketProjectionEnum.RUNNER_DESCRIPTION
              };
                market = Globals.Exchange.ListMarketCatalogue(exchangeId, marketFilter, marketProjections,
                                                              MarketSortEnum.FIRST_TO_START);
                if (market.Count == 0)
                {
                    throw new Exception("MarketTabPage: cannot get market data for market id " + marketId);
                }
                this.marketName = marketName;
                Globals.SetMarket(market[0]);
                Globals.CheckNetworkStatusIsConnected();
                Text = marketName;
                string mn = marketName.Replace(':', '_').Replace('/', '_').Replace('.', '_').Trim();
                string filename = Globals.MakeLogName(mn + "_" + market[0].MarketId);
                logger = LogManager.Current.AddLogger
                  (filename + ".txt", filename + ".txt", "%date [%-16thread] %-5level - %message%n");

                string readSettings = AppSettings.ReadSettings();
                if (!String.IsNullOrEmpty(readSettings))
                    logger.Error("Constructor: failed to read settings: " + readSettings);

                runnersStatusChangeList = new List<long>();

                TimeSpan toStart = startTime - DateTime.Now.ToUniversalTime();
                if (toStart.TotalMilliseconds > 0)
                    startTimer = new System.Threading.Timer
                      (CheckStartTime, null, 5000, checkStartTimeRate * 1000);
            }
            catch (Exception ex)
            {
                Globals.UnhandledException(ex);
                //_logger.Error (ex.ToString ());
            }
        }

        private void InitializeComponent()
        {
            this.listViewBets = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetOdds = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBetTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonClose = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonXMatching = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonDebug = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelLiability = new System.Windows.Forms.ToolStripLabel();
            this.panelRunners = new System.Windows.Forms.Panel();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewBets
            // 
            this.listViewBets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderBetType,
            this.columnHeaderStatus,
            this.columnHeaderBetOdds,
            this.columnHeaderBetSize,
            this.columnHeaderBetTime});
            this.listViewBets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewBets.FullRowSelect = true;
            this.listViewBets.GridLines = true;
            this.listViewBets.Location = new System.Drawing.Point(0, 0);
            this.listViewBets.Name = "listViewBets";
            this.listViewBets.Size = new System.Drawing.Size(528, 630);
            this.listViewBets.TabIndex = 1;
            this.listViewBets.UseCompatibleStateImageBehavior = false;
            this.listViewBets.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 200;
            // 
            // columnHeaderBetType
            // 
            this.columnHeaderBetType.Text = "Type";
            this.columnHeaderBetType.Width = 50;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 88;
            // 
            // columnHeaderBetOdds
            // 
            this.columnHeaderBetOdds.Text = "Odds";
            this.columnHeaderBetOdds.Width = 56;
            // 
            // columnHeaderBetSize
            // 
            this.columnHeaderBetSize.Text = "Size";
            this.columnHeaderBetSize.Width = 48;
            // 
            // columnHeaderBetTime
            // 
            this.columnHeaderBetTime.Text = "Time";
            this.columnHeaderBetTime.Width = 80;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonStart,
            this.toolStripSeparator1,
            this.toolStripButtonStop,
            this.toolStripSeparator2,
            this.toolStripButtonClose,
            this.toolStripSeparator3,
            this.toolStripButtonXMatching,
            this.toolStripSeparator4,
            this.toolStripButtonDebug,
            this.toolStripSeparator5,
            this.toolStripLabelLiability});
            this.toolStrip2.Location = new System.Drawing.Point(3, 3);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1004, 25);
            this.toolStrip2.TabIndex = 33;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.Image = global::Autobook.Properties.Resources.Play1Hot_32;
            this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Size = new System.Drawing.Size(51, 22);
            this.toolStripButtonStart.Text = "Start";
            this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonStart_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.Enabled = false;
            this.toolStripButtonStop.Image = global::Autobook.Properties.Resources.Stop32;
            this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Size = new System.Drawing.Size(51, 22);
            this.toolStripButtonStop.Text = "Stop";
            this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonStop_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonClose
            // 
            this.toolStripButtonClose.Image = global::Autobook.Properties.Resources.WindowClose;
            this.toolStripButtonClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClose.Name = "toolStripButtonClose";
            this.toolStripButtonClose.Size = new System.Drawing.Size(56, 22);
            this.toolStripButtonClose.Text = "Close";
            this.toolStripButtonClose.ToolTipText = "Close this Window";
            this.toolStripButtonClose.Click += new System.EventHandler(this.toolStripButtonClose_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonXMatching
            // 
            this.toolStripButtonXMatching.Checked = true;
            this.toolStripButtonXMatching.CheckOnClick = true;
            this.toolStripButtonXMatching.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonXMatching.Image = global::Autobook.Properties.Resources.algorithm;
            this.toolStripButtonXMatching.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonXMatching.Name = "toolStripButtonXMatching";
            this.toolStripButtonXMatching.Size = new System.Drawing.Size(90, 22);
            this.toolStripButtonXMatching.Text = "X-Matching";
            this.toolStripButtonXMatching.ToolTipText = "Cross-Matching enable/disable";
            this.toolStripButtonXMatching.CheckedChanged += new System.EventHandler(this.toolStripButtonXMatching_CheckedChanged);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonDebug
            //
            this.toolStripButtonDebug.Image = global::Autobook.Properties.Resources.debug;
            this.toolStripButtonDebug.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonDebug.Name = "toolStripButtonDebug";
            this.toolStripButtonDebug.Size = new System.Drawing.Size(62, 22);
            this.toolStripButtonDebug.Text = "Debug";
            this.toolStripButtonDebug.ToolTipText = "Show Debug Data";
            this.toolStripButtonDebug.Click += new System.EventHandler(this.toolStripButtonDebug_Click);
            //
            // toolStripSeparator5
            //
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripLabelLiability
            //
            this.toolStripLabelLiability.Name = "toolStripLabelLiability";
            this.toolStripLabelLiability.Size = new System.Drawing.Size(80, 22);
            this.toolStripLabelLiability.Text = "Liability: £0.00";
            this.toolStripLabelLiability.ForeColor = System.Drawing.Color.Red;
            this.toolStripLabelLiability.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            //
            // panelRunners
            //
            this.panelRunners.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelRunners.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRunners.Name = "panelRunners";
            this.panelRunners.TabIndex = 32;
            //
            // MarketTabPage
            //
            this.Controls.Add(this.panelRunners);
            this.Controls.Add(this.toolStrip2);
            this.Location = new System.Drawing.Point(4, 22);
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(1010, 671);
            this.UseVisualStyleBackColor = true;
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        void toolStripButtonDebug_Click(object sender, EventArgs e)
        {
            toolStripButtonDebug.Enabled = false;

            formMarketDebug = new FormMarketDebug(this);
            formMarketDebug.OnDebugClosed += FormMarketDebugOnDebugClosed;
            formMarketDebug.Show(this);
        }

        void FormMarketDebugOnDebugClosed(object sender, EventArgs e)
        {
            formMarketDebug = null;
            toolStripButtonDebug.Enabled = true;
        }

        private void CheckStartTime(object state)
        {
            try
            {
                TimeSpan toStart = startTime - DateTime.Now.ToUniversalTime();
                if ((toStart.TotalMinutes < AppSettings.BeforeStart) && (toStart.TotalMinutes > 0))
                {
                    startTimer.Dispose();
                    logger.Info($"Auto Start, less than {AppSettings.BeforeStart} to go");
                    logger.Info("Auto Start, reload runners list");
                    FillMarketPrices();
                    Invoke(new MethodInvoker(Start));
                }
            }
            catch (Exception ex)
            {
                Globals.UnhandledException(ex);
            }
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        public void Start()
        {
            try
            {
                //nbpIndex = (AppSettings.IsNextBestPrice) ? 1 : 0;
                //nbpIndex = 0;
                //_logger.Info ("Start: lock enter");
                lock (forLocking)
                {
                    if (isStarted)
                    {
                        logger.Info("Start: already started");
                        return;
                    }
                    isStarted = true;
                }

                //_logger.Info ("Start: lock release");
                isPrProfitReached = false;
                isIpProfitReached = false;
                if (!InitializeLiability())
                    return;
                UpdatePrices();

                // Update UI buttons safely (may not be visible yet)
                if (InvokeRequired)
                {
                    try
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            toolStripButtonStart.Enabled = false;
                            toolStripButtonStop.Enabled = true;
                        }));
                    }
                    catch
                    {
                        // Ignore if control not created yet
                    }
                }
                else
                {
                    toolStripButtonStart.Enabled = false;
                    toolStripButtonStop.Enabled = true;
                }

                stopHandling.Reset();
                logger.Info($"Start: {marketName} back {BackAmount} lay {LayAmount}");
                logger.Info($"Profit targets IP {AppSettings.IpProfit}, PR {AppSettings.PrProfit}");
                handlingThread = new Thread(HandlingProc) { IsBackground = true, Name = "HandlingProc" };
                handlingThread.Start();
                //if (totalBets > 0)
                //StartRefreshThread ();
            }
            catch (Exception ex)
            {
                Globals.UnhandledException(ex);
            }
        }

        /*
            private void StartRefreshThread ()
            {
              //int rate = AppSettings.IPRefreshInterval * 10;
              //RefreshRate = (rate <= 10000) ? 10000 : rate;
              RefreshRate = (RefreshInterval <= 1000) ? 1000 : RefreshInterval;
              _stopRefreshHandling.Reset ();
              if (_refreshThread == null)
              {
                _refreshThread = new Thread (HandlingRefreshProc);
                _refreshThread.IsBackground = true;
                _refreshThread.Name = "HandlingRefreshProc";
                _refreshThread.Start ();
              }
              else
                _logger.Error ("StartRefreshThread: there is already a running instance");
            }

            private void StopRefreshThread ()
            {
              _stopRefreshHandling.Set ();
              if (_refreshThread != null)
              {
                _refreshThread.Join ((int) (RefreshRate * 1.5D));
                _refreshThread = null;
              }
            }
        */

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        public void Stop()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(Stop));
                    return;
                }
                //_logger.Info ("Stop: lock enter");
                lock (forLocking)
                {
                    if (!isStarted)
                    {
                        logger.Info("Stop: already stopped");
                        return;
                    }
                    isStarted = false;
                }
                //_logger.Info ("Stop: lock release");

                isLiabilityInitialized = false;
                startTimer?.Dispose();
                toolStripButtonStart.Enabled = true;
                toolStripButtonStop.Enabled = false;
                stopHandling.Set();
                //StopRefreshThread ();
                handlingThread?.Join((int)(RefreshInterval * 1.5d));
                // update p&l
                if (marketStatus == MarketStatusEnum.OPEN)
                    DisplayProfitAndLoss();
                UpdatePrices(true);

                // Log race results
                LogRaceResults();
            }
            catch (Exception ex)
            {
                Globals.UnhandledException(ex);
            }
        }

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            formMarketDebug?.Close();

            Stop();
            ((TabControl)Parent).TabPages.Remove(this);
        }

        private void LogRaceResults()
        {
            try
            {
                // Create results log file path in application directory
                string resultsLogPath = Path.Combine(
                    Application.StartupPath,
                    "RaceResults.log"
                );

                using (StreamWriter sw = new StreamWriter(resultsLogPath, true))
                {
                    sw.WriteLine("================================================================================");
                    sw.WriteLine($"Race: {marketName}");
                    sw.WriteLine($"Market ID: {marketId}");
                    sw.WriteLine($"Start Time: {startTime:yyyy-MM-dd HH:mm:ss}");
                    sw.WriteLine($"Stopped: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sw.WriteLine($"Status: {marketStatus}");
                    sw.WriteLine("--------------------------------------------------------------------------------");
                    sw.WriteLine("Runner Results (P&L):");
                    sw.WriteLine("--------------------------------------------------------------------------------");

                    // Get current P&L from display
                    lock (forLockingPAndL)
                    {
                        if (views != null && views.Count > 0)
                        {
                            double totalPL = 0;
                            var orderedViews = views.OrderByDescending(v => v.Value.ProfitLiability);
                            foreach (var kvp in orderedViews)
                            {
                                string runnerName = GetRunnerName(kvp.Key);
                                double pl = kvp.Value.ProfitLiability;
                                sw.WriteLine($"  {runnerName,-30} P&L: {pl,10:F2}");
                                totalPL += pl;
                            }
                            sw.WriteLine("--------------------------------------------------------------------------------");
                            sw.WriteLine($"  {"TOTAL",-30} P&L: {totalPL,10:F2}");
                        }
                        else
                        {
                            sw.WriteLine("  No P&L data available");
                        }
                    }

                    sw.WriteLine("================================================================================");
                    sw.WriteLine();
                }

                logger.Info($"Race results logged to {resultsLogPath}");
            }
            catch (Exception ex)
            {
                logger.Error($"Error logging race results: {ex.Message}");
            }
        }

        private void HandlingProc()
        {
            try
            {
                int count = 0;
                while (!stopHandling.WaitOne(RefreshInterval, false))
                {
                    //try
                    //{
                    //System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
                    if (marketStatus == MarketStatusEnum.OPEN)
                    {
                        //sw.Start ();
                        DisplayBets();
                        //sw.Stop ();
                        //Console.WriteLine ("DisplayBets (rounded): {0} ms", sw.ElapsedMilliseconds);
                        //sw.Start ();
                        UpdatePrices();
                        //sw.Stop ();
                        //Console.WriteLine ("UpdatePrices (rounded): {0} ms", sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        if (count == 5)
                        {
                            count = 0;
                            //sw.Start ();
                            UpdatePrices();
                            //sw.Stop ();
                            //Console.WriteLine ("UpdatePrices (rounded): {0} ms", sw.ElapsedMilliseconds);
                        }
                        count++;
                    }
                    //}
                    //catch (Exception ex)
                    //{
                    //  Globals.UnhandledException (ex);
                    //}
                }
            }
            catch (Exception ex)
            {
                Globals.UnhandledException(ex);
                Stop();
            }
            finally
            {
                handlingThread = null;
                stopHandling.Reset();
            }
        }

        /*
            private void HandlingRefreshProc ()
            {
              try
              {
                _logger.Info ("HandlingRefreshProc started");
                while (!_stopRefreshHandling.WaitOne (RefreshRate, false))
                {
                  //DateTime before = DateTime.Now;
                  //try
                  //{
                    DisplayBets ();
                  //}
                  //catch (Exception ex)
                  //{
                  //  Globals.UnhandledException (ex);
                  //}
                  //TimeSpan after = DateTime.Now - before;
                  //Console.WriteLine ("{1} DisplayBets {0} ms",
                  //  market.name, after.TotalMilliseconds);
                }
                _logger.Info ("HandlingRefreshProc stopped");
              }
              catch (Exception ex)
              {
                Globals.UnhandledException (ex);
              }
              finally
              {
                _refreshThread = null;
                _stopRefreshHandling.Reset ();
              }
            }
        */

        private bool InitializeLiability()
        {
            if (isLiabilityInitialized)
                return true;

            if (IsDynamicLiability)
            {
                pandls = Globals.Exchange.GetMarketProfitAndLoss(exchangeId, marketId);
                if ((pandls != null) && (pandls.ProfitAndLosses.Count > 0))
                {
                    if (!Globals.GetMaxMinLiability(pandls.ProfitAndLosses, out var pandlMin, out _, out var isAnyLiability))
                    {
                        logger.Error("InitializeLiability: GetMaxMinLiability failed");
                        return false;
                    }
                    dynamicLiability = Liability;
                    if (isAnyLiability)
                    {
                        //if (Math.Sign (pandlMin.ifWin) < 0)
                        //{

                        // dealing with negative values => test inverted
                        // if outstanding liability (-4.85) < Liability setting (-25)
                        // then dynamicLiability = Liability setting
                        // else dynamicLiability = outstanding liability
                        // ---> should these 2 lines not be reversed because of -tive values?
                        // i.e does the app. consider -4.85 > -25

                        if (pandlMin.IfWin < Liability)
                            dynamicLiability = pandlMin.IfWin;
                        dynamicLiability += DynamicLiabilityVariant;
                        //}
                    }
                    usedLiability = dynamicLiability;
                    logger.Info("InitializeLiability: dynamic liability set to " +
                                 dynamicLiability.ToString(Globals.DOUBLE_FORMAT));
                }
                else
                {
                    logger.Error("InitializeLiability: GetMarketProfitAndLoss failed");
                    return false;
                }
            }

            isLiabilityInitialized = true;

            return true;
        }

        public void UpdatePrices()
        {
            UpdatePrices(false);
        }

        public void UpdatePrices(bool isForcedRefresh)
        {
            try
            {
                //DateTime before = DateTime.Now;
                ISet<PriceDataEnum> priceData = new HashSet<PriceDataEnum>();
                priceData.Add(PriceDataEnum.EX_ALL_OFFERS);

                var priceProjection = new PriceProjection
                {
                    PriceData = priceData,
                    Virtualise = AppSettings.IsPriceVirtualise
                };
                var marketBook = Globals.Exchange.ListMarketBook(exchangeId, marketId, priceProjection);
                marketStatus = marketBook.Status;

                //TimeSpan after = DateTime.Now - before;
                //Console.WriteLine ("{0} GetMarketPrices {1} ms",
                //  marketId, after.TotalMilliseconds);

                Globals.CheckNetworkStatusIsConnected();
                if ((!isForcedRefresh) && (marketBook.Status == MarketStatusEnum.CLOSED))
                {
                    logger.Info($"market {marketName} closed, stop updating");
                    Stop();
                    return;
                }

                /*
                        if (!_isInPlay)
                          if (marketPrices.delay > 0)
                          {
                            _isInPlay = (marketPrices.marketStatus == BetfairE.MarketStatusEnum.ACTIVE);
                            if (_isInPlay)
                              _logger.Info (string.Format ("market {0} goes in play: back {1} lay {2}",
                                BetfairClient.Framework.Exchange.GetMarketName (market), BackAmount, LayAmount));
                          }
                */

                bool isInPlayLocal = marketBook.IsInplay;
                if (isInPlay != isInPlayLocal)
                {
                    isInPlay = isInPlayLocal;
                    if (!isInPlay)
                    {
                        if (logger.IsInfoEnabled)
                            logger.Info($"market {marketName} not in play: {marketBook.Status.ToString()}");
                    }
                    else
                    {
                        if (logger.IsInfoEnabled)
                            logger.Info($"market {marketName} goes in play: back {BackAmount} lay {LayAmount} low back {AppSettings.LowBack} low lay {AppSettings.LowLay}");

                        string readSettings = AppSettings.ReadSettings();
                        if (!String.IsNullOrEmpty(readSettings))
                            logger.Error("market goes in play: failed to read settings: " + readSettings);

                        ResetSmallAmounts();

                        // Reset best all-green tracking for in-play static liability
                        bestAllGreenLowest = 0.0D;

                        //InitializeLProfitsAndLosses();

                        // 1)
                        // When the event goes in play if dynamic liability is selected, it needs to do the following check
                        // (only applicable if liability is red -tive)
                        // is in-play liability setting > used liability ?
                        // if true, set used liability = in-play liability setting
                        // 2)
                        // If dynamic liability is selected: market goes in play
                        // calculate biggest red (lowest p&l)
                        // dynamic liability = biggest red
                        // used liability = biggest red
                        // is used liability < inplay liability setting
                        // if true used liability & dynamic liability = inplay liability setting
                        // 3)
                        // When using it for golf though - if you remember, the app has to be reset every 24hrs because the keep alive token expires,
                        // and since most golf tournaments are over 4 days, it could very well be that the app. could be started with all selections all green,
                        // so if it could be set to lowest green in this situation it would be really good.

                        // 4)
                        // Currently the app starts dynamic liability immediately when it should wait until the liability setting is reached before starting.
                        // E.g. if liability setting = -50
                        // if all selections >= -50, then dynamic should not start (I.e continue as normal until below)
                        // if / when any selection <-50 then dynamic should start.
                    }

                    FillMarketPrices();
                }

                if (isInPlay)
                {
                    /* You may remember us discussing the problem with horses being withdrawn - but Betfair leave them in the market in-play.
   This distorts the market because lay bets placed by the app have to be refunded after the market finishes when Betfair remove the runner.
   This can leave the app all red to sometimes large amounts - this for a commercial program is a problem.
   Betfair do however turn off their cross-matching algorithm & they now have a flag in the api to say if it is on or off.
   If the app(all versions) could check this(if 'false' then don't go in-play) it will allow the user to remove the runner
   (right-click) and then start manually if watching otherwise the app will place no bets if unattended and stop the above happening
*/

                    if (!marketBook.IsCrossMatching)
                    {
                        if (_isCrossMatchingEnabled)
                        {
                            crossMatchingCounter++;
                            logger.Info($"Cross-matching algorithm flag = false, waiting {crossMatchingCounter}/{MaxCrossMatchingCounter} ...");

                            if (crossMatchingCounter >= MaxCrossMatchingCounter)
                            {
                                logger.Info("Cross-matching algorithm flag = false, stopping ...");
                                Stop();
                                return;
                            }
                        }
                    }
                }

                if ((!isForcedRefresh) && (isIpProfitReached && isInPlay))
                {
                    logger.Info("profit reached, stopping");
                    Stop();
                    return;
                }

                var runners = new List<Runner>();
                foreach (var runnerPrice in marketBook.Runners)
                {
                    if (views.ContainsKey(runnerPrice.SelectionId))
                    {
                        // skip disabled runners
                        lock (_forLockingStatusChanges)
                        {
                            if (runnersStatusChangeList.Contains(runnerPrice.SelectionId))
                                continue;
                            if (runnerPrice.Status == RunnerStatusEnum.REMOVED)
                            {
                                runnersStatusChangeList.Add(runnerPrice.SelectionId);
                                continue;
                            }
                        }

                        var runner = new Runner(runnerPrice, IsSwopPrice, IsNextBestPrice);
                        //_logger.Info ("UpdatePrices: lock enter " + GetRunnerName (runnerPrice.selectionId));
                        lock (forLockingPAndL)
                        {
                            views[runnerPrice.SelectionId].UpdateOdds(runner);
                            // Update filtered status
                            views[runnerPrice.SelectionId].SetFiltered(filteredRunnerIds.Contains(runnerPrice.SelectionId));
                        }
                        //_logger.Info ("UpdatePrices: lock release " + GetRunnerName (runnerPrice.selectionId));
                        runners.Add(runner);
                    }
                    else
                        logger.Error("UpdatePrices: cannot find selection " + GetRunnerName(runnerPrice.SelectionId));
                }
                if ((isPrProfitReached) && (!isInPlay))
                    // stop placing bets, wait for market going InPlay
                    DisplayBookPercentages(runners, false);
                else if (!marketBook.IsCrossMatching && _isCrossMatchingEnabled)
                    DisplayBookPercentages(runners, false);
                else
                    DisplayBookPercentages(runners, marketBook.Status == MarketStatusEnum.OPEN);
            }
            catch (Exception ex)
            {
                //  _logger.Error (ex.ToString ());
                Globals.UnhandledException(ex);
            }
        }

        /// <summary>
        /// Yes, I think that is the conclusion we both came to - that no matter what value p/l is set to, it gets overwritten with the zero when the 1st bet is placed.
        /// => disabled.
        /// </summary>
        private void InitializeLProfitsAndLosses()
        {
            if (pandls != null && pandls.ProfitAndLosses.All(pl => pl.IfWin == 0.0D))
            {
                foreach (var pandl in pandls.ProfitAndLosses)
                {
                    pandl.IfWin = Liability;
                    logger.Info($"{GetRunnerName(pandl.SelectionId)} p&l initialized to {pandl.IfWin.ToString(Globals.DOUBLE_FORMAT)}.");
                }
            }
        }

        private void ResetSmallAmounts()
        {
            foreach (var sbak in smallBackAmounts.Keys.ToList())
            {
                smallBackAmounts[sbak] = 0.0d;
            }
            foreach (var slak in smallLayAmounts.Keys.ToList())
            {
                smallLayAmounts[slak] = 0.0d;
            }
        }

        public void FillMarketPrices()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(FillMarketPrices));
                    return;
                }

                //_logger.Info ("FillMarketPrices: lock enter");
                lock (forLockingPAndL)
                {
                    views.Clear();
                }
                //_logger.Info ("FillMarketPrices: lock release");

                ISet<PriceDataEnum> priceData = new HashSet<PriceDataEnum>();
                priceData.Add(PriceDataEnum.EX_ALL_OFFERS);

                var priceProjection = new PriceProjection
                {
                    PriceData = priceData,
                    Virtualise = AppSettings.IsPriceVirtualise
                };
                var marketBook = Globals.Exchange.ListMarketBook(exchangeId, marketId, priceProjection);

                // Check if market has valid data (could be closed/settled)
                if (marketBook == null || marketBook.Runners == null || marketBook.Runners.Count == 0)
                {
                    logger.Warn($"FillMarketPrices: No runners data available for market {marketId} - market may be closed/settled");
                    return;
                }

                panelRunners.SuspendLayout();
                panelRunners.Controls.Clear();
                panelRunners.AutoScroll = true;
                // add in reverse order
                AddBookPrices();
                var runners = new List<Runner>();
                for (int i = marketBook.Runners.Count - 1; i >= 0; i--)
                {
                    // skip disabled runners
                    lock (_forLockingStatusChanges)
                    {
                        if (runnersStatusChangeList.Contains(marketBook.Runners[i].SelectionId))
                            continue;
                        if (marketBook.Runners[i].Status == RunnerStatusEnum.REMOVED)
                        {
                            runnersStatusChangeList.Add(marketBook.Runners[i].SelectionId);
                            continue;
                        }
                    }

                    var runner = new Runner(marketBook.Runners[i], IsSwopPrice, IsNextBestPrice);
                    AddRunnerPrices(Globals.Exchange.FindRunner(marketBook, marketBook.Runners[i].SelectionId), runner);
                    runners.Add(runner);
                }
                /*
                foreach (BetfairE.RunnerPrices runnerPrice in marketPrices.runnerPrices)
                {
                  AddRunnerPrices (Exchange.FindRunner (market, runnerPrice.selectionId), runnerPrice);
                }
                */
                DisplayBookPercentages(runners, false);
                //string info = string.Empty;
                //if (market == null)
                //{
                //  info = "; No names; exceeded API throttle";
                //}
                //columnHeaderSelection.Text = string.Format ("Selections: {0}{1}", marketPrices.runnerPrices.Length, info);
                Globals.CheckNetworkStatusIsConnected();
            }
            catch (Exception ex)
            {
                Globals.UnhandledException(ex);
                //  if (Globals.OnKeepAliveError != null)
                //    Globals.OnKeepAliveError (ex.Message);
                //  //MessageBox.Show (ex.Message, "Fill Market Prices", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                panelRunners.ResumeLayout();
            }
        }

        private double GetAmount(double price, double amountAvailable)
        {
            double amount = 0.0d;

            switch (LayBetMode)
            {
                case LayBetMode.Payout:
                    amount = price * amountAvailable;
                    break;

                case LayBetMode.Liability:
                    if (price > 1.0d)
                        amount = (price - 1.0d) * amountAvailable;
                    break;
            }

            return amount;
        }

        private void TradeOutCheck(IReadOnlyCollection<Runner> runnerPrices, Runner selectedRunner, bool isLayPrice)
        {
            //try
            //{

            var price = isLayPrice ? selectedRunner.LayPrice : selectedRunner.BackPrice;

            logger.Info($"TradeOutCheck: price {price.ToString(Globals.DOUBLE_FORMAT)} < topr {AppSettings.TradeOutTrigger}, {GetRunnerName(selectedRunner.SelectionId)} isLay {isLayPrice.ToString()}");
            // 2. Check if selection pl > 0
            if (views.ContainsKey(selectedRunner.SelectionId))
            {
                if (!tradeoutBetsPlaced.Contains(selectedRunner.SelectionId))
                {
                    if (views[selectedRunner.SelectionId].ProfitLiability > 0.0D)
                    {
                        logger.Info("TradeOutCheck: sel p&l = " +
                          views[selectedRunner.SelectionId].ProfitLiability.ToString(Globals.DOUBLE_FORMAT));
                        // 3. Check if any other selections pl < 0z
                        double toam = double.MaxValue;
                        long toamSelectionId = -1;

                        double topa = views[selectedRunner.SelectionId].ProfitLiability *
                          (AppSettings.TradeOutPercent / 100.0D);
                        bool isAllGreen = IsAllGreen();
                        if (isAllGreen)
                        {
                            logger.Info("TradeOutCheck: all selections are green");
                            InternalPlaceBet(topa, selectedRunner.SelectionId);
                            return;
                        }

                        //_logger.Info ("TradeOutCheck: lock enter");
                        lock (forLockingPAndL)
                        {
                            foreach (KeyValuePair<long, MarketView2> kvp in views)
                            {
                                if (kvp.Value.ProfitLiability < 0)
                                {
                                    // 4. Save trade out selection current profit (cupr)
                                    // Save selection pl that has largest potential loss (biggest red) � toam
                                    // (ignore selection if current price is 1000)
                                    if (kvp.Value.ProfitLiability < toam)
                                    {
                                        bool isSelection1000 =
                                            (from runner in runnerPrices
                                             where runner.SelectionId == kvp.Key
                                             select runner.LastPriceMatched == Globals.MaxPrice).FirstOrDefault();

                                        if (!isSelection1000)
                                        {
                                            toam = kvp.Value.ProfitLiability;
                                            toamSelectionId = kvp.Key;
                                            logger.Info("TradeOutCheck: largest current potential loss (biggest red) = " +
                                              toam.ToString(Globals.DOUBLE_FORMAT));
                                        }
                                        else
                                            logger.Info("TradeOutCheck: ignore selection price = 1000");
                                    }
                                }
                            }
                        }
                        //_logger.Info ("TradeOutCheck: lock release");

                        if (toamSelectionId >= 0)
                        {
                            // 5. Calculate trade out % amount (topa) = (cupr * topp)
                            //double topa = _views[selectedRP.selectionId].ProfitLiability *
                            //(AppSettings.TradeOutPercent / 100.0D);
                            toam = Math.Abs(toam);
                            logger.Info($"TradeOutCheck: topa = {topa.ToString(Globals.DOUBLE_FORMAT)} toam = {toam.ToString(Globals.DOUBLE_FORMAT)}");
                            // Is toam > topa
                            // If yes, then set topa = toam : goto 7
                            if (toam > topa)
                            {
                                topa = toam;
                                logger.Info("TradeOutCheck: topa forced to toam = " + topa.ToString(Globals.DOUBLE_FORMAT));
                            }
                            // If topa*(topr-1) > cupr then topa=cupr*(topr-1)
                            // 200*1=200>100, topa=100*1 = 100
                            if ((topa * (AppSettings.TradeOutPrice - 1.0D)) > views[selectedRunner.SelectionId].ProfitLiability)
                            {
                                topa = views[selectedRunner.SelectionId].ProfitLiability * (AppSettings.TradeOutPrice - 1.0D);
                                logger.Info("TradeOutCheck: topa forced to cupr*(topr-1) = " + topa.ToString(Globals.DOUBLE_FORMAT));
                            }
                            // 6. Is topa > 2
                            if (topa <= 2.0D)
                            {
                                topa = 2.0D;
                                logger.Info("TradeOutCheck: topa forced to 2.0");
                            }
                            // 7. Place lay bet topa @ topr
                            InternalPlaceBet(topa, selectedRunner.SelectionId);
                            /*
                            BetfairE.PlaceBets[] betsArray = new BetfairE.PlaceBets[1];
                            betsArray[nbpIndex] = new BetfairE.PlaceBets ();
                            //if (isLayPrice)
                            betsArray[nbpIndex].price = AppSettings.TradeOutPrice;
                            //else
                            //  betsArray[0].price = (double) Exchange.IncrementPrice ((decimal) selectedRP.bestPricesToBack[nbpIndex].price);
                            betsArray[0].betCategoryType = BetfairE.BetCategoryTypeEnum.E;
                            betsArray[0].betPersistenceType = BetfairE.BetPersistenceTypeEnum.NONE;
                            betsArray[0].marketId = market.marketId;
                            betsArray[0].size = Math.Round (topa, 2);
                            betsArray[0].selectionId = selectedRP.selectionId;
                            betsArray[0].bspLiability = 0.0d;
                            betsArray[0].betType = BetfairE.BetTypeEnum.L;

                            _logger.Info (string.Format ("TradeOutCheck: placing lay bet {0} @ {1}",
                              betsArray[0].size.ToString (), betsArray[0].price.ToString (Globals.DoubleFormat)));

                                          IList<BetfairE.PlaceBetsResult> placeResults = null;
                                          //IList<BetfairE.PlaceBetsResult> placeResults = Globals.Exchange.PlaceBets (exchangeId, betsArray);
                            if ((_isInPlay) && (AppSettings.IsCancelBets))
                              // Cancel & Stop = trade out bet is cancelled if not matched straight away
                              // Stop = trade out bet is not cancelled
                              if (AppSettings.TradeOutEnd == TradeOutEndMode.CancelAllStop)
                                HandleBetResult (placeResults);
                            //if ((placeResults != null) && (placeResults.Count >= 1))
                            //{
                            //  _logger.Info ("TradeOutCheck: trade-out bet not matched");
                            //  return;
                            //}
                            if ((placeResults != null) && (placeResults[0].resultCode != BetfairE.PlaceBetsResultEnum.OK))
                            {
                              _logger.Info ("TradeOutCheck: aborting, trade-out bet not placed, status = " +
                                placeResults[0].resultCode.ToString ());
                              return;
                            }

                            // only 1 tradeout bet allowed per selection
                            _views[selectedRP.selectionId].IsTradeOutBetPlaced = true;
                            */
                            // 8. An option to either let app. Continue trading after trade-out bet has been matched or
                            //    to cancel all outstanding bets, and stop trading if trade-out bet has been matched and all selections are �green� (p/l >0)
                            /*
                            if (AppSettings.TradeOutEnd != TradeOutEndMode.Continue)
                            {
                              int?[] markets = new int?[1];
                              markets[0] = marketId;
                              // check all selections are �green� (p/l >0)
                              bool isAllGreen = IsAllGreen ();

                              if (isAllGreen)
                              {
                                _logger.Info ("TradeOutCheck: all selections are green, cancelling all unmatched bets");
                                Globals.Exchange.CancelBetsByMarket (exchangeId, markets);
                                _logger.Info ("TradeOutCheck: recheck all selections are green");
                                isAllGreen = IsAllGreen ();
                                if (isAllGreen)
                                {
                                  _logger.Info ("TradeOutCheck: stopping, all selections are green");
                                  Stop ();
                                }
                                else
                                  _logger.Info ("TradeOutCheck: cannot stop, not all selections are green");
                              }
                            }
                            */
                            if (AppSettings.TradeOutEnd != TradeOutEndMode.Continue)
                            {
                                // check all selections are �green� (p/l >0)
                                isAllGreen = IsAllGreen();

                                if (isAllGreen)
                                {
                                    logger.Info("TradeOutCheck: all selections are green, cancelling all unmatched bets");
                                    Globals.Exchange.CancelOrders(exchangeId, marketId);
                                    logger.Info("TradeOutCheck: recheck all selections are green");
                                    isAllGreen = IsAllGreen();
                                    if (isAllGreen)
                                    {
                                        logger.Info("TradeOutCheck: stopping, all selections are green");
                                        Stop();
                                    }
                                    else
                                        logger.Info("TradeOutCheck: cannot stop, not all selections are green");
                                }
                                else
                                    logger.Info("TradeOutCheck: cannot stop, not all selections are green");
                            }
                            else
                            {
                                // get p&l to avoid double tradeout bet placement when tradeout mode = Continue
                                //Globals.Exchange.GetMarketProfitAndLoss (exchangeId, marketId);
                            }
                        }
                        else
                            logger.Error("TradeOutCheck: no largest current potential loss (biggest red)");
                    }
                    else
                        logger.Info("TradeOutCheck: p&l not positive " +
                          views[selectedRunner.SelectionId].ProfitLiability);
                }
                else
                    logger.Info("TradeOutCheck: trade out bet already placed for this selection");
            }
            else
                logger.Error("TradeOutCheck: cannot find p&l");
            //}
            //catch (Exception ex)
            //{
            //  _logger.Error ("TradeOutCheck: " + ex.ToString ());
            //}
            Globals.CheckNetworkStatusIsConnected();
        }

        private void InternalPlaceBet(double size, long selectionId)
        {
            var limitOrder = new LimitOrder();

            var bet = new PlaceInstruction();

            //if (isLayPrice)
            limitOrder.Price = AppSettings.TradeOutPrice;
            //else
            //  betsArray[0].price = (double) Exchange.IncrementPrice ((decimal) selectedRP.bestPricesToBack[nbpIndex].price);
            limitOrder.Size = Math.Round(size, 2);
            limitOrder.PersistenceType = BetPersistence;

            bet.OrderType = OrderTypeEnum.LIMIT;
            bet.LimitOrder = limitOrder;
            bet.SelectionId = selectionId;
            bet.Side = SideEnum.LAY;

            logger.Info($"InternalPlaceBet: placing lay bet {limitOrder.Size.ToString(Globals.DOUBLE_FORMAT)} @ {limitOrder.Price.ToString(Globals.DOUBLE_FORMAT)}");

            // Trade-out disabled
            logger.Info("InternalPlaceBet: Trade-out is disabled");
            return;

            #pragma warning disable CS0162 // Unreachable code
            var betsToPlace = new List<PlaceInstruction> { bet };

#if NOBET
            PlaceExecutionReport placeResults = null;
#else
            var placeResults = Globals.Exchange.PlaceOrders(exchangeId, marketId, betsToPlace);
#endif
            if ((isInPlay) && (AppSettings.IsCancelBets))
                // Cancel & Stop = trade out bet is canceled if not matched straight away
                // Stop = trade out bet is not canceled
                if (AppSettings.TradeOutEnd == TradeOutEndMode.CancelAllStop)
                    HandleBetResult(placeResults);
            //if ((placeResults != null) && (placeResults.Count >= 1))
            //{
            //  _logger.Info ("TradeOutCheck: trade-out bet not matched");
            //  return;
            //}
            if ((placeResults != null) && (placeResults.Status != ExecutionReportStatusEnum.SUCCESS))
            {
                logger.Info("InternalPlaceBet: aborting, trade-out bet not placed, status = " + placeResults.Status);
                return;
            }

            // only 1 tradeout bet allowed per selection
            tradeoutBetsPlaced.Add(selectionId);
            logger.Info("InternalPlaceBet: no more bets will be allowed on this selection " + GetRunnerName(selectionId));
        }

        private bool IsAllGreen()
        {
            //try
            //{
            var pals = Globals.Exchange.GetMarketProfitAndLoss(exchangeId, marketId);
            Globals.CheckNetworkStatusIsConnected();
            bool isAllGreen = (pals.ProfitAndLosses.Count > 0);
            foreach (var pandl in pals.ProfitAndLosses)
            {
                logger.Info($"IsAllGreen: selection {GetRunnerName(pandl.SelectionId)} p&l {pandl.IfWin.ToString(Globals.DOUBLE_FORMAT)}");
                if (pandl.IfWin <= 0.0D)
                {
                    isAllGreen = false;
                    break;
                }
            }
            logger.Info("IsAllGreen: " + isAllGreen);

            return isAllGreen;
            //}
            //catch (Exception ex)
            //{
            //  _logger.Error ("IsAllGreen: " + ex);
            //return false;
            //}
        }

        /// <summary>
        /// Filter runners to bring book% down to target overround by removing highest priced runners
        /// </summary>
        private HashSet<long> FilterRunnersByBookPercentage(List<Runner> runnerPrices, double currentBookPercent, double targetOverround)
        {
            var excludedRunnerIds = new HashSet<long>();

            // FIRST: Always exclude runners with price >= 100 (mandatory)
            var runnersToExclude = runnerPrices.Where(r => !double.IsNaN(r.LayPrice) && r.LayPrice >= 100.0).ToList();
            foreach (var runner in runnersToExclude)
            {
                excludedRunnerIds.Add(runner.SelectionId);
                logger.Info($"FilterRunners: Excluding {GetRunnerName(runner.SelectionId)} at price {runner.LayPrice:F2} (price >= 100, mandatory exclusion)");
            }

            // Recalculate book% after removing >= 100 runners
            double bookPercent = runnerPrices
                .Where(r => !double.IsNaN(r.LayPrice) && !excludedRunnerIds.Contains(r.SelectionId))
                .Sum(r => Globals.GetBookShare(r.LayPrice) * 100.0d);

            logger.Info($"FilterRunners: Book% after removing >= 100 runners: {bookPercent:F2}%");

            // If book% is already at or below target, no additional filtering needed
            if (bookPercent <= (100.0d + targetOverround))
            {
                logger.Info($"FilterRunners: Book% {bookPercent:F2}% <= target {100.0d + targetOverround:F2}%, no additional filtering needed");
                return excludedRunnerIds;
            }

            // Group remaining runners by price (highest to lowest), excluding already excluded runners
            var runnersByPrice = runnerPrices
                .Where(r => !double.IsNaN(r.LayPrice) && !excludedRunnerIds.Contains(r.SelectionId))
                .GroupBy(r => r.LayPrice)
                .OrderByDescending(g => g.Key)
                .ToList();

            foreach (var priceGroup in runnersByPrice)
            {
                // Calculate total book share for all runners at this price
                double groupBookShare = priceGroup.Sum(r => Globals.GetBookShare(r.LayPrice) * 100.0d);

                // Check if removing this group would bring us below target
                double newBookPercent = bookPercent - groupBookShare;

                if (newBookPercent < (100.0d + targetOverround))
                {
                    // Removing this group would put us below target, so keep these runners
                    logger.Info($"FilterRunners: Cannot remove {priceGroup.Count()} runner(s) at price {priceGroup.Key:F2} " +
                               $"(group share {groupBookShare:F2}%) - would reduce book% to {newBookPercent:F2}% which is < target {100.0d + targetOverround:F2}%");
                    break;
                }

                // Remove this group
                foreach (var runner in priceGroup)
                {
                    excludedRunnerIds.Add(runner.SelectionId);
                    logger.Info($"FilterRunners: Excluding {GetRunnerName(runner.SelectionId)} at price {runner.LayPrice:F2} " +
                               $"(share {Globals.GetBookShare(runner.LayPrice) * 100.0d:F2}%)");
                }

                bookPercent = newBookPercent;
                logger.Info($"FilterRunners: Book% after removing group: {bookPercent:F2}%");

                // If we've reached the target, stop
                if (bookPercent <= (100.0d + targetOverround))
                    break;
            }

            if (excludedRunnerIds.Count > 0)
            {
                logger.Info($"FilterRunners: Excluded {excludedRunnerIds.Count} runner(s), " +
                           $"final book% {bookPercent:F2}% (target: {100.0d + targetOverround:F2}%)");
            }

            return excludedRunnerIds;
        }

        private void DisplayBookPercentages(List<Runner> runnerPrices, bool isPlaceBet)
        {
            double layBook = 0.0d;
            double backBook = 0.0d;
            bool isBackAvailable = true;
            //bool isBackValid = true;
            bool isSkipBackBet = false;
            bool isSkipLayBet = false;
            double minBackSize = double.MaxValue;
            double minLaySize = double.MaxValue;
            int layPriceCount = 0;
            double minSelectionPrice = double.MaxValue;

            if (runnerPrices.Count > 0)
                minSelectionPrice = runnerPrices.Min(r => r.LayPrice);

            // First pass: calculate initial lay book% to check if filtering is needed
            double initialLayBook = 0.0d;
            foreach (Runner runner in runnerPrices)
            {
                if (!double.IsNaN(runner.LayPrice))
                {
                    if (!IsSkipHighLaySelection(runner.LayPrice, runner.SelectionId, minSelectionPrice))
                    {
                        initialLayBook += Globals.GetBookShare(runner.LayPrice);
                    }
                }
            }
            initialLayBook *= 100.0d;

            // Apply filtering if book% exceeds target
            double targetOverMargin = OverMargin;
            if (AppSettings.IpUnderOverMode == UnderOverMode.Dynamic)
                targetOverMargin = (targetOverMargin * runnerPrices.Count) - 0.01D;

            var excludedRunnerIds = FilterRunnersByBookPercentage(runnerPrices, initialLayBook, targetOverMargin);

            // Store filtered runner IDs for UI display
            filteredRunnerIds = excludedRunnerIds;

            // Second pass: calculate final book percentages with filtered runners
            foreach (Runner runner in runnerPrices)
            {
                if (runner.LayPrice <= AppSettings.TradeOutTrigger)
                    TradeOutCheck(runnerPrices, runner, true);

                if (runner.BackPrice < (double)Globals.DecrementPrice((decimal)AppSettings.TradeOutTrigger))
                    TradeOutCheck(runnerPrices, runner, false);

                #region Back

                double amount;
                if (!double.IsNaN(runner.BackPrice))
                {
                    if (runner.BackPrice <= 1.01D)
                    {
                        logger.Info($"invalid back: {GetRunnerName(runner.SelectionId)} {runner.BackPrice.ToString(Globals.DOUBLE_FORMAT)}");
                        //isBackValid = false;
                    }
                    backBook += Globals.GetBookShare(runner.BackPrice);
                    if (isInPlay)
                    {
                        if (runner.BackPrice <= AppSettings.LowBack)
                        {
                            logger.Info($"low back: {GetRunnerName(runner.SelectionId)} {runner.BackPrice.ToString(Globals.DOUBLE_FORMAT)}");
                            isSkipBackBet = true;
                        }
                    }
                    // if either next best, swop or both routines was being used, it would always place a bet to the actual total bet amount because
                    // it is actually placing a bet that is not physically available on the system (a new bet in reality) -
                    // so if bet amount was 20, it would always place a bet for 20.
                    if (IsNextBestPrice || IsSwopPrice)
                        amount = BackAmount;
                    else
                        amount = GetAmount(runner.BackPrice, runner.BackAmount);
                    if (amount < minBackSize)
                        minBackSize = amount;
                }
                else
                {
                    //DisplayDebugMessage (string.Format ("back invalid {0}: {1} ",
                    //  DateTime.Now.ToLongTimeString (), GetRunnerName (rp.selectionId)));
                    logger.Error("back prices not available for " + GetRunnerName(runner.SelectionId));
                    isBackAvailable = false;
                }

                #endregion Back

                #region Lay

                if (!double.IsNaN(runner.LayPrice))
                {
                    // Skip if filtered out by book percentage filter
                    if (excludedRunnerIds.Contains(runner.SelectionId))
                    {
                        logger.Info($"Skipping filtered runner {GetRunnerName(runner.SelectionId)} at price {runner.LayPrice:F2}");
                        continue;
                    }

                    layPriceCount++;
                    //if (minSelectionPrice > runner.LayPrice)
                    //  minSelectionPrice = runner.LayPrice;
                    // When the app. shows the book %'s, would we be able to have an option to display the book % on the lay side for just the selections within the range of the high lay setting?
                    // In effect it would show the lay book % just for the selections it can actually place a lay bet on, and ignore the others.
                    // It would just be an option for the lay side, and would need to be in settings - if say we had an option to choose either 'full %' which would have the app. performing as it does at present or 'high lay %' which would calculate a book % based on the high lay price.

                    if (IsSkipHighLaySelection(runner.LayPrice, runner.SelectionId, minSelectionPrice))
                        continue;

                    layBook += Globals.GetBookShare(runner.LayPrice);
                    if (isInPlay)
                    {
                        if (runner.LayPrice <= AppSettings.LowLay)
                        {
                            logger.Info($"low lay: {GetRunnerName(runner.SelectionId)} {runner.LayPrice.ToString(Globals.DOUBLE_FORMAT)}");
                            isSkipLayBet = true;
                        }
                    }
                    if (IsNextBestPrice || IsSwopPrice)
                        amount = LayAmount;
                    else
                        amount = GetAmount(runner.LayPrice, runner.LayAmount);
                    if (amount < minLaySize)
                        minLaySize = amount;
                }
                else
                {
                    logger.Error("lay prices not available for " + GetRunnerName(runner.SelectionId));
                }

                #endregion Lay
            }

            backBook *= 100.0d;
            layBook *= 100.0d;
            /*
            #if UNMATCHABLE_BET
                  backBook *= 0.95d;
                  layBook *= 0.95d;
            #endif
            */
            DisplayBook(backBook, layBook, isBackAvailable);

            if (isPlaceBet)
            {
                //The app. needs to ignore any selections that have no back price available, and calculate a book percentage just on selections with prices available
                //if (isBackAvailable && !isSkipBackBet && isBackValid)
                if (!isSkipBackBet)
                {
                    double underMargin = UnderMargin;
                    if (AppSettings.IpUnderOverMode == UnderOverMode.Dynamic)
                        underMargin = (UnderMargin * runnerPrices.Count) - 0.01D;
                    if (backBook <= (100.0d - underMargin))
                    //if (((!AppSettings.IsNextBestPrice) && (backBook <= (100.0d - UnderMargin))) ||
                    //((AppSettings.IsNextBestPrice) && (backBook > (100.0d + OverMargin))))
                    {
                        if (isInPlay)
                            PlaceBet(new PlaceBetParam(runnerPrices, BetType.Back, Math.Min(BackAmount, minBackSize), minSelectionPrice, backBook, excludedRunnerIds));
                        else
                        {
                            if (backBook >= (100.0d - AppSettings.PrVariant))
                            {
                                //BeginInvoke (new PlaceBetDelegate (PlaceBet), new object[] { runnerPrices, BetType.Back, Math.Min (BackAmount, minBackSize) });
                                PlaceBet(new PlaceBetParam(runnerPrices, BetType.Back, Math.Min(BackAmount, minBackSize), minSelectionPrice, backBook, excludedRunnerIds));
                                //PlaceBet (runnerPrices, BetType.Back, Math.Min (BackAmount, minBackSize));
                            }
                            else
                                logger.Info($"Back book {backBook.ToString(Globals.DOUBLE_FORMAT)} not between {(100.0d - AppSettings.PrVariant).ToString(Globals.DOUBLE_FORMAT)} and {(100.0d - underMargin).ToString(Globals.DOUBLE_FORMAT)}, bet skipped");
                        }
                    }
                    else
                    {
                        //if (AppSettings.IsNextBestPrice)
                        //  _logger.Info (string.Format ("Back book {0} <= {1}, bet skipped",
                        //    backBook.ToString (Globals.DoubleFormat), (100.0d + OverMargin).ToString (Globals.DoubleFormat)));
                        //else
                        logger.Info($"Back book {backBook.ToString(Globals.DOUBLE_FORMAT)} > {(100.0d - underMargin).ToString(Globals.DOUBLE_FORMAT)}, bet skipped");
                    }
                }

                double overMargin = OverMargin;
                if (AppSettings.IpUnderOverMode == UnderOverMode.Dynamic)
                    overMargin = (overMargin * runnerPrices.Count) - 0.01D;
                if (layBook >= (100.0d + overMargin))
                //if (((!AppSettings.IsNextBestPrice) && (layBook >= (100.0d + OverMargin))) ||
                //((AppSettings.IsNextBestPrice) && (layBook < (100.0d - UnderMargin))))
                {
                    if (layPriceCount >= 2)
                    {
                        if (!isSkipLayBet)
                        {
                            // Only trigger PlaceBet if not already running
                            if (!isPlacingBets)
                            {
                                if (isInPlay)
                                {
                                    // Launch PlaceBet on separate thread to avoid blocking refresh loop
                                    ThreadPool.QueueUserWorkItem(PlaceBet, new PlaceBetParam(runnerPrices, BetType.Lay, LayAmount, minSelectionPrice, layBook, excludedRunnerIds));
                                }
                                else
                                {
                                    if (layBook <= (100.0d + AppSettings.PrVariant))
                                        ThreadPool.QueueUserWorkItem(PlaceBet, new PlaceBetParam(runnerPrices, BetType.Lay, LayAmount, minSelectionPrice, layBook, excludedRunnerIds));
                                    else
                                        logger.Info($"Lay book {layBook.ToString(Globals.DOUBLE_FORMAT)} not between {(100.0d + overMargin).ToString(Globals.DOUBLE_FORMAT)} and {(100.0d + AppSettings.PrVariant).ToString(Globals.DOUBLE_FORMAT)}");
                                }
                            }
                        }
                    }
                    else
                        logger.Info("Lay price count < 2 ");
                }
                else
                {
                    //if (AppSettings.IsNextBestPrice)
                    //  _logger.Info (string.Format ("Lay book {0} >= {1}",
                    //    layBook.ToString (Globals.DoubleFormat), (100.0d - UnderMargin).ToString (Globals.DoubleFormat)));
                    //else
                    logger.Info($"Lay book {layBook.ToString(Globals.DOUBLE_FORMAT)} < {(100.0d + overMargin).ToString(Globals.DOUBLE_FORMAT)}");
                }
            }
        }

        /// <summary>
        /// Check if high lay selection should be skipped
        /// </summary>
        /// <param name="layPrice">selection lay price</param>
        /// <param name="selectionId">selection id (to display selection price)</param>
        /// <param name="minSelectionPrice">minimum selection price</param>
        /// <returns>true if the selection should be skipped</returns>
        private bool IsSkipHighLaySelection(double layPrice, long selectionId, double minSelectionPrice)
        {
            bool result = false;

            switch (HighLayMode)
            {
                case HighLayMode.Static:
                    if (layPrice >= HighLay)
                    {
                        if (!IsIgnoreHighLay)
                        {
                            logger.Info($"IsSkipHighLaySelection: {GetRunnerName(selectionId)} Static High Lay skipped, price {layPrice}");
                            result = true;
                        }
                        else
                        {
                            if (isInPlay && IsLayBookPercentage)
                            {
                                logger.Info($"IsSkipHighLaySelection: {GetRunnerName(selectionId)} Static High Lay Percent skipped, price {layPrice}");
                                result = true;
                            }
                            else if (layPrice <= HighLayIncrease)
                            {
                                // HighLay exceeded, Ignore HighLay set but HighLay Increase not exceeded
                                // do nothing, go on check other conditions
                            }
                            else
                            {
                                logger.Info($"IsSkipHighLaySelection: {GetRunnerName(selectionId)} Static High Lay Increase skipped, price {layPrice}");
                                result = true;
                            }
                        }
                    }
                    break;

                case HighLayMode.Dynamic:
                    if (layPrice >= (minSelectionPrice * HighLayMultiplier))
                    {
                        logger.Info($"IsSkipHighLaySelection: {GetRunnerName(selectionId)} Dynamic High Lay Multiplier skipped, price {layPrice}");
                        result = true;
                    }
                    else
                    {
                        // IsLayBookPercentage setting is not defined in Pre-Race mode
                        if (isInPlay && IsLayBookPercentage)
                        {
                            if (layPrice > HighLay)
                            {
                                logger.Info($"IsSkipHighLaySelection: {GetRunnerName(selectionId)} Dynamic High Lay Percent skipped, price {layPrice}");
                                result = true;
                            }
                        }
                    }
                    break;
            }

            return result;
        }

        private void DisplayBook(double backBook, double layBook, bool isBackAvailable)
        {
            //Font bookFont = new Font (listViewRunners.Font, FontStyle.Regular);

            bookView.UpdateBook(backBook, layBook, isBackAvailable);
        }

        private void AddRunnerPrices(BetfairNgClient.Json.Runner runner, Runner runnerPrices)
        {
            //string selectionName = (runner == null) ? runnerPrices.selectionId.ToString () : runner.name;
            var view = new MarketView2(runner, runnerPrices, GetRunnerName(runner.SelectionId));
            view.OnRunnerStatusChanged += view_OnRunnerStatusChanged;
            //_logger.Info ("AddRunnerPrices: lock enter " + GetRunnerName (runner.selectionId));
            lock (forLockingPAndL)
            {
                views.Add(runner.SelectionId, view);
            }
            //_logger.Info ("AddRunnerPrices: lock release " + GetRunnerName (runner.selectionId));
            view.Dock = DockStyle.Top;
            panelRunners.Controls.Add(view);
        }

        void view_OnRunnerStatusChanged(long selectionId, RunnerStatusChange statusChange)
        {
            lock (_forLockingStatusChanges)
            {
                switch (statusChange)
                {
                    case RunnerStatusChange.Disabled:
                        if (!runnersStatusChangeList.Contains(selectionId))
                            runnersStatusChangeList.Add(selectionId);
                        break;

                    case RunnerStatusChange.Enabled:
                        if (runnersStatusChangeList.Contains(selectionId))
                            runnersStatusChangeList.Remove(selectionId);
                        break;
                }
            }
        }

        private void AddBookPrices()
        {
            bookView = new BookView { Dock = DockStyle.Top };
            if (InvokeRequired)
                Invoke(new MethodInvoker(delegate { panelRunners.Controls.Add(bookView); }));
            else
                panelRunners.Controls.Add(bookView);
        }

        public string GetRunnerName(long selectionId)
        {
            var runner = market[0].Runners.SingleOrDefault(r => r.SelectionId == selectionId);
            var runnerName = (runner == null) ? selectionId.ToString() : runner.RunnerName;

            return runnerName;
        }

        private void AddOrderToList(string runnerName, string betType, string status, double price, double size, string betId = "")
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { AddOrderToList(runnerName, betType, status, price, size, betId); }));
            }
            else
            {
                var item = new ListViewItem(runnerName);
                item.SubItems.Add(betType);
                item.SubItems.Add(status);
                item.SubItems.Add(price.ToString(Globals.DOUBLE_FORMAT));
                item.SubItems.Add(size.ToString(Globals.DOUBLE_FORMAT));
                item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));

                // Color code by status
                if (status == "Matched")
                    item.BackColor = System.Drawing.Color.LightGreen;
                else if (status == "Unmatched")
                    item.BackColor = System.Drawing.Color.LightYellow;
                else if (status == "Replaced")
                    item.BackColor = System.Drawing.Color.LightBlue;
                else if (status == "Placed")
                    item.BackColor = System.Drawing.Color.White;

                // Store betId in tag if provided
                if (!string.IsNullOrEmpty(betId))
                    item.Tag = betId;

                var targetListView = externalListViewBets ?? listViewBets;
                targetListView.Items.Insert(0, item);

                // Limit to last 100 orders
                if (targetListView.Items.Count > 100)
                    targetListView.Items.RemoveAt(targetListView.Items.Count - 1);
            }
        }

        private void ClearOrdersList()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ClearOrdersList));
            }
            else
            {
                var targetListView = externalListViewBets ?? listViewBets;
                targetListView.Items.Clear();
            }
        }

        private void IncrementRunnerBetCount(long selectionId, double payout = 0)
        {
            if (views.ContainsKey(selectionId))
            {
                if (payout > 0)
                    views[selectionId].IncrementBetCount(payout);
                else
                    views[selectionId].IncrementBetCount();
            }
        }

        private void IncrementRunnerRepCount(long selectionId)
        {
            // IncrementRepCount removed - now using IncrementCancelledCount when bets are cancelled
        }

        #region Bets Handling

        private void HandleBetResult(PlaceExecutionReport placeResults)
        {
            if (placeResults != null)
            {
                var betsToCancel = new List<CancelInstruction>();
                foreach (var ir in placeResults.InstructionReports)
                {
                    //_logger.Info (string.Format ("HandleBetResult: {0} {1} {2} {3} {4}",
                    //  pbr.resultCode, pbr.success,
                    //  pbr.averagePriceMatched, pbr.sizeMatched, pbr.betId));
                    //if (pbr.sizeMatched <= 0.0D)
                    //{
                    var cancelBet = new CancelInstruction { BetId = ir.BetId };
                    logger.Info("HandleBetResult: added for cancelling, bet #" + ir.BetId);

                    betsToCancel.Add(cancelBet);
                    //}
                }
                if (betsToCancel.Count > 0)
                {
                    var param = new CancelBetsParam(exchangeId, betsToCancel);
                    //ThreadPool.QueueUserWorkItem (new WaitCallback (CancelBets), param);
                    CancelBets(param);
                }
            }
            else
                logger.Info("HandleBetResult: place bet failed");
        }

        /*
            void HandleBetResult (BetfairE.PlaceBets placeBet, IList<BetfairE.PlaceBetsResult> placeResults)
            {
              if ((placeResults != null) && (placeResults.Count > 0))
              {
                if (placeResults[0].resultCode == BetfairE.PlaceBetsResultEnum.OK)
                {
                  string selectionName = GetRunnerName (placeBet.selectionId);
                  if (placeResults[0].sizeMatched > 0)
                    DisplayBets ();
                  else
                    AddBetsToList (selectionName, placeBet.betType, BetfairE.BetStatusEnum.U,
                      placeBet.price, placeBet.size.Value, DateTime.Now);
                }
                else
                  DisplayDebugMessage ("PlaceBet: place bet failed " + placeResults[0].resultCode.ToString ());
              }
              else
                DisplayDebugMessage ("PlaceBet: place bet failed");
            }
        */

        private void SetNewLiability(double lowestRunnerLiability)
        {
            double newLiability = lowestRunnerLiability + DynamicLiabilityVariant;
            if (newLiability > dynamicLiability)
            {
                dynamicLiability = newLiability;
                logger.Info("PlaceBet: dynamic liability set to " +
                  dynamicLiability.ToString(Globals.DOUBLE_FORMAT));
            }
            else
                logger.Info("PlaceBet: dynamic liability not set to " +
                  newLiability.ToString(Globals.DOUBLE_FORMAT));
        }

        //void	PlaceBet (BetfairE.RunnerPrices[] runnerPrices, BetType type, double amount)
        private void PlaceBet(object state)
        {
            var param = state as PlaceBetParam;
            if (param == null)
            {
                logger.Error("PlaceBet: invalid parameter");
                return;
            }

            // Set flag to prevent concurrent PlaceBet threads
            if (isPlacingBets)
            {
                logger.Info("PlaceBet: already placing bets, skipping this cycle");
                return;
            }

            isPlacingBets = true;
            try
            {
                //if (InvokeRequired)
                //  Invoke (new PlaceBetDelegate (runnersControl1_OnPlaceBet), new object[] { runnerPrices, type });
                //else
                //{
#if SIMULATION
				ListViewItem lvi;
				listViewBets.BeginUpdate ();
#else
                PlaceInstruction placeBet;
                var bets = new List<PlaceInstruction>();
#endif

                //DateTime now = DateTime.Now;
                logger.Info($"PlaceBet {param.Type.ToString()} amount {param.Amount.ToString(Globals.DOUBLE_FORMAT)}, dynamicLiability {dynamicLiability.ToString(Globals.DOUBLE_FORMAT)}");
                if (!SetLiability(param.Type, out var isLiabilityExceeded, out var isAllGreen))
                    return;

                CurrentOrderSummaryReport unmatchedBets = null;

                switch (param.Type)
                {
                    #region Back

                    case BetType.Back:
                        if (!IsDynamicLiability)
                            if ((isLiabilityExceeded) && (BackBetDealMode == BetDealMode.NoBets))
                            {
                                logger.Info("PlaceBet: bet placement aborted, Back liability too high");
                                return;
                            }

                        //double backAmountTotal = Globals.GetBackAmountTotal (param, nbpIndex);

                        if (!isStarted)
                        {
                            logger.Info("PlaceBet: abort bets placement, thread stopped");
                            return;
                        }
                        foreach (Runner runner in param.RunnerPrices)
                        {
                            if (!isStarted)
                            {
                                logger.Info("PlaceBet: abort bets placement, thread stopped");
                                return;
                            }

                            if (double.IsNaN(runner.BackPrice))
                            {
                                logger.Info($"PlaceBet: Back {GetRunnerName(runner.SelectionId)} price not available");
                                continue;
                            }

                            if ((runner.BackPrice == Globals.MaxPrice) && isInPlay)
                            {
                                logger.Info($"PlaceBet: Maximum Back {GetRunnerName(runner.SelectionId)} price skipped");
                                continue;
                            }
                            if ((IsBackOver) && (runner.BackPrice < BackOver))
                            {
                                logger.Info($"PlaceBet: back not over: {GetRunnerName(runner.SelectionId)} {runner.BackPrice.ToString(Globals.DOUBLE_FORMAT)}");
                                continue;
                            }

                            // I don't think it needs to check for implied liability when placing back bets, just when placing lay bets.
                            // The reason being that if the app. tries to place a back bet to reduce liability on a selection but the implied liability is exceeded, it will not place the bet which is not what we want.
                            /*
                                          // make this call only if needed and only once
                                          if (unmatchedBets == null)
                                          {
                                            unmatchedBets = Globals.Exchange.GetMUBets
                                                (exchangeId, marketId, BetfairE.BetStatusEnum.U, DateTime.MinValue,
                                                BetfairE.BetsOrderByEnum.PLACED_DATE, ref record, 100);
                                          }

                                          // check that the selections current liability + unmatched liability does not exceed the liability setting
                                          impliedLiability = Globals.GetTotalImpliedLiability (rp.selectionId, unmatchedBets);

                                          // regardless of the selections current p/l the implied liability is constant, which is not really beneficial
                                          // if a selection is red then ideally we want to limit the amount of unmatched liability,
                                          // but if a selection is green, then the amount of unmatched liability could be increased
                                          if (ImpliedLiabilityPercent == 0.0D)
                                            impliedLiabilityMax = ImpliedLiability;
                                          else
                                          {
                                            //  implied liability for the current selection = implied liability setting + (p/l * %implied setting)
                                            impliedLiabilityMax = Globals.GetImpliedLiability (ImpliedLiability, ImpliedLiabilityPercent, pandls[rp.selectionId].ifWin);
                                          }
                                          if (impliedLiability > impliedLiabilityMax)
                                          {
                                            _logger.Info (string.Format ("PlaceBet: bet placement aborted for {0}, Back implied liability too high {1} > {2}",
                                                GetRunnerName (rp.selectionId),
                                                impliedLiability.ToString (Globals.DoubleFormat),
                                                impliedLiabilityMax.ToString (Globals.DoubleFormat)));
                                            continue;
                                          }
                             */
                            // liability routine check
                            // Yes, I think it makes sense to do the check each time.
                            // It just seems to be that 1st time when the dynamic liability amount changes - on dynamic,
                            // the app. should not really be in a position where it will place back bets on all runners because
                            // at least 1 selection will always be <= used liability.
                            if (isLiabilityExceeded)
                            {
                                var rpal = pandls.ProfitAndLosses.Where(pl => pl.SelectionId == runner.SelectionId).ToArray();
                                if (rpal.Any())
                                {
                                    var firstRpal = rpal[0];
                                    if (IsSkipBet(BetType.Back, firstRpal.IfWin, usedLiability, isAllGreen))
                                    {
                                        // reduce liability on selections that exceeded max liability
                                        // => skip bet for others (that are not exceeding) here
                                        logger.Info($"PlaceBet: bet placement skipped for this selection, {GetRunnerName(runner.SelectionId)} - p&l = {firstRpal.IfWin.ToString(Globals.DOUBLE_FORMAT)} - usedLiability = {usedLiability.ToString(Globals.DOUBLE_FORMAT)}");
                                        continue;
                                    }
                                }
                                else
                                {
                                    logger.Error("PlaceBet: Back, cannot find current liability " +
                                                 GetRunnerName(runner.SelectionId));
                                }
                            }

                            //switch (LayBetMode)
                            //{
                            //  case LayBetMode.Payout:
                            double backBook = Globals.GetBookShare(runner.BackPrice);
                            //    break;

                            //  case LayBetMode.Liability:
                            //    backBook = Globals.GetBookShareLiability (rp.bestPricesToBack[nbpIndex].price);
                            //    break;
                            //}

                            double betAmount;
                            if (AppSettings.DynamicStake > 0.0d)
                            {
                                var overround = Math.Abs(100.0d - param.BookPercentage);
                                betAmount = AppSettings.DynamicStake * overround;
                            }
                            else
                                betAmount = backBook * param.Amount;

                            double size = HandleSmallBackBet(runner.SelectionId, betAmount);
                            if (size == 0.0d)
                            {
                                logger.Info($"PlaceBet: Back wait for overbet used up {GetRunnerName(runner.SelectionId)} {runner.BackPrice.ToString(Globals.DOUBLE_FORMAT)} {runner.BackAmount.ToString(Globals.DOUBLE_FORMAT)}");
                                continue;
                            }

                            /*
                            1. Compute liability for all selections
                               Is liability for any selection(s) > liability setting if bets are placed?
                               If no, then place bets.
                               If yes, then check 2.
                            */
                            /*
                            if (pandls.ContainsKey (rp.selectionId))
                            {
                              double backLiability = Globals.GetLayLiability (rp.bestPricesToBack[nbpIndex].price, betAmount) -
                                (backAmountTotal - betAmount);
                              double liabilityIfPlaced = pandls[rp.selectionId].ifWin + backLiability;
                              _logger.Info (string.Format ("PlaceBet: backLiability {0} liabilityIfPlaced {1} current {2}",
                                backLiability, liabilityIfPlaced, pandls[rp.selectionId].ifWin));
                              if ((Math.Sign (liabilityIfPlaced) == -1) && (Math.Abs (liabilityIfPlaced) > Liability))
                              {
                                /* 2. Is current liability before placing these bets already > liability setting?
                                   If yes, then place no bets
                                   If no, then reduce bet amounts so liability stays <= liability settings.
                                *
                                if ((Math.Sign (pandls[rp.selectionId].ifWin) == -1) &&
                                  (Math.Abs (pandls[rp.selectionId].ifWin) > Liability))
                                {
                                  if (BackBetDealMode == BetDealMode.NoBets)
                                  {
                                    _logger.Info (string.Format ("PlaceBet: bet placement aborted, Back liability too high {0} {1} > {2}",
                                      GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (Globals.DoubleFormat), Liability.ToString (Globals.DoubleFormat)));
                                    return;
                                  }
                                  else
                                  {
                                    _logger.Info (string.Format ("PlaceBet: bet placement skipped for this selection, Back liability too high {0} {1} > {2}",
                                      GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (Globals.DoubleFormat), Liability.ToString (Globals.DoubleFormat)));
                                    continue;
                                  }
                                }
                              }
                            }
                            else
                            {
                              _logger.Info ("PlaceBet: cannot find current liability " +
                                GetRunnerName (rp.selectionId));
                            }
                            */
                            /*

                                        if (betAmount < Globals.MinBetAmount)
                                        {
                                          DisplayDebugMessage (string.Format ("PlaceBet: Back small bet, force size to min {0} {1} {2} {3}",
                                            GetRunnerName (rp.selectionId),
                                            rp.bestPricesToBack[nbpIndex].price.ToString (Globals.DoubleFormat),
                                            rp.bestPricesToBack[nbpIndex].amountAvailable.ToString (Globals.DoubleFormat), betAmount.ToString (Globals.DoubleFormat)));
                                          betAmount = Globals.MinBetAmount;
                                        }
                            */
                            size = Math.Round(size, 2);
                            logger.Info(string.Format("PlaceBet: place Back {0} {3}@{1} avail. {2}",
                              GetRunnerName(runner.SelectionId), runner.BackPrice.ToString(Globals.DOUBLE_FORMAT),
                              runner.BackAmount.ToString(Globals.DOUBLE_FORMAT), size.ToString(Globals.DOUBLE_FORMAT)));
                            placeBet = new PlaceInstruction();

#if SIMULATION
						if (size > 0.0d)
						{
							lvi = new ListViewItem (GetRunnerName (rp.selectionId));
							lvi.SubItems.Add (type.ToString ());
							lvi.BackColor = Globals.BACK_COLOUR;
							lvi.SubItems.Add ("Matched");
							lvi.SubItems.Add (rp.bestPricesToBack[nbpIndex].price.ToString (Globals.DoubleFormat));
							lvi.SubItems.Add (size.ToString ("###0.##"));
							lvi.SubItems.Add (now.ToLongTimeString ());
							var targetListView = externalListViewBets ?? listViewBets;
							targetListView.Items.Add (lvi);
						}
#else

                            var limitOrder = new LimitOrder
                            {
                                PersistenceType = BetPersistence,
                            };
#if UNMATCHABLE_BET
							size = 4.0d;
              limitOrder.Price = 1000.0d;
#else
                            limitOrder.Price = runner.BackPrice;
#endif
                            limitOrder.Size = size;
                            placeBet.LimitOrder = limitOrder;
                            // TODO ask order and persistence type to Ian
                            placeBet.OrderType = OrderTypeEnum.LIMIT;
                            //if (AppSettings.IsNextBestPrice)
                            //  // invert bet type
                            //  placeBet.betType = BetfairE.BetTypeEnum.L;
                            //else
                            placeBet.Side = SideEnum.BACK;
                            placeBet.SelectionId = runner.SelectionId;
                            bets.Add(placeBet);
#endif
                        }
                        break;

                    #endregion Back

                    #region Lay

                    case BetType.Lay:
                        if (!isStarted)
                        {
                            logger.Info("PlaceBet: abort bets placement, thread stopped");
                            return;
                        }
                        //double layAmountTotal = Globals.GetLayAmountTotal (param, LayBetMode, nbpIndex);

                        // Calculate the minimum uniform payout for all selections based on the largest price
                        double uniformPayout = CalculateMinimumUniformPayout(param.RunnerPrices, LayBetMode);

                        // CATCH-UP LOGIC: Find runner with lowest P&L and count negative runners
                        long lowestPLRunnerId = 0;
                        double lowestPL = double.MaxValue;
                        double lowestPLMatchedPayout = 0;
                        double minEligibleMatchedPayout = double.MaxValue;
                        int negativeRunnerCount = 0;

                        foreach (Runner runner in param.RunnerPrices)
                        {
                            var rpal = pandls.ProfitAndLosses.Where(pl => pl.SelectionId == runner.SelectionId).ToArray();
                            if (rpal.Any())
                            {
                                if (rpal[0].IfWin < lowestPL)
                                {
                                    lowestPL = rpal[0].IfWin;
                                    lowestPLRunnerId = runner.SelectionId;
                                }
                                if (rpal[0].IfWin < 0)
                                {
                                    negativeRunnerCount++;
                                }
                            }
                        }

                        // Check if lowest P&L runner triggers catch-up mode
                        // Trigger if: 1) P&L < liability OR 2) only one runner is negative AND we have matched bets
                        int runnersWithMatches = 0;
                        lock (matchedByRunnerLock)
                        {
                            runnersWithMatches = matchedByRunner.Count(m => m.Value > 0);
                        }

                        bool triggerCatchup = lowestPLRunnerId != 0 &&
                            (IsSkipBet(BetType.Lay, lowestPL, usedLiability, isAllGreen) ||
                             (negativeRunnerCount == 1 && lowestPL < 0 && runnersWithMatches > 1));

                        if (triggerCatchup)
                        {
                            if (!isCatchupMode)
                            {
                                string reason = (negativeRunnerCount == 1 && lowestPL < 0)
                                    ? $"only one runner negative ({negativeRunnerCount})"
                                    : "P&L < liability";
                                logger.Info($"PlaceBet: {reason}, entering CATCH-UP MODE - {GetRunnerName(lowestPLRunnerId)} p&l = {lowestPL.ToString(Globals.DOUBLE_FORMAT)} - usedLiability = {usedLiability.ToString(Globals.DOUBLE_FORMAT)}");
                                isCatchupMode = true;
                            }

                            lock (matchedByRunnerLock)
                            {
                                lowestPLMatchedPayout = matchedByRunner.ContainsKey(lowestPLRunnerId) ? matchedByRunner[lowestPLRunnerId] : 0;
                            }
                            logger.Info($"PlaceBet: CATCH-UP MODE - Lowest P&L runner {GetRunnerName(lowestPLRunnerId)} has £{lowestPLMatchedPayout:F2} matched");

                            // Find the minimum matched payout among eligible runners
                            lock (matchedByRunnerLock)
                            {
                                foreach (Runner runner in param.RunnerPrices)
                                {
                                    if (runner.SelectionId == lowestPLRunnerId)
                                        continue; // Skip the problem runner

                                    // Skip filtered runners
                                    if (param.ExcludedRunnerIds.Contains(runner.SelectionId))
                                        continue;

                                    var rpal = pandls.ProfitAndLosses.Where(pl => pl.SelectionId == runner.SelectionId).ToArray();
                                    if (rpal.Any() && !IsSkipBet(BetType.Lay, rpal[0].IfWin, usedLiability, isAllGreen))
                                    {
                                        // This runner is eligible (P&L > usedLiability and not filtered)
                                        double thisMatchedPayout = matchedByRunner.ContainsKey(runner.SelectionId) ? matchedByRunner[runner.SelectionId] : 0;
                                        if (thisMatchedPayout < minEligibleMatchedPayout)
                                        {
                                            minEligibleMatchedPayout = thisMatchedPayout;
                                        }
                                    }
                                }
                            }

                            // Check if min is within £10 of trigger (already caught up)
                            double tierThreshold = lowestPLMatchedPayout - 10.0;
                            if (minEligibleMatchedPayout >= tierThreshold)
                            {
                                logger.Info($"PlaceBet: CATCH-UP MODE - All eligible runners within £10 of trigger (min £{minEligibleMatchedPayout:F2} >= £{tierThreshold:F2}), betting on all");
                                minEligibleMatchedPayout = double.MaxValue; // Signal to bet on all eligible
                            }
                            else if (minEligibleMatchedPayout < double.MaxValue)
                            {
                                logger.Info($"PlaceBet: CATCH-UP MODE - Will only bet on runners with £{minEligibleMatchedPayout:F2} matched (gradual leveling)");
                            }
                        }
                        else
                        {
                            isCatchupMode = false;
                        }

                        foreach (Runner runner in param.RunnerPrices)
                        {
                            if (!isStarted)
                            {
                                logger.Info("PlaceBet: abort bets placement, thread stopped");
                                return;
                            }

                            // Skip if excluded by book percentage filter
                            if (param.ExcludedRunnerIds.Contains(runner.SelectionId))
                            {
                                logger.Info($"PlaceBet: Skipping filtered runner {GetRunnerName(runner.SelectionId)}");
                                continue;
                            }

                            // CATCH-UP LOGIC: Skip runners based on matched payout tiers
                            if (isCatchupMode && lowestPLRunnerId != 0)
                            {
                                // Skip the lowest P&L runner (the one in trouble)
                                if (runner.SelectionId == lowestPLRunnerId)
                                {
                                    logger.Info($"PlaceBet: CATCH-UP - Skipping {GetRunnerName(runner.SelectionId)} - lowest P&L runner (liability exceeded)");
                                    continue;
                                }

                                // For other runners, check if they're also in trouble
                                var runnerPL = pandls.ProfitAndLosses.Where(pl => pl.SelectionId == runner.SelectionId).ToArray();
                                if (runnerPL.Any())
                                {
                                    // Skip if this runner's P&L also < usedLiability (also in trouble)
                                    if (IsSkipBet(BetType.Lay, runnerPL[0].IfWin, usedLiability, isAllGreen))
                                    {
                                        logger.Info($"PlaceBet: CATCH-UP - Skipping {GetRunnerName(runner.SelectionId)} - P&L {runnerPL[0].IfWin.ToString(Globals.DOUBLE_FORMAT)} < usedLiability {usedLiability.ToString(Globals.DOUBLE_FORMAT)}");
                                        continue;
                                    }

                                    // GRADUAL LEVELING: Only bet on runners at minimum tier (unless all within £10 of trigger)
                                    double thisMatchedPayout;
                                    lock (matchedByRunnerLock)
                                    {
                                        thisMatchedPayout = matchedByRunner.ContainsKey(runner.SelectionId) ? matchedByRunner[runner.SelectionId] : 0;
                                    }

                                    // If minEligibleMatchedPayout = double.MaxValue, it means all are within £10, so bet on all
                                    if (minEligibleMatchedPayout < double.MaxValue && thisMatchedPayout != minEligibleMatchedPayout)
                                    {
                                        logger.Info($"PlaceBet: CATCH-UP - Skipping {GetRunnerName(runner.SelectionId)} - matched £{thisMatchedPayout:F2}, only betting on £{minEligibleMatchedPayout:F2} tier");
                                        continue;
                                    }
                                }
                            }


                            if (double.IsNaN(runner.LayPrice))
                            {
                                logger.Info($"PlaceBet: Lay {GetRunnerName(runner.SelectionId)} price not available");
                                continue;
                            }

                            if (runner.LayPrice == Globals.MaxPrice)
                            {
                                logger.Info($"PlaceBet: {GetRunnerName(runner.SelectionId)} Maximum Lay price skipped");
                                continue;
                            }
                            if ((IsLayOver) && (runner.LayPrice < LayOver))
                            {
                                logger.Info($"PlaceBet: lay not over: {GetRunnerName(runner.SelectionId)} {runner.LayPrice.ToString(Globals.DOUBLE_FORMAT)}");
                                continue;
                            }
                            if (IsSkipHighLaySelection(runner.LayPrice, runner.SelectionId, param.MinSelectionPrice))
                                continue;

                            double price = (LayBetMode == LayBetMode.Payout)
                              ? runner.LayPrice
                              : runner.LayPrice - 1.0D;

                            double cf = 0.0D;
                            if ((IsSmallLay) && (smallLayAmounts.ContainsKey(runner.SelectionId)))
                                cf = smallLayAmounts[runner.SelectionId];

                            var overround = Math.Abs(100.0d - param.BookPercentage);

                            // Calculate betAmount to achieve uniform payout across all selections
                            double betAmount = uniformPayout / price;

                            // Round up to ensure minimum payout requirement is met
                            betAmount = Math.Ceiling(betAmount * 100) / 100;
                            var rpal = pandls.ProfitAndLosses.Where(pl => pl.SelectionId == runner.SelectionId).ToArray();

                            // Calculate actual total payout for this bet
                            double totalPayout = betAmount * price;

                            logger.Info($"PlaceBet: lay cf {cf.ToString(Globals.DOUBLE_FORMAT)} amount {param.Amount.ToString(Globals.DOUBLE_FORMAT)} price {price.ToString(Globals.DOUBLE_FORMAT)} betAmount {betAmount.ToString(Globals.DOUBLE_FORMAT)} totalPayout {totalPayout.ToString(Globals.DOUBLE_FORMAT)} selection {GetRunnerName(runner.SelectionId)}");

                            // Check if total payout is less than Betfair minimum (should not happen with uniform payout calculation)
                            if (totalPayout < 10.0d)
                            {
                                // no bet - total payout too small
                                logger.Info($"PlaceBet: no bet placed, total payout {totalPayout.ToString(Globals.DOUBLE_FORMAT)} < 10.0");
                                if (IsSmallLay)
                                {
                                    if (!smallLayAmounts.ContainsKey(runner.SelectionId))
                                        smallLayAmounts.Add(runner.SelectionId, param.Amount);
                                    else
                                        smallLayAmounts[runner.SelectionId] += param.Amount;
                                }
                                continue;
                            }

                            // reset small amount if needed (1, otherwise made here above in place 2)
                            if (AppSettings.IpcfReset)
                                if (smallLayAmounts.ContainsKey(runner.SelectionId))
                                    smallLayAmounts.Remove(runner.SelectionId);

                            // make this call only if needed and only once
                            if (unmatchedBets == null)
                            {
                                unmatchedBets = Globals.Exchange.ListCurrentOrders
                                    (exchangeId, null, marketId, OrderProjectionEnum.EXECUTABLE, null, null, OrderByEnum.BY_BET);
                            }

                            // check that the selections current liability + unmatched liability does not exceed the liability setting

                            // regardless of the selections current p/l the implied liability is constant, which is not really beneficial
                            // if a selection is red then ideally we want to limit the amount of unmatched liability,
                            // but if a selection is green, then the amount of unmatched liability could be increased

                            double impliedLiability = Globals.GetTotalImpliedLiability(runner.SelectionId, unmatchedBets);
                            double impliedLiabilityMax = 0.0D;

                            rpal = pandls.ProfitAndLosses.Where(pl => pl.SelectionId == runner.SelectionId).ToArray();

                            if (ImpliedLiabilityPercent == 0.0D)
                                impliedLiabilityMax = ImpliedLiability;
                            else
                            {
                                //  implied liability for the current selection = implied liability setting + (p/l * %implied setting)
                                impliedLiabilityMax = Globals.GetImpliedLiability(ImpliedLiability, ImpliedLiabilityPercent, rpal[0].IfWin);
                            }
                            if (impliedLiability > impliedLiabilityMax)
                            {
                                logger.Info($"PlaceBet: bet placement aborted for {GetRunnerName(runner.SelectionId)}, Lay implied liability too high {impliedLiability.ToString(Globals.DOUBLE_FORMAT)} > {impliedLiabilityMax.ToString(Globals.DOUBLE_FORMAT)}");
                                continue;
                            }

                            /*
                            if (pandls.ContainsKey (rp.selectionId))
                            {
                              double layLiability = (layAmountTotal - betAmount) -
                                Globals.GetLayLiability (rp.bestPricesToLay[nbpIndex].price, betAmount);
                              double liabilityIfPlaced = pandls[rp.selectionId].ifWin + layLiability;
                              _logger.Info (string.Format ("PlaceBet: layLiability {0} liabilityIfPlaced {1} current {2}",
                                layLiability, liabilityIfPlaced, pandls[rp.selectionId].ifWin));
                              if ((Math.Sign (liabilityIfPlaced) == -1) && (Math.Abs (liabilityIfPlaced) > Liability))
                              {
                                /* 2. Is current liability before placing these bets already > liability setting?
                                   If yes, then place no bets
                                   If no, then reduce bet amounts so liability stays <= liability settings.
                                *
                                if ((Math.Sign (pandls[rp.selectionId].ifWin) == -1) &&
                                  (Math.Abs (pandls[rp.selectionId].ifWin) > Liability))
                                {
                                  if (LayBetDealMode == BetDealMode.NoBets)
                                  {
                                    _logger.Info (string.Format ("PlaceBet: bet placement aborted, Lay liability too high {0} {1} > {2}",
                                      GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (Globals.DoubleFormat), Liability.ToString (Globals.DoubleFormat)));
                                    return;
                                  }
                                  else
                                  {
                                    _logger.Info (string.Format ("PlaceBet: bet placement skipped for this selection, Lay liability too high {0} {1} > {2}",
                                      GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (Globals.DoubleFormat), Liability.ToString (Globals.DoubleFormat)));
                                    continue;
                                  }
                                }
                              }
                            }
                            else
                            {
                              _logger.Info ("PlaceBet: cannot find current liability " +
                                GetRunnerName (rp.selectionId));
                            }
                            */

                            double size = Math.Round(betAmount, 2);
                            logger.Info(string.Format("PlaceBet: place Lay {0} {3}@{1} avail. {2}",
                              GetRunnerName(runner.SelectionId), runner.LayPrice.ToString(Globals.DOUBLE_FORMAT),
                              runner.LayAmount.ToString(Globals.DOUBLE_FORMAT), size.ToString(Globals.DOUBLE_FORMAT)));
                            placeBet = new PlaceInstruction();

#if SIMULATION
						if (size > 0.0d)
						{
							lvi = new ListViewItem (GetRunnerName (rp.selectionId));
							lvi.SubItems.Add (type.ToString ());
							lvi.BackColor = Globals.LAY_COLOUR;
							lvi.SubItems.Add ("Matched");
							lvi.SubItems.Add (rp.bestPricesToLay[nbpIndex].price.ToString (Globals.DoubleFormat));
							lvi.SubItems.Add (size.ToString ("###0.##"));
							lvi.SubItems.Add (now.ToLongTimeString ());
							var targetListView = externalListViewBets ?? listViewBets;
							targetListView.Items.Add (lvi);
						}
#else
                            var limitOrder = new LimitOrder
                            {
                                PersistenceType = BetPersistence,
                            };

#if UNMATCHABLE_BET
							size = 4.0d;
              limitOrder.Price = 1.01d;
#else
                            limitOrder.Price = runner.LayPrice;
#endif
                            limitOrder.Size = size;
                            placeBet.LimitOrder = limitOrder;
                            placeBet.OrderType = OrderTypeEnum.LIMIT;
                            //if (AppSettings.IsNextBestPrice)
                            //  // invert bet type
                            //  placeBet.betType = BetfairE.BetTypeEnum.B;
                            //else
                            placeBet.Side = SideEnum.LAY;
                            placeBet.SelectionId = runner.SelectionId;
                            bets.Add(placeBet);
#endif
                        }
                        break;

                        #endregion Lay
                }

                //? refactoring big class CTRL + SHIFT + M
#if !SIMULATION

                //bets.Clear ();
                if (bets.Count > 0)
                {
                    // ReplaceUnmatchedBetsIfNeeded removed - will implement separate cancel routine
                    bool allowPlacement = true;

                    if (allowPlacement)
                    {
                        PlaceExecutionReport placeResults = null;

#if !NOBET
                        // 1. Place bets and wait for response
                        placeResults = Globals.Exchange.PlaceOrders(exchangeId, marketId, bets);
                        lastBetPlacementTime = DateTime.Now;

                        // Reset catch-up mode after successful bet placement
                        if (placeResults != null && placeResults.Status == ExecutionReportStatusEnum.SUCCESS)
                        {
                            if (isCatchupMode)
                            {
                                logger.Info("PlaceBet: Bets placed successfully, exiting CATCH-UP MODE");
                                isCatchupMode = false;
                            }
                        }
#endif

                        // Display placed orders
                        if (placeResults != null && placeResults.InstructionReports != null)
                        {
                            foreach (var instructionReport in placeResults.InstructionReports)
                            {
                                var bet = bets.FirstOrDefault(b => b.SelectionId == instructionReport.Instruction.SelectionId);
                                if (bet != null)
                                {
                                    string runnerName = GetRunnerName(bet.SelectionId);
                                    string betType = bet.Side == SideEnum.BACK ? "Back" : "Lay";
                                    double price = bet.LimitOrder?.Price ?? 0;
                                    double size = bet.LimitOrder?.Size ?? 0;

                                    if (instructionReport.Status == InstructionReportStatusEnum.SUCCESS)
                                    {
                                        // Record when bet was placed
                                        if (!string.IsNullOrEmpty(instructionReport.BetId))
                                        {
                                            betPlacedTime[instructionReport.BetId] = DateTime.Now;
                                        }

                                        // Track total payout exposure for this selection
                                        double payout = size * price;
                                        if (!totalPayoutExposure.ContainsKey(bet.SelectionId))
                                            totalPayoutExposure[bet.SelectionId] = 0;
                                        totalPayoutExposure[bet.SelectionId] += payout;

                                        // Track intended payout (what we meant to place)
                                        if (!intendedPayoutTotal.ContainsKey(bet.SelectionId))
                                            intendedPayoutTotal[bet.SelectionId] = 0;
                                        intendedPayoutTotal[bet.SelectionId] += payout;

                                        // Increment bet count for this runner with payout
                                        IncrementRunnerBetCount(bet.SelectionId, payout);

                                        // Check if fully matched or partially matched
                                        double sizeMatched = instructionReport.SizeMatched ?? 0;
                                        double sizeRemaining = size - sizeMatched;

                                        if (sizeMatched > 0)
                                        {
                                            AddOrderToList(runnerName, betType, "Matched", price, sizeMatched, instructionReport.BetId);
                                        }
                                        if (sizeRemaining > 0.01)
                                        {
                                            AddOrderToList(runnerName, betType, "Unmatched", price, sizeRemaining, instructionReport.BetId);
                                        }
                                    }
                                    else
                                    {
                                        AddOrderToList(runnerName, betType, "Failed: " + instructionReport.ErrorCode, price, size);
                                    }
                                }
                            }
                        }

                        // reset small amount if needed (2, otherwise made here above in place 1)
                        if ((IsSmallLay) && (!AppSettings.IpcfReset))
                            if (Globals.IsBetsPlaced(placeResults))
                                foreach (var pb in bets)
                                {
                                    if (smallLayAmounts.ContainsKey(pb.SelectionId))
                                        smallLayAmounts.Remove(pb.SelectionId);
                                }

                        if ((isInPlay) && (AppSettings.IsCancelBets))
                            HandleBetResult(placeResults);

                        // 2. Update display after placing bets (gets fresh P&L and matchedByRunner)
                        DisplayBets();

                        // 3. Wait briefly to allow bets to match (2 seconds)
                        System.Threading.Thread.Sleep(2000);

                        // 4. Get fresh P&L and current prices
                        DisplayProfitAndLoss();

                        // 5. Cancel unmatched bets that have drifted in price
                        CancelDriftedUnmatchedBets();

                        // 6. Update display after cancelling (updates matchedByRunner for next cycle's catch-up logic)
                        DisplayBets();
                    }
                    else
                    {
                        logger.Info("PlaceBet: Bet placement blocked");
                    }
                }
                else
                {
                    // No new bets to place, but still check for unmatched bets to cancel
                    CancelDriftedUnmatchedBets();

                    // Update display after cancelling
                    DisplayBets();
                }

                Globals.CheckNetworkStatusIsConnected();
#endif

#if SIMULATION
			// add separator
			lvi = new ListViewItem (string.Empty);
			lvi.BackColor = System.Drawing.Color.LightGray;
			lvi.UseItemStyleForSubItems = true;
			var targetListView = externalListViewBets ?? listViewBets;
			targetListView.Items.Add (lvi);
#endif
            }
            catch (Exception ex)
            {
                //_logger.Error (ex.ToString ());
                Globals.UnhandledException(ex);
            }
            finally
            {
                // Reset flag to allow next PlaceBet cycle
                isPlacingBets = false;
            }
            //      finally
            //      {
            //#if SIMULATION
            //        listViewBets.EndUpdate ();
            //#else
            //        //DisplayBets ();
            //#endif
            //      }
        }

        private bool SetLiability(BetType betType, out bool isLiabilityExceeded, out bool isAllGreen)
        {
            isLiabilityExceeded = false;
            isAllGreen = false;
            //int record = 0;
            if ((pandls != null) && (pandls.ProfitAndLosses.Count > 0))
            {
                if (!Globals.GetMaxMinLiability(pandls.ProfitAndLosses, out var pandlMin, out _, out _))
                {
                    logger.Error("SetLiability: GetMaxMinLiability failed");
                    return false;
                }

                isAllGreen = pandlMin.IfWin > 0.0D;
                isLiabilityExceeded = Globals.IsLiabilityExceeded(pandlMin.IfWin, usedLiability, isAllGreen, betType);

                // check if liability has yet been exceeded
                // if yes, start to reduce liability
                if (isLiabilityExceeded && !hasLiabilityBeenExceeded)
                    hasLiabilityBeenExceeded = true;

                // dealing with negative values => test inverted
                if (pandlMin.IfWin < dynamicLiability)
                {
                    // Is following line asking if any selection has any liability ?
                    // If so, could this be an issue if it goes inplay with all selections at zero ?
                    // Resulting in it not setting usedLiability until a bet placed ?
                    // I remember we had to add a check because if all selection where zero it got stuck in a loop.
                    //if (isAnyLiability)
                    //{

                    // Only update dynamic liability if using dynamic liability mode
                    if (IsDynamicLiability)
                        SetNewLiability(pandlMin.IfWin);

                    //}
                    //else
                    //{
                    //  _logger.Info ("PlaceBet: dynamic liability not set, no liability yet");
                    //}
                }
                else
                {
                    if (hasLiabilityBeenExceeded && IsDynamicLiability)
                        SetNewLiability(pandlMin.IfWin);
                }

                if (IsDynamicLiability)
                    usedLiability = dynamicLiability;
                else
                {
                    // Static liability mode
                    if (isInPlay)
                    {
                        // In-play static liability: track best all-green position and use it to tighten liability
                        if (isAllGreen && pandlMin.IfWin > bestAllGreenLowest)
                        {
                            bestAllGreenLowest = pandlMin.IfWin;
                            logger.Info($"SetLiability: new best all-green lowest = {bestAllGreenLowest.ToString(Globals.DOUBLE_FORMAT)}");
                        }

                        // Adjust liability based on best all-green position achieved
                        usedLiability = Liability + bestAllGreenLowest;
                        logger.Info($"SetLiability: in-play static liability = {Liability.ToString(Globals.DOUBLE_FORMAT)} + {bestAllGreenLowest.ToString(Globals.DOUBLE_FORMAT)} = {usedLiability.ToString(Globals.DOUBLE_FORMAT)}");
                    }
                    else
                    {
                        // Pre-play static liability: remains constant as long as at least 1 selection is red.
                        // As soon as all selections are green, switch to dynamic liability routine.
                        usedLiability = Math.Sign(pandlMin.IfWin) > 0 ? dynamicLiability : Liability;
                    }
                }

                logger.Info($"SetLiability: usedLiability = {usedLiability.ToString(Globals.DOUBLE_FORMAT)}, lowest p&l = {pandlMin.IfWin.ToString(Globals.DOUBLE_FORMAT) ?? "NA"}");

                // Update liability label
                UpdateLiabilityLabel();
            }

            return true;
        }

        private void UpdateLiabilityLabel()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(UpdateLiabilityLabel));
                return;
            }

            toolStripLabelLiability.Text = $"Liability: £{usedLiability:F2}";
            toolStripLabelLiability.ForeColor = usedLiability < 0 ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }

        /*
         * runner liability  liability test  all runners positive  lay  back
           ----------------  --------------  --------------------  ---  ----
                  +                NR                  no          yes  yes    - correct
        +20                           1 or more selections -tive
                  +               yes                 yes           no  yes    - correct
        +20      usedliability>=+20    All selections +tive
                  +                no                 yes          yes  yes    - correct
        +20      usedliability<+20    All selections +tive
                  -               yes                  NR          yes  yes    - correct
        -20      usedliability<-20
             E.g usedliability = -50
                  -                no                  NR           no  yes    - correct
        -20      usedliability>=-20
                      E.g usedliability = -10

        private bool IsSkipBet (BetType betType, double selectionPAndL, double usedLiability, bool isAllPositive)
        {
          switch (betType)
          {
            case BetType.Back:
              // true in all cases => do nothing here
              break;

            case BetType.Lay:
              if (selectionPAndL >= 0.0D)
              {
                if (isAllPositive)
                {
                  if (selectionPAndL < usedLiability)
                    return true;
                }
              }
              else
              {
                return (selectionPAndL < usedLiability);
              }
              break;
          }

          return false;
        }
        */

        /*
         *
            Get selection liability for all selections.
            Are all selections green +tive - if yes goto all green settings

    If lay bet
            Set liabflag  = 0
            Is any selection liability  < liability setting (i.e. -25 < -20 = yes, -15 < -20 = no, -20 < -20 = no)
            If true set liabflag (n) = 1

            If liabflag = 0 then place lay bet for current selection.
            If liabflag = 1 then :
            Is current selection liability  < liability setting (i.e. -25 < -20 = yes, -15 < -20 = no, -20 < -20 = no)
            If true then skip lay bet for current selection
            If false then place lay bet for current selection.

            Goto next selection

    If back bet
            Set liabflag  = 0
            Is any selection liability <= liability setting (i.e. -25 < -20 = yes, -15 < -20 = no, -20 < -20 = yes)
            If true set liabflag = 1

            If liabflag = 0 then place back bet for current selection.
            If liabflag = 1 then :
            Is current selection liability <= liability setting (i.e. -25 < -20 = yes, -15 < -20 = no, -20 < -20 = yes)
            If true then place back bet for current selection
            If false then skip bet for current selection.

            Goto next selection

            all green settings would be the inverse of the above.

            Is any selection liability (profit) <= liability setting (set to dynamic for both static & dynamic

     -Not sure to completely understand here.
    If dynamic liability is used when all selections are green (it should always be the case since the modifcation to the static liability we made, this test is useless and can be removed as it will always be true, isn't it ?
            If true set liabflag = 1 (I'm presuming this will always be set to 1 because it will always be equal to lowest green (is this correct?))
    Yes - I was just wanting you to confirm that I was correct - which you have!

            Then check each selection
            If liabflag = 0 then bets can be placed for all selections as normal.
            If liabflag = 1 then the app. knows there is at least 1 selection that needs the selection profit increased.
            If liabflag = 1 then check selection liability.
            Is selection liability <= liability setting.
    (i.e. 10 <= 20 = yes, 20 <= 20 = yes, 30 <= 20 = no)
            If yes and back bet then place back bet on this selection
            If yes and lay bet then skip lay bet for this selection
            If no and back bet then skip back bet on this selection
            If no and lay bet then place lay bet on this selection
            Goto next selection
         */

        private static bool IsSkipBet(BetType betType, double selectionPAndL, double liability, bool isAllGreen)
        {
            bool result = false;

            switch (betType)
            {
                case BetType.Back:
                    result = (!Globals.IsLiabilityExceeded(selectionPAndL, liability, isAllGreen, betType));
                    break;

                case BetType.Lay:
                    result = Globals.IsLiabilityExceeded(selectionPAndL, liability, isAllGreen, betType);
                    break;
            }

            return result;
        }

        /*
            private void DisplayBets (object state)
            {
              DisplayBets ();
            }
        */

        public void DisplayBets()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(DisplayBets));
                    return;
                }

                //totalBets = 0;
                //listViewBets.BeginUpdate ();
                //listViewBets.Items.Clear ();
                //totalBets = DisplayBets (BetfairE.BetStatusEnum.M);
                /*
                      List<BetfairE.CancelBets> betsToCancel = new List<BetfairE.CancelBets> ();
                      DisplayBetsInternal (ref betsToCancel);
                      if (betsToCancel.Count > 0)
                      {
                        CancelBetsParam param = new CancelBetsParam (exchangeId, betsToCancel);
                        ThreadPool.QueueUserWorkItem (new WaitCallback (CancelBets), param);
                      }
                */
                //DateTime before = DateTime.Now;
                DisplayProfitAndLoss();
            }
            catch (Exception ex)
            {
                Globals.UnhandledException(ex);
            }
            //TimeSpan after = DateTime.Now - before;
            //Console.WriteLine ("{0} DisplayProfitAndLoss {1} ms",
            //  market.name, after.TotalMilliseconds);

            //if (OnDisplayWallet != null)
            //  OnDisplayWallet ();
            //listViewBets.EndUpdate ();
        }

        /*
            private double HandleSmallLayBet (int selectionId, double amount, double price)
            {
              double betAmount;
              double cf;

              if (_smallLayAmounts.ContainsKey (selectionId))
                cf = _smallLayAmounts[selectionId];
              else
                cf = 0;

              betAmount = (cf + amount) / price;
              _logger.Info (string.Format ("HandleSmallLayBet: cf {0} amount {1} price {2} betAmount {3} selection {4}",
                cf.ToString (Globals.DoubleFormat), amount.ToString (Globals.DoubleFormat), price.ToString (Globals.DoubleFormat),
                betAmount.ToString (Globals.DoubleFormat), GetRunnerName (selectionId)));
              if (betAmount < Globals.MinBetAmount)
              {
                // no bet
                if (!_smallLayAmounts.ContainsKey (selectionId))
                  _smallLayAmounts.Add (selectionId, amount);
                else
                  _smallLayAmounts[selectionId] += amount;
                betAmount = 0.0D;
              }
              else
                if (_smallLayAmounts.ContainsKey (selectionId))
                  _smallLayAmounts.Remove (selectionId);

              return betAmount;
            }
        */

        /// <summary>
        /// Calculates the minimum uniform payout across all runners that satisfies Betfair's minimum stake requirement.
        /// Checks all prices to find which one produces the largest payout when stake is rounded up to meet the 10 minimum.
        /// </summary>
        /// <param name="runners">List of runners with their prices</param>
        /// <param name="layBetMode">Lay bet mode to determine price calculation</param>
        /// <returns>The minimum uniform payout amount to use for all selections</returns>
        private double CalculateMinimumUniformPayout(List<Runner> runners, LayBetMode layBetMode)
        {
            double maxPayout = 10.0d;
            double priceForMaxPayout = 0.0d;

            // Check each price to find which one produces the largest payout after rounding
            foreach (var runner in runners)
            {
                if (!double.IsNaN(runner.LayPrice) && runner.LayPrice != Globals.MaxPrice && runner.LayPrice < 100.0d)
                {
                    double price = (layBetMode == LayBetMode.Payout)
                        ? runner.LayPrice
                        : runner.LayPrice - 1.0D;

                    // Calculate stake needed for this price to achieve minimum payout of 10
                    double stake = 10.0d / price;

                    // Round up to 2 decimal places (ceiling)
                    double roundedStake = Math.Ceiling(stake * 100.0d) / 100.0d;

                    // Calculate actual payout with rounded stake
                    double payout = roundedStake * price;

                    // Track the maximum payout
                    if (payout > maxPayout)
                    {
                        maxPayout = payout;
                        priceForMaxPayout = price;
                    }
                }
            }

            logger.Info($"CalculateMinimumUniformPayout: priceForMaxPayout {priceForMaxPayout.ToString(Globals.DOUBLE_FORMAT)} uniformPayout {maxPayout.ToString(Globals.DOUBLE_FORMAT)}");

            return maxPayout;
        }

        private double HandleSmallBackBet(long selectionId, double amount)
        {
            double totalAmount = amount;
            double betAmount;

            if (smallBackAmounts.ContainsKey(selectionId))
            {
                totalAmount = amount - smallBackAmounts[selectionId];
                if (totalAmount >= Globals.MinBetAmount)
                {
                    smallBackAmounts.Remove(selectionId);
                    betAmount = totalAmount;

                    logger.Info($"HandleSmallBackBet: used up amount {amount.ToString(Globals.DOUBLE_FORMAT)} betAmount {betAmount.ToString(Globals.DOUBLE_FORMAT)} selection {GetRunnerName(selectionId)}");
                }
                else
                {
                    if (totalAmount >= 0)
                    {
                        smallBackAmounts[selectionId] = Globals.MinBetAmount - totalAmount;
                        betAmount = Globals.MinBetAmount;
                    }
                    else
                    {
                        smallBackAmounts[selectionId] -= amount;
                        betAmount = 0.0d;
                    }

                    logger.Info($"HandleSmallBackBet: _smallBackAmounts {smallBackAmounts[selectionId].ToString(Globals.DOUBLE_FORMAT)} amount {amount.ToString(Globals.DOUBLE_FORMAT)} betAmount {betAmount.ToString(Globals.DOUBLE_FORMAT)} selection {GetRunnerName(selectionId)}");
                }
            }
            else
            {
                if (totalAmount < Globals.MinBetAmount)
                {
                    double overBet = Globals.MinBetAmount - totalAmount;

                    smallBackAmounts.Add(selectionId, overBet);
                    // now, compute overbet and then, return min amount
                    //betAmount = Globals.MinBetAmount;
                    betAmount = Globals.MinBetAmount;

                    logger.Info(string.Format("HandleSmallBackBet: overbet {3} amount {0} betAmount {1} selection {2}",
                      amount.ToString(Globals.DOUBLE_FORMAT), betAmount.ToString(Globals.DOUBLE_FORMAT),
                      GetRunnerName(selectionId), overBet.ToString(Globals.DOUBLE_FORMAT)));
                }
                else
                {
                    betAmount = totalAmount;
                }
            }

            return betAmount;
        }

        /*
            int DisplayBetsInternal (ref List<BetfairE.CancelBets> betsToCancel)
            {
              int totalRecords = 0;

              try
              {
                int record = 0;
                //double price=0.0d;
                //double size=0.0d;
                //DateTime placedDate=DateTime.Now;
                DateTime lastCall = DateTime.MinValue;

                //lock (_lockDisplayBets)
                //{
                //listViewBets.BeginUpdate ();
                //listViewBets.Items.Clear ();
                do
                {
                  IList<BetfairE.MUBet> bets =
                  Globals.Exchange.GetMUBets (exchangeId, marketId, BetfairE.BetStatusEnum.MU, lastCall, BetfairE.BetsOrderByEnum.PLACED_DATE, ref record, 100);
                  //lastCall = DateTime.Now;
                  //IList<BetfairE.Bet> bets = Globals.exchange.GetCurrentBets (exchangeId, marketId, status, BetfairE.BetsOrderByEnum.PLACED_DATE, ref record, 100);
                  totalRecords += record;
                  foreach (BetfairE.MUBet bet in bets)
                  {
                    switch (bet.betStatus)
                    {
                      case BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.M:
                      case BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.S:
                        //price = bet.avgPrice;
                        //size = bet.matchedSize;
                        //price = bet.price;
                        //size = bet.size;
                        //placedDate = bet.matchedDate;
                        break;

                      case BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.U:
                        //price = bet.price;
                        //size = bet.size;
                        //placedDate = bet.placedDate;
                        if ((_isInPlay) && (AppSettings.IsCancelBets))
                        {
                          BetfairE.CancelBets cancelBet = new BetfairE.CancelBets ();
                          cancelBet.betId = bet.betId;
                          //_logger.Info (string.Format ("will cancel bet {0} {1}", bet.selectionName, bet.betId));
                          _logger.Info ("will cancel bet " + bet.betId);
                          betsToCancel.Add (cancelBet);
                        }
                        break;

                                    //case BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.S:
                                    //  price = bet.avgPrice;
                                    //  size = bet.matchedSize;
                                    //  placedDate = bet.settledDate;
                                    //  break;
                    }

                    //if (bet.betStatus != BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.U)
                    //AddBetsToList (GetRunnerName (bet.selectionId), bet.betType,
                    //  bet.betStatus, price, size, placedDate);

                    // update profit & loss

                                //if (_views.ContainsKey (bet.selectionId))
                                //{
                                //  _views[bet.selectionId].ProfitLiability = bet.profitAndLoss;
                                //}
                  }
                }
                while (record > 0);
                //}
                Globals.CheckNetworkStatusIsConnected ();
              }
              //catch (Exception ex)
              //{
              //  _logger.Error (ex.ToString ());
              //}
              finally
              {
                listViewBets.EndUpdate ();
              }

              return totalRecords;
            }
        */

        private void DisplayProfitAndLoss()
        {
            try
            {
                var isProfitReached = true;
                //DateTime before = DateTime.Now;
                pandls = Globals.Exchange.GetMarketProfitAndLoss(exchangeId, marketId);
                if (isInPlay)
                {
                    // call SetLiability no more than once a second.
                    var now = DateTime.Now;
                    var lastCallDiff = now - _lastSetLiability;
                    _lastSetLiability = now;
                    if (lastCallDiff.Milliseconds > 1000)
                        SetLiability(BetType.Lay, out bool _, out bool _);
                }
                //TimeSpan after = DateTime.Now - before;
                //Console.WriteLine ("{1} GetMarketProfitAndLoss {0} ms",
                //  market.name, after.TotalMilliseconds);

                // Get current orders to count matched/unmatched per runner (use ALL projection to get all order details)
                var currentOrders = Globals.Exchange.ListCurrentOrders(exchangeId, null, marketId, OrderProjectionEnum.ALL, null, null, null);
                // Don't clear matchedByRunner - rebuild it from current orders so catch-up always has accurate data
                var newMatchedByRunner = new Dictionary<long, double>();
                var unmatchedByRunner = new Dictionary<long, double>();
                var matchedPayoutByRunner = new Dictionary<long, double>();
                var unmatchedPayoutByRunner = new Dictionary<long, double>();

                if (currentOrders?.CurrentOrders != null)
                {
                    foreach (var order in currentOrders.CurrentOrders)
                    {
                        if (order.SizeMatched > 0)
                        {
                            // Track matched payout (size * price = total payout for lay bets)
                            if (!matchedPayoutByRunner.ContainsKey(order.SelectionId))
                                matchedPayoutByRunner[order.SelectionId] = 0;
                            matchedPayoutByRunner[order.SelectionId] += order.SizeMatched * order.AveragePriceMatched;
                        }
                        if (order.SizeRemaining > 0)
                        {
                            // Track unmatched payout (stake risked for lay bets)
                            if (!unmatchedPayoutByRunner.ContainsKey(order.SelectionId))
                                unmatchedPayoutByRunner[order.SelectionId] = 0;
                            unmatchedPayoutByRunner[order.SelectionId] += order.SizeRemaining;

                            if (!unmatchedByRunner.ContainsKey(order.SelectionId))
                                unmatchedByRunner[order.SelectionId] = 0;
                            unmatchedByRunner[order.SelectionId] += order.SizeRemaining;
                        }
                    }

                    // Use actual matched payout from orders for catch-up logic
                    foreach (var kvp in matchedPayoutByRunner)
                    {
                        newMatchedByRunner[kvp.Key] = kvp.Value; // Store actual matched payout amount
                    }
                }

                // Update matchedByRunner atomically - only increase, never decrease (settled orders disappear from API)
                lock (matchedByRunnerLock)
                {
                    foreach (var kvp in newMatchedByRunner)
                    {
                        // Only update if new value is higher (accumulate matched payout, never lose it)
                        if (!matchedByRunner.ContainsKey(kvp.Key) || kvp.Value > matchedByRunner[kvp.Key])
                        {
                            matchedByRunner[kvp.Key] = kvp.Value;
                        }
                    }
                }

                foreach (var pandl in pandls.ProfitAndLosses)
                {
                    //_logger.Info (string.Format ("P&L: {0} {1}",
                    //  pandl.Value.selectionName, pandl.Value.ifWin));
                    if (pandl.IfWin < Profit)
                        isProfitReached = false;
                    if (views.ContainsKey(pandl.SelectionId))
                    {
                        //_logger.Info ("DisplayProfitAndLoss: lock enter " + GetRunnerName (pandl.Key));
                        lock (forLockingPAndL)
                        {
                            views[pandl.SelectionId].ProfitLiability = pandl.IfWin;

                            // Update matched/unmatched counts and payouts
                            double matchedPayout = matchedPayoutByRunner.ContainsKey(pandl.SelectionId) ? matchedPayoutByRunner[pandl.SelectionId] : 0;
                            int matchedCount = matchedPayout < 10.0 ? 0 : (int)Math.Round(matchedPayout / 10.0); // Display count based on payout
                            double unmatchedPayout = unmatchedPayoutByRunner.ContainsKey(pandl.SelectionId) ? unmatchedPayoutByRunner[pandl.SelectionId] : 0;
                            int unmatchedCount = unmatchedPayout < 10.0 ? 0 : (int)Math.Round(unmatchedPayout / 10.0); // Display count based on payout
                            views[pandl.SelectionId].SetMatchedUnmatched(matchedCount, unmatchedCount, matchedPayout, unmatchedPayout);
                        }
                        //_logger.Info ("DisplayProfitAndLoss: lock release" + GetRunnerName (pandl.Key));
                    }
                }
                if (isProfitReached)
                {
                    if (isInPlay)
                    {
                        if (!isIpProfitReached)
                        {
                            logger.Info("DisplayProfitAndLoss: IP profit is reached");
                            isIpProfitReached = true;
                        }
                    }
                    else
                    {
                        if (!isPrProfitReached)
                        {
                            logger.Info("DisplayProfitAndLoss: PR profit is reached");
                            isPrProfitReached = true;
                        }
                    }
                }
                Globals.CheckNetworkStatusIsConnected();
            }
            catch (Exception ex)
            {
                //  _logger.Error (ex.ToString ());
                Globals.UnhandledException(ex);
            }
        }

        /*
            private void DisplayBetsDelayed (object state)
            {
              DisplayBets ();
            }
            */

        private class CancelBetsParam
        {
            public readonly int ExchangeId;
            public readonly List<CancelInstruction> BetsToCancel;

            public CancelBetsParam(int exchangeId, List<CancelInstruction> betsToCancel)
            {
                ExchangeId = exchangeId;
                BetsToCancel = betsToCancel;
            }
        }

        private void CancelBets(object state)
        {
            //try
            //{
            var param = state as CancelBetsParam;
            if (param == null)
            {
                logger.Error("CancelBets: invalid parameter");
                return;
            }

            Globals.Exchange.CancelOrders(param.ExchangeId, marketId, param.BetsToCancel);
            Globals.CheckNetworkStatusIsConnected();
            /*
                    foreach (BetfairE.CancelBetsResult cbr in cancelResults)
                    {
                      _logger.Info (string.Format ("CancelBet: result {0} {1} {2} {3} {4}",
                        cbr.betId, cbr.resultCode, cbr.success, cbr.sizeCancelled, cbr.sizeMatched));
                    }
            */
            //DisplayBets ();
            //}
            //catch (Exception ex)
            //{
            //  _logger.Error (ex.ToString ());
            //}
        }

        /*
            void AddBetsToList (string selectionName, BetfairE.BetTypeEnum betType, BetfairE.BetStatusEnum betStatus,
              double price, double size, DateTime placedDate)
            {
              ListViewItem lvi = new ListViewItem (selectionName);
              switch (betType)
              {
                case BetfairClient.Framework.com.betfair.api6.exchange.BetTypeEnum.B:
                  lvi.SubItems.Add ("Back");
                  lvi.BackColor = Globals.BACK_COLOUR;
                  break;

                case BetfairClient.Framework.com.betfair.api6.exchange.BetTypeEnum.L:
                  lvi.SubItems.Add ("Lay");
                  lvi.BackColor = Globals.LAY_COLOUR;
                  break;
              }
              switch (betStatus)
              {
                case BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.M:
                  lvi.SubItems.Add ("Matched");
                  break;

                case BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.U:
                  lvi.SubItems.Add ("Unmatched");
                  break;

                case BetfairClient.Framework.com.betfair.api6.exchange.BetStatusEnum.S:
                  lvi.SubItems.Add ("Settled");
                  break;
              }

              lvi.SubItems.Add (price.ToString (Globals.DoubleFormat));
              lvi.SubItems.Add (size.ToString (Globals.DoubleFormat));
              lvi.SubItems.Add (placedDate.ToLongTimeString ());
              var targetListView = externalListViewBets ?? listViewBets;
              targetListView.Items.Add (lvi);
            }
            */

        #endregion Bets Handling

        #region Settings

        private double BackAmount => (isInPlay) ? AppSettings.IpBackAmount : AppSettings.PrBackAmount;

        private double LayAmount => (isInPlay) ? AppSettings.IpLayAmount : AppSettings.PrLayAmount;

        private double Liability => (isInPlay) ? AppSettings.IpLiability : AppSettings.PrLiability;

        public double ImpliedLiability => (isInPlay) ? AppSettings.IpImpliedLiability : AppSettings.PrImpliedLiability;

        public double ImpliedLiabilityPercent => (isInPlay) ? AppSettings.IpImpliedLiabilityPercent : AppSettings.PrImpliedLiabilityPercent;

        private LayBetMode LayBetMode => (isInPlay) ? AppSettings.IpLayBetMode : AppSettings.PrLayBetMode;

        private double Profit => (isInPlay) ? AppSettings.IpProfit : AppSettings.PrProfit;

        private bool IsSmallLay => (isInPlay) ? AppSettings.IpIsSmallLay : AppSettings.PrIsSmallLay;

        public bool IsIgnoreHighLay => (isInPlay) ? AppSettings.IpIsIgnoreHighLay : AppSettings.PrIsIgnoreHighLay;

        public bool IsLayBookPercentage => AppSettings.IpIsLayBookPercentage;

        public bool IsDynamicLiability => false; // Always use static liability mode

        private int RefreshInterval => (isInPlay) ? AppSettings.IpRefreshInterval : AppSettings.PrRefreshInterval;

        private double HighLay => (isInPlay) ? AppSettings.IpHighLay : AppSettings.PrHighLay;

        private double HighLayIncrease => (isInPlay) ? AppSettings.IpHighLayIncrease : AppSettings.PrHighLayIncrease;

        private double HighLayMultiplier => (isInPlay) ? AppSettings.IpHighLayMultiplier : AppSettings.PrHighLayMultiplier;

        private BetDealMode BackBetDealMode => (isInPlay) ? AppSettings.IpBackBetDealMode : AppSettings.PrBackBetDealMode;

        private BetDealMode LayBetDealMode => (isInPlay) ? AppSettings.IpLayBetDealMode : AppSettings.PrLayBetDealMode;

        private HighLayMode HighLayMode => (isInPlay) ? AppSettings.IpHighLayMode : AppSettings.PrHighLayMode;

        private bool IsLayOver => (isInPlay) ? AppSettings.IsLayOverIp : AppSettings.IsLayOverPr;

        public double LayOver => (isInPlay) ? AppSettings.LayOverIp : AppSettings.LayOverPr;

        private bool IsBackOver => (isInPlay) ? AppSettings.IsBackOverIp : AppSettings.IsBackOverPr;

        public double BackOver => (isInPlay) ? AppSettings.BackOverIp : AppSettings.BackOverPr;

        public double UnderMargin
        {
            get
            {
                if (isInPlay)
                {
                    return AppSettings.IpUnderOverMode == UnderOverMode.Fixed
                               ? AppSettings.IpUnderMarginFixed
                               : AppSettings.IpUnderMarginDynamic;
                }

                return AppSettings.PrUnderMargin;
            }
        }

        public double OverMargin
        {
            get
            {
                if (isInPlay)
                {
                    return AppSettings.IpUnderOverMode == UnderOverMode.Fixed
                               ? AppSettings.IpOverMarginFixed
                               : AppSettings.IpOverMarginDynamic;
                }

                return AppSettings.PrOverMargin;
            }
        }

        public double DynamicLiabilityVariant => (isInPlay) ? AppSettings.IpDynamicLiabilityVariant : AppSettings.PrDynamicLiabilityVariant;

        public PersistenceTypeEnum BetPersistence =>
            AppSettings.BetPersistence == BetPersistenceType.Lapse ? PersistenceTypeEnum.LAPSE
            : AppSettings.BetPersistence == BetPersistenceType.Persist ? PersistenceTypeEnum.PERSIST
            : PersistenceTypeEnum.MARKET_ON_CLOSE;

        private bool IsSwopPrice => (isInPlay) ? AppSettings.IpIsSwopPrice : AppSettings.PrIsSwopPrice;

        private bool IsNextBestPrice => (isInPlay) ? AppSettings.IpIsNextBestPrice : AppSettings.PrIsNextBestPrice;

        #endregion Settings

        private void toolStripButtonXMatching_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine(toolStripButtonXMatching.Checked);
            _isCrossMatchingEnabled = !_isCrossMatchingEnabled;
        }

        /// <summary>
        /// Gets unmatched bets for the current market that have unmatched portions
        /// </summary>
        private CurrentOrderSummaryReport GetUnmatchedBets()
        {
            try
            {
                logger.Info("GetUnmatchedBets: Calling ListCurrentOrders...");
                var unmatchedBets = Globals.Exchange.ListCurrentOrders(
                    exchangeId,
                    null,
                    marketId,
                    OrderProjectionEnum.EXECUTABLE,
                    null,
                    null,
                    OrderByEnum.BY_BET);

                logger.Info($"GetUnmatchedBets: Received {unmatchedBets?.CurrentOrders?.Count ?? 0} orders");

                // Filter to only include bets with unmatched portions
                if (unmatchedBets?.CurrentOrders != null)
                {
                    var filtered = unmatchedBets.CurrentOrders
                        .Where(o => o.SizeRemaining > 0)
                        .ToList();

                    logger.Info($"GetUnmatchedBets: {filtered.Count} orders have unmatched portions (SizeRemaining > 0)");
                    unmatchedBets.CurrentOrders = filtered;
                }

                return unmatchedBets;
            }
            catch (Exception ex)
            {
                logger.Error($"GetUnmatchedBets: Exception - {ex.Message}");
                logger.Error($"GetUnmatchedBets: Stack trace - {ex.StackTrace}");
                return null;
            }
        }

        private int totalCancelledBets = 0; // Track total cancelled bets for display

        public int TotalCancelledBets => totalCancelledBets; // Public accessor for display

        /// <summary>
        /// Cancel unmatched bets where the current price has drifted beyond threshold
        /// Uses formula: cancel_price = ((unmatched_price - 1) * 1.2) + 1 (~10 ticks, ~5% probability drift)
        /// </summary>
        private void CancelDriftedUnmatchedBets()
        {
            try
            {
                var unmatchedBets = GetUnmatchedBets();

                if (unmatchedBets == null || unmatchedBets.CurrentOrders == null || unmatchedBets.CurrentOrders.Count == 0)
                {
                    return; // No unmatched bets
                }

                // Get current market prices from latest update
                var priceProjection = new PriceProjection
                {
                    PriceData = new HashSet<PriceDataEnum> { PriceDataEnum.EX_BEST_OFFERS }
                };
                var marketBook = Globals.Exchange.ListMarketBook(exchangeId, marketId, priceProjection);

                if (marketBook == null || marketBook.Runners == null)
                {
                    logger.Warn("CancelDriftedUnmatchedBets: Could not get current market prices");
                    return;
                }

                // Build a lookup of current lay prices by selection ID
                var currentPrices = new Dictionary<long, double>();
                foreach (var runnerBook in marketBook.Runners)
                {
                    // Get best lay price (price we can lay at)
                    if (runnerBook.ExchangePrices?.AvailableToLay != null && runnerBook.ExchangePrices.AvailableToLay.Count > 0)
                    {
                        currentPrices[runnerBook.SelectionId] = runnerBook.ExchangePrices.AvailableToLay[0].Price;
                    }
                }

#if !NOBET
                // Build cancel instructions for drifted bets
                var cancelInstructions = new List<CancelInstruction>();
                foreach (var unmatchedBet in unmatchedBets.CurrentOrders)
                {
                    if (unmatchedBet.SizeRemaining > 0 && currentPrices.ContainsKey(unmatchedBet.SelectionId))
                    {
                        double unmatchedPrice = unmatchedBet.PriceSize.Price;
                        double currentPrice = currentPrices[unmatchedBet.SelectionId];

                        // Calculate cancel threshold: ((price - 1) * 1.2) + 1
                        double cancelThreshold = ((unmatchedPrice - 1.0) * PRICE_DRIFT_MULTIPLIER) + 1.0;

                        // Cancel if current price has drifted beyond threshold
                        if (currentPrice >= cancelThreshold)
                        {
                            cancelInstructions.Add(new CancelInstruction { BetId = unmatchedBet.BetId });
                            logger.Info($"CancelDriftedUnmatchedBets: Cancelling {GetRunnerName(unmatchedBet.SelectionId)} - unmatched@{unmatchedPrice:F2}, current@{currentPrice:F2}, threshold@{cancelThreshold:F2}");
                        }
                    }
                }

                if (cancelInstructions.Count > 0)
                {
                    logger.Info($"CancelDriftedUnmatchedBets: Cancelling {cancelInstructions.Count} drifted bet(s) out of {unmatchedBets.CurrentOrders.Count} total unmatched");

                    // Send cancel request
                    var cancelResults = Globals.Exchange.CancelOrders(exchangeId, marketId, cancelInstructions, null);

                    // Count successfully cancelled bets
                    if (cancelResults?.InstructionReports != null)
                    {
                        int cancelledCount = 0;
                        var cancelledBetLookup = unmatchedBets.CurrentOrders.ToDictionary(b => b.BetId);

                        foreach (var report in cancelResults.InstructionReports)
                        {
                            if (report.Status == InstructionReportStatusEnum.SUCCESS)
                            {
                                cancelledCount++;
                                logger.Info($"CancelDriftedUnmatchedBets: Successfully cancelled bet {report.Instruction.BetId}");

                                // Update the cancelled count and payout on the runner's display
                                if (cancelledBetLookup.ContainsKey(report.Instruction.BetId))
                                {
                                    var cancelledBet = cancelledBetLookup[report.Instruction.BetId];
                                    double cancelledPayout = cancelledBet.SizeRemaining * cancelledBet.PriceSize.Price;
                                    UpdateRunnerCancelledPayout(cancelledBet.SelectionId, cancelledPayout);
                                }
                            }
                            else
                            {
                                logger.Info($"CancelDriftedUnmatchedBets: Failed to cancel bet {report.Instruction.BetId} - {report.ErrorCode}");
                            }
                        }

                        totalCancelledBets += cancelledCount;
                        logger.Info($"CancelDriftedUnmatchedBets: Cancelled {cancelledCount} bet(s). Total cancelled today: {totalCancelledBets}");
                    }
                }
                else
                {
                    logger.Info($"CancelDriftedUnmatchedBets: No drifted bets to cancel ({unmatchedBets.CurrentOrders.Count} unmatched bets within threshold)");
                }
#else
                logger.Info($"CancelDriftedUnmatchedBets: NOBET mode");
#endif
            }
            catch (Exception ex)
            {
                logger.Error($"CancelDriftedUnmatchedBets: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel all unmatched bets immediately
        /// </summary>
        private void CancelAllUnmatchedBets()
        {
            try
            {
                var unmatchedBets = GetUnmatchedBets();

                if (unmatchedBets == null || unmatchedBets.CurrentOrders == null || unmatchedBets.CurrentOrders.Count == 0)
                {
                    return; // No unmatched bets
                }

                logger.Info($"CancelAllUnmatchedBets: Found {unmatchedBets.CurrentOrders.Count} unmatched bet(s) to cancel");

#if !NOBET
                // Build cancel instructions
                var cancelInstructions = new List<CancelInstruction>();
                foreach (var unmatchedBet in unmatchedBets.CurrentOrders)
                {
                    if (unmatchedBet.SizeRemaining > 0)
                    {
                        cancelInstructions.Add(new CancelInstruction { BetId = unmatchedBet.BetId });
                        logger.Info($"CancelAllUnmatchedBets: Cancelling {GetRunnerName(unmatchedBet.SelectionId)} bet {unmatchedBet.BetId} - £{unmatchedBet.SizeRemaining:F2}@{unmatchedBet.PriceSize.Price:F2}");
                    }
                }

                if (cancelInstructions.Count > 0)
                {
                    // Send cancel request
                    var cancelResults = Globals.Exchange.CancelOrders(exchangeId, marketId, cancelInstructions, null);

                    // Count successfully cancelled bets and update display per runner
                    if (cancelResults?.InstructionReports != null)
                    {
                        int cancelledCount = 0;
                        var cancelledBetLookup = unmatchedBets.CurrentOrders.ToDictionary(b => b.BetId);

                        foreach (var report in cancelResults.InstructionReports)
                        {
                            if (report.Status == InstructionReportStatusEnum.SUCCESS)
                            {
                                cancelledCount++;
                                logger.Info($"CancelAllUnmatchedBets: Successfully cancelled bet {report.Instruction.BetId}");

                                // Update the cancelled count and payout on the runner's display
                                if (cancelledBetLookup.ContainsKey(report.Instruction.BetId))
                                {
                                    var cancelledBet = cancelledBetLookup[report.Instruction.BetId];
                                    // Calculate cancelled payout: SizeRemaining * Price
                                    double cancelledPayout = cancelledBet.SizeRemaining * cancelledBet.PriceSize.Price;
                                    UpdateRunnerCancelledPayout(cancelledBet.SelectionId, cancelledPayout);
                                }
                            }
                            else
                            {
                                logger.Info($"CancelAllUnmatchedBets: Failed to cancel bet {report.Instruction.BetId} - {report.ErrorCode}");
                            }
                        }

                        totalCancelledBets += cancelledCount;
                        logger.Info($"CancelAllUnmatchedBets: Cancelled {cancelledCount} bet(s). Total cancelled today: {totalCancelledBets}");
                    }
                }
#else
                logger.Info($"CancelAllUnmatchedBets: NOBET mode - would cancel {unmatchedBets.CurrentOrders.Count} bet(s)");
#endif
            }
            catch (Exception ex)
            {
                logger.Error($"CancelAllUnmatchedBets: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the cancelled bet count display for a specific runner
        /// </summary>
        private void UpdateRunnerCancelledCount(long selectionId)
        {
            try
            {
                if (views.ContainsKey(selectionId))
                {
                    var marketView = views[selectionId];
                    marketView.IncrementCancelledCount();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"UpdateRunnerCancelledCount: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the cancelled payout for a specific runner
        /// </summary>
        private void UpdateRunnerCancelledPayout(long selectionId, double payout)
        {
            try
            {
                if (views.ContainsKey(selectionId))
                {
                    var marketView = views[selectionId];
                    marketView.IncrementCancelledPayout(payout);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"UpdateRunnerCancelledPayout: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel unmatched bets after 3 seconds (no consolidation).
        /// When P&L < liability, place catch-up bets on selections with lower matched exposure.
        /// </summary>
        private bool ReplaceUnmatchedBetsIfNeeded(List<PlaceInstruction> bets)
        {
            try
            {
                logger.Info("ReplaceUnmatchedBetsIfNeeded: Starting check...");
                var unmatchedBets = GetUnmatchedBets();

                // No unmatched bets - place new bets normally
                if (unmatchedBets == null || unmatchedBets.CurrentOrders == null || unmatchedBets.CurrentOrders.Count == 0)
                {
                    logger.Info("ReplaceUnmatchedBets: No unmatched bets, proceeding with new placement");
                    return true;
                }

                logger.Info($"ReplaceUnmatchedBets: Found {unmatchedBets.CurrentOrders.Count} unmatched bet(s)");

                // Check if 3 seconds have passed since last placement
                double secondsSinceLastPlacement = (DateTime.Now - lastBetPlacementTime).TotalSeconds;

                // Only cancel if 3+ seconds have passed
                if (secondsSinceLastPlacement < 3.0)
                {
                    logger.Info($"ReplaceUnmatchedBets: Only {secondsSinceLastPlacement:F1}s since last placement, waiting");
                    return false;
                }

                logger.Info($"ReplaceUnmatchedBets: {secondsSinceLastPlacement:F1}s since last placement, proceeding to cancel unmatched");

#if !NOBET
                // Step 1: Cancel ALL unmatched bets and wait for response
                var cancelInstructions = new List<CancelInstruction>();
                var unmatchedBetLookup = new Dictionary<string, CurrentOrderSummary>();

                foreach (var unmatchedBet in unmatchedBets.CurrentOrders)
                {
                    if (unmatchedBet.SizeRemaining > 0)
                    {
                        cancelInstructions.Add(new CancelInstruction
                        {
                            BetId = unmatchedBet.BetId,
                            SizeReduction = unmatchedBet.SizeRemaining
                        });
                        unmatchedBetLookup[unmatchedBet.BetId] = unmatchedBet;
                        logger.Info($"ReplaceUnmatchedBets: Cancelling {GetRunnerName(unmatchedBet.SelectionId)} bet {unmatchedBet.BetId} - £{unmatchedBet.SizeRemaining.ToString(Globals.DOUBLE_FORMAT)}@{unmatchedBet.PriceSize.Price.ToString(Globals.DOUBLE_FORMAT)}");
                    }
                }

                if (cancelInstructions.Count == 0)
                {
                    logger.Info("ReplaceUnmatchedBets: No unmatched bets to cancel");
                    return true;
                }

                // Send cancel request and WAIT for response
                logger.Info($"ReplaceUnmatchedBets: Sending cancel request for {cancelInstructions.Count} bet(s)...");
                var cancelResults = Globals.Exchange.CancelOrders(exchangeId, marketId, cancelInstructions, null);

                // Step 2: Check which bets were successfully cancelled
                var successfullyCancelledBySelection = new Dictionary<long, CurrentOrderSummary>();

                if (cancelResults?.InstructionReports != null)
                {
                    foreach (var report in cancelResults.InstructionReports)
                    {
                        if (report.Status == InstructionReportStatusEnum.SUCCESS && unmatchedBetLookup.ContainsKey(report.Instruction.BetId))
                        {
                            var cancelledBet = unmatchedBetLookup[report.Instruction.BetId];
                            successfullyCancelledBySelection[cancelledBet.SelectionId] = cancelledBet;
                            logger.Info($"ReplaceUnmatchedBets: Successfully cancelled {GetRunnerName(cancelledBet.SelectionId)} bet {report.Instruction.BetId}");
                        }
                        else
                        {
                            logger.Info($"ReplaceUnmatchedBets: Failed to cancel bet {report.Instruction.BetId} - {report.ErrorCode}");
                        }
                    }
                }

                logger.Info($"ReplaceUnmatchedBets: {successfullyCancelledBySelection.Count} bets successfully cancelled");

                // Step 3: NO CONSOLIDATION - just cancelled, don't replace
                // Catch-up logic will handle placing bets on selections that are behind
#else
                logger.Info($"ReplaceUnmatchedBets: NOBET mode - would cancel and replace {unmatchedBets.CurrentOrders.Count} bet(s)");
#endif

                return false; // Bets already placed/handled, don't place again
            }
            catch (Exception ex)
            {
                logger.Error($"ReplaceUnmatchedBets: {ex.Message}");
                return false;
            }
        }
    }
}
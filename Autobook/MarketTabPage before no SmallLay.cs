using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
#if TEST
using BetfairE = BetfairClient.Framework.Test.com.betfair.api6.exchange;
using BetfairG = BetfairClient.Framework.Test.com.betfair.api6.global;
#else
using BetfairE = BetfairClient.Framework.com.betfair.api6.exchange;
using BetfairG = BetfairClient.Framework.com.betfair.api6.global;
#endif
using BetfairClient.Framework;
using System.Threading;
using PthLog;

namespace Autobook
{
	public class MarketTabPage : TabPage
	{
		private const int MinutesToStart = 30;
		private const int CheckStartTimeRate = 60; // in seconds
		private const string DoubleFormat = "###0.00";

		public int marketId = 0;
		public int exchangeId;
		private BetfairE.Market market = null;
		private System.Windows.Forms.Panel panelRunners;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripButton toolStripButtonStart;
		private System.Windows.Forms.ToolStripButton toolStripButtonStop;
		private System.Windows.Forms.ToolStripButton toolStripButtonClose;
		private System.Windows.Forms.Panel panelBets;
		private System.Windows.Forms.ListView listViewBets;
		//private System.Windows.Forms.ColumnHeader columnHeaderBetType;
		private System.Windows.Forms.ColumnHeader columnHeaderBetType;
		private System.Windows.Forms.ColumnHeader columnHeaderBetOdds;
		private System.Windows.Forms.ColumnHeader columnHeaderBetSize;
		private System.Windows.Forms.ColumnHeader columnHeaderBetTime;
		private Dictionary<int, MarketView2> _views = new Dictionary<int, MarketView2> ();
		private List<int> _tradeoutBetsPlaced = new List<int> ();
		private BookView _bookView;
		private ManualResetEvent _stopHandling = new ManualResetEvent (false);
		private ManualResetEvent _stopRefreshHandling = new ManualResetEvent (false);
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private Thread _handlingThread;
		private ColumnHeader columnHeaderName;
		private ColumnHeader columnHeaderStatus;
		private bool _isInPlay = false;
		private Dictionary<int, double> _smallLayAmounts = new Dictionary<int, double> ();
		private Dictionary<int, double> _smallBackAmounts = new Dictionary<int, double> ();
		//private int totalBets = 0;
		//private Thread _refreshThread;
		private Logger _logger;
		//private object _lockDisplayBets = new object ();
		//public event MethodInvoker OnDisplayWallet;
		//private int RefreshRate = 1000;
		private Dictionary<int, BetfairE.ProfitAndLoss> pandls;
		private bool isPRProfitReached = false;
		private bool isIPProfitReached = false;
		private DateTime startTime;
		private System.Threading.Timer startTimer;
		private bool isStarted = false;
		private object forLocking = new object ();
		private object forLockingPAndL = new object ();
        //private BetfairE.MarketStatusEnum _previousMarketStatus = BetfairE.MarketStatusEnum.INACTIVE;
		private BetfairE.MarketStatusEnum _marketStatus = BetfairE.MarketStatusEnum.INACTIVE;

		private delegate void PlaceBetDelegate (BetfairE.RunnerPrices[] runnerPrices, BetType type, double amount);

		//public MarketTabPage ()
		//{
		//}

		public MarketTabPage (int marketId, int exchangeId, DateTime startTime)
		{
			try
			{
				InitializeComponent ();
				this.marketId = marketId;
				this.exchangeId = exchangeId;
				this.startTime = startTime;
				market = Globals.Exchange.GetMarket (exchangeId, marketId);
				Globals.CheckNetworkStatusIsConnected ();
				this.Text = BetfairClient.Framework.Exchange.GetMarketName (market);
				string mn = market.name.Replace (':', '_').Replace ('/', '_').Replace ('.', '_').Trim ();
				string filename = Globals.MakeLogName (mn + "_" + market.marketId.ToString ());
				_logger = LogManager.Current.AddLogger
					(filename + ".txt", filename + ".txt", "%date [%-16thread] %-5level - %message%n");
				TimeSpan toStart = startTime - DateTime.Now.ToUniversalTime ();
				if (toStart.TotalMilliseconds > 0)
					startTimer = new System.Threading.Timer
						(CheckStartTime, null, 5000, CheckStartTimeRate * 1000);
			}
			catch (Exception ex)
			{
				Globals.UnhandledException (ex);
				//_logger.Error (ex.ToString ());
			}
		}

		private void InitializeComponent ()
		{
			this.listViewBets = new System.Windows.Forms.ListView ();
			this.columnHeaderName = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderBetType = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderStatus = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderBetOdds = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderBetSize = new System.Windows.Forms.ColumnHeader ();
			this.columnHeaderBetTime = new System.Windows.Forms.ColumnHeader ();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip ();
			this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton ();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator ();
			this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton ();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator ();
			this.toolStripButtonClose = new System.Windows.Forms.ToolStripButton ();
			this.panelRunners = new System.Windows.Forms.Panel ();
			this.panelBets = new System.Windows.Forms.Panel ();
			this.toolStrip2.SuspendLayout ();
			this.panelBets.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// listViewBets
			// 
			this.listViewBets.Columns.AddRange (new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderBetType,
            this.columnHeaderStatus,
            this.columnHeaderBetOdds,
            this.columnHeaderBetSize,
            this.columnHeaderBetTime});
			this.listViewBets.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewBets.FullRowSelect = true;
			this.listViewBets.GridLines = true;
			this.listViewBets.Location = new System.Drawing.Point (0, 0);
			this.listViewBets.Name = "listViewBets";
			this.listViewBets.Size = new System.Drawing.Size (520, 630);
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
			this.toolStrip2.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonStart,
            this.toolStripSeparator1,
            this.toolStripButtonStop,
            this.toolStripSeparator2,
            this.toolStripButtonClose});
			this.toolStrip2.Location = new System.Drawing.Point (3, 3);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size (1004, 25);
			this.toolStrip2.TabIndex = 33;
			this.toolStrip2.Text = "toolStrip2";
			// 
			// toolStripButtonStart
			// 
			this.toolStripButtonStart.Image = global::Autobook.Properties.Resources.Play1Hot_32;
			this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonStart.Name = "toolStripButtonStart";
			this.toolStripButtonStart.Size = new System.Drawing.Size (51, 22);
			this.toolStripButtonStart.Text = "Start";
			this.toolStripButtonStart.Click += new System.EventHandler (this.toolStripButtonStart_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size (6, 25);
			// 
			// toolStripButtonStop
			// 
			this.toolStripButtonStop.Enabled = false;
			this.toolStripButtonStop.Image = global::Autobook.Properties.Resources.Stop32;
			this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonStop.Name = "toolStripButtonStop";
			this.toolStripButtonStop.Size = new System.Drawing.Size (51, 22);
			this.toolStripButtonStop.Text = "Stop";
			this.toolStripButtonStop.Click += new System.EventHandler (this.toolStripButtonStop_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size (6, 25);
			// 
			// toolStripButtonClose
			// 
			this.toolStripButtonClose.Image = global::Autobook.Properties.Resources.WindowClose;
			this.toolStripButtonClose.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonClose.Name = "toolStripButtonClose";
			this.toolStripButtonClose.Size = new System.Drawing.Size (56, 22);
			this.toolStripButtonClose.Text = "Close";
			this.toolStripButtonClose.Click += new System.EventHandler (this.toolStripButtonClose_Click);
			// 
			// panelRunners
			// 
			this.panelRunners.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelRunners.Location = new System.Drawing.Point (3, 33);
			this.panelRunners.Name = "panelRunners";
			this.panelRunners.Size = new System.Drawing.Size (651, 630);
			this.panelRunners.TabIndex = 32;
			// 
			// panelBets
			// 
			this.panelBets.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panelBets.Controls.Add (this.listViewBets);
			this.panelBets.Location = new System.Drawing.Point (660, 31);
			this.panelBets.Name = "panelBets";
			this.panelBets.Size = new System.Drawing.Size (532, 634);
			this.panelBets.TabIndex = 3;
			// 
			// MarketTabPage
			// 
			this.Controls.Add (this.toolStrip2);
			this.Controls.Add (this.panelRunners);
			this.Controls.Add (this.panelBets);
			this.Location = new System.Drawing.Point (4, 22);
			this.Padding = new System.Windows.Forms.Padding (3);
			this.Size = new System.Drawing.Size (1010, 671);
			this.UseVisualStyleBackColor = true;
			this.toolStrip2.ResumeLayout (false);
			this.toolStrip2.PerformLayout ();
			this.panelBets.ResumeLayout (false);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		private void CheckStartTime (object state)
		{
			try
			{
				TimeSpan toStart = startTime - DateTime.Now.ToUniversalTime ();
				if ((toStart.TotalMinutes < MinutesToStart) && (toStart.TotalMinutes > 0))
				{
					startTimer.Dispose ();
					_logger.Info (string.Format ("Auto Start, less than {0} to go", MinutesToStart));
					_logger.Info ("Auto Start, reload runners list");
					FillMarketPrices ();
					Invoke (new MethodInvoker (delegate () { Start (); }));
				}
			}
			catch (Exception ex)
			{
				Globals.UnhandledException (ex);
			}
		}

		private void toolStripButtonStart_Click (object sender, EventArgs e)
		{
			Start ();
		}

		private void Start ()
		{
			try
			{
				//_logger.Info ("Start: lock enter");
				lock (forLocking)
				{
					if (isStarted)
					{
						_logger.Info ("Start: already started");
						return;
					}
					isStarted = true;
				}
				//_logger.Info ("Start: lock release");
				isPRProfitReached = false;
				isIPProfitReached = false;
				UpdatePrices ();
				toolStripButtonStart.Enabled = false;
				toolStripButtonStop.Enabled = true;
				_stopHandling.Reset ();
				_logger.Info (string.Format ("Start: {0} back {1} lay {2}",
					BetfairClient.Framework.Exchange.GetMarketName (market), BackAmount, LayAmount));
				_logger.Info (string.Format ("Profit targets IP {0}, PR {1}",
					AppSettings.IPProfit, AppSettings.PRProfit));
				_handlingThread = new Thread (HandlingProc);
				_handlingThread.IsBackground = true;
				_handlingThread.Name = "HandlingProc";
				_handlingThread.Start ();
				//if (totalBets > 0)
				//StartRefreshThread ();
			}
			catch (Exception ex)
			{
				Globals.UnhandledException (ex);
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
		private void toolStripButtonStop_Click (object sender, EventArgs e)
		{
			Stop ();
		}

		public void Stop ()
		{
			try
			{
				if (InvokeRequired)
				{
					Invoke (new MethodInvoker (Stop));
					return;
				}
				//_logger.Info ("Stop: lock enter");
				lock (forLocking)
				{
					if (!isStarted)
					{
						_logger.Info ("Stop: already stopped");
						return;
					}
					isStarted = false;
				}
				//_logger.Info ("Stop: lock release");

				if (startTimer != null)
					startTimer.Dispose ();
				toolStripButtonStart.Enabled = true;
				toolStripButtonStop.Enabled = false;
				_stopHandling.Set ();
				//StopRefreshThread ();
				if (_handlingThread != null)
					_handlingThread.Join ((int) (RefreshInterval * 1.5d));
				// update p&l
				if (_marketStatus == BetfairE.MarketStatusEnum.ACTIVE)
					DisplayProfitAndLoss ();
				UpdatePrices (true);
			}
			catch (Exception ex)
			{
				Globals.UnhandledException (ex);
			}
		}

		private void toolStripButtonClose_Click (object sender, EventArgs e)
		{
			Stop ();
			((TabControl) this.Parent).TabPages.Remove (this);
		}

		private void HandlingProc ()
		{
			try
			{
				int count = 0;
				while (!_stopHandling.WaitOne (RefreshInterval, false))
				{
					//try
					//{
						//System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
						if (_marketStatus == BetfairE.MarketStatusEnum.ACTIVE)
						{
							//sw.Start ();
							DisplayBets ();
							//sw.Stop ();
							//Console.WriteLine ("DisplayBets (rounded): {0} ms", sw.ElapsedMilliseconds);
							//sw.Start ();
							UpdatePrices ();
							//sw.Stop ();
							//Console.WriteLine ("UpdatePrices (rounded): {0} ms", sw.ElapsedMilliseconds);
						}
						else
						{
							if (count == 5)
							{
								count = 0;
								//sw.Start ();
								UpdatePrices ();
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
				Globals.UnhandledException (ex);
				Stop ();
			}
			finally
			{
				_handlingThread = null;
				_stopHandling.Reset ();
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
		public void UpdatePrices ()
		{
			UpdatePrices (false);
		}

		public void UpdatePrices (bool isForcedRefresh)
		{
			try
			{
				//DateTime before = DateTime.Now;
				BetfairE.MarketPrices marketPrices = Globals.Exchange.GetMarketPrices (exchangeId, marketId);
				_marketStatus = marketPrices.marketStatus;
				//TimeSpan after = DateTime.Now - before;
				//Console.WriteLine ("{0} GetMarketPrices {1} ms",
				//  marketId, after.TotalMilliseconds);

				Globals.CheckNetworkStatusIsConnected ();
				if ((!isForcedRefresh) && (marketPrices.marketStatus == BetfairE.MarketStatusEnum.CLOSED))
				{
					_logger.Info (string.Format ("market {0} closed, stop updating",
						BetfairClient.Framework.Exchange.GetMarketName (market)));
					Stop ();
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

				bool isInPlay = ((marketPrices.delay > 0) &&
					(marketPrices.marketStatus == BetfairE.MarketStatusEnum.ACTIVE));
				if (_isInPlay != isInPlay)
				{
					if (_isInPlay)
					{
						if (_logger.IsInfoEnabled)
							_logger.Info (string.Format ("market {0} not in play: {1}",
							BetfairClient.Framework.Exchange.GetMarketName (market), marketPrices.marketStatus.ToString ()));
					}
					else
					{
						if (_logger.IsInfoEnabled)
							_logger.Info (string.Format ("market {0} goes in play: back {1} lay {2} low back {3} low lay {4}",
							BetfairClient.Framework.Exchange.GetMarketName (market), BackAmount, LayAmount,
							AppSettings.LowBack, AppSettings.LowLay));
						FillMarketPrices ();
					}
					_isInPlay = isInPlay;
				}

				if ((!isForcedRefresh) && (isIPProfitReached && _isInPlay))
				{
					_logger.Info ("profit reached, stopping");
					Stop ();
					return;
				}

				foreach (BetfairE.RunnerPrices runnerPrice in marketPrices.runnerPrices)
				{
					if (_views.ContainsKey (runnerPrice.selectionId))
					{
						//_logger.Info ("UpdatePrices: lock enter " + GetRunnerName (runnerPrice.selectionId));
						lock (forLockingPAndL)
						{
							_views[runnerPrice.selectionId].UpdateOdds (runnerPrice);
						}
						//_logger.Info ("UpdatePrices: lock release " + GetRunnerName (runnerPrice.selectionId));
					}
					else
						_logger.Error ("UpdatePrices: cannot find selection " + GetRunnerName (runnerPrice.selectionId));
				}
				if ((isPRProfitReached) && (!_isInPlay))
					// stop placing bets, wait for market going InPlay
					DisplayBookPercentages (marketPrices.runnerPrices, false);
				else
					DisplayBookPercentages (marketPrices.runnerPrices, marketPrices.marketStatus == BetfairE.MarketStatusEnum.ACTIVE);
			}
			catch (Exception ex)
			{
				//  _logger.Error (ex.ToString ());
				Globals.UnhandledException (ex);
			}
		}

		public void FillMarketPrices ()
		{
			try
			{
				if (InvokeRequired)
				{
					Invoke (new MethodInvoker (FillMarketPrices));
					return;
				}

				//_logger.Info ("FillMarketPrices: lock enter");
				lock (forLockingPAndL)
				{
					_views.Clear ();
				}
				//_logger.Info ("FillMarketPrices: lock release");
				panelRunners.SuspendLayout ();
				BetfairE.MarketPrices marketPrices = Globals.Exchange.GetMarketPrices (exchangeId, marketId);
				panelRunners.Controls.Clear ();
				panelRunners.AutoScroll = true;
				// add in reverse order
				AddBookPrices ();
				for (int i = marketPrices.runnerPrices.Length - 1; i >= 0; i--)
				{
					AddRunnerPrices (Exchange.FindRunner (market, marketPrices.runnerPrices[i].selectionId), marketPrices.runnerPrices[i]);
				}
				/*
        foreach (BetfairE.RunnerPrices runnerPrice in marketPrices.runnerPrices)
        {
          AddRunnerPrices (Exchange.FindRunner (market, runnerPrice.selectionId), runnerPrice);
        }
				*/
				DisplayBookPercentages (marketPrices.runnerPrices, false);
				string info = string.Empty;
				if (market == null)
				{
					info = "; No names; exceeded API throttle";
				}
				//columnHeaderSelection.Text = string.Format ("Selections: {0}{1}", marketPrices.runnerPrices.Length, info);
				Globals.CheckNetworkStatusIsConnected ();
			}
			catch (Exception ex)
			{
				Globals.UnhandledException (ex);
			//  if (Globals.OnKeepAliveError != null)
			//    Globals.OnKeepAliveError (ex.Message);
			//  //MessageBox.Show (ex.Message, "Fill Market Prices", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				panelRunners.ResumeLayout ();
			}
		}

		private double GetAmount (BetfairE.Price price)
		{
			double amount = 0.0d;

			switch (LayBetMode)
			{
				case LayBetMode.Payout:
					amount = price.price * price.amountAvailable;
					break;

				case LayBetMode.Liability:
					if (price.price > 1.0d)
						amount = (price.price - 1.0d) * price.amountAvailable;
					break;
			}

			return amount;
		}

		private void TradeOutCheck (BetfairE.RunnerPrices[] runnerPrices, BetfairE.RunnerPrices selectedRP, bool isLayPrice)
		{
			//try
			//{
			double price = (isLayPrice) ? selectedRP.bestPricesToLay[0].price : selectedRP.bestPricesToBack[0].price;
			_logger.Info (string.Format ("TradeOutCheck: price {0} < topr {1}, {2} isLay {3}",
				price.ToString (DoubleFormat), AppSettings.TradeOutTrigger,
				GetRunnerName (selectedRP.selectionId), isLayPrice.ToString ()));
			// 2. Check if selection pl > 0
			if (_views.ContainsKey (selectedRP.selectionId))
			{
				if (!_tradeoutBetsPlaced.Contains (selectedRP.selectionId))
				{
					if (_views[selectedRP.selectionId].ProfitLiability > 0.0D)
					{
						_logger.Info ("TradeOutCheck: sel p&l = " +
							_views[selectedRP.selectionId].ProfitLiability.ToString (DoubleFormat));
						// 3. Check if any other selections pl < 0z
						double toam = double.MaxValue;
						int toamSelectionId = -1;

						double topa = _views[selectedRP.selectionId].ProfitLiability *
							(AppSettings.TradeOutPercent / 100.0D);
						bool isAllGreen = IsAllGreen ();
						if (isAllGreen)
						{
							_logger.Info ("TradeOutCheck: all selections are green");
							InternalPlaceBet (topa, selectedRP.selectionId);
							return;
						}

						//_logger.Info ("TradeOutCheck: lock enter");
						lock (forLockingPAndL)
						{
							foreach (KeyValuePair<int, MarketView2> kvp in _views)
							{
								if (kvp.Value.ProfitLiability < 0)
								{
									// 4. Save trade out selection current profit (cupr)
									// Save selection pl that has largest potential loss (biggest red) – toam
									// (ignore selection if current price is 1000)
									if (kvp.Value.ProfitLiability < toam)
									{
										bool isSelection1000 = false;
										foreach (BetfairE.RunnerPrices rp in runnerPrices)
										{
											if (rp.selectionId == kvp.Key)
											{
												isSelection1000 = rp.lastPriceMatched == Globals.MaxPrice;
												break;
											}
										}

										if (!isSelection1000)
										{
											toam = kvp.Value.ProfitLiability;
											toamSelectionId = kvp.Key;
											_logger.Info ("TradeOutCheck: largest current potential loss (biggest red) = " +
												toam.ToString (DoubleFormat));
										}
										else
											_logger.Info ("TradeOutCheck: ignore selection price = 1000");
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
							toam = Math.Abs (toam);
							_logger.Info (string.Format ("TradeOutCheck: topa = {0} toam = {1}",
								topa.ToString (DoubleFormat), toam.ToString (DoubleFormat)));
							// Is toam > topa
							// If yes, then set topa = toam : goto 7       
							if (toam > topa)
							{
								topa = toam;
								_logger.Info ("TradeOutCheck: topa forced to toam = " + topa.ToString (DoubleFormat));
							}
							// If topa*(topr-1) > cupr then topa=cupr*(topr-1)
							// 200*1=200>100, topa=100*1 = 100
							if ((topa * (AppSettings.TradeOutPrice - 1.0D)) > _views[selectedRP.selectionId].ProfitLiability)
							{
								topa = _views[selectedRP.selectionId].ProfitLiability * (AppSettings.TradeOutPrice - 1.0D);
								_logger.Info ("TradeOutCheck: topa forced to cupr*(topr-1) = " + topa.ToString (DoubleFormat));
							}
							// 6. Is topa > 2
							if (topa <= 2.0D)
							{
								topa = 2.0D;
								_logger.Info ("TradeOutCheck: topa forced to 2.0");
							}
							// 7. Place lay bet topa @ topr
							InternalPlaceBet (topa, selectedRP.selectionId);
							/*
							BetfairE.PlaceBets[] betsArray = new BetfairE.PlaceBets[1];
							betsArray[0] = new BetfairE.PlaceBets ();
							//if (isLayPrice)
							betsArray[0].price = AppSettings.TradeOutPrice;
							//else
							//  betsArray[0].price = (double) Exchange.IncrementPrice ((decimal) selectedRP.bestPricesToBack[0].price);
							betsArray[0].betCategoryType = BetfairE.BetCategoryTypeEnum.E;
							betsArray[0].betPersistenceType = BetfairE.BetPersistenceTypeEnum.NONE;
							betsArray[0].marketId = market.marketId;
							betsArray[0].size = Math.Round (topa, 2);
							betsArray[0].selectionId = selectedRP.selectionId;
							betsArray[0].bspLiability = 0.0d;
							betsArray[0].betType = BetfairE.BetTypeEnum.L;

							_logger.Info (string.Format ("TradeOutCheck: placing lay bet {0} @ {1}",
								betsArray[0].size.ToString (), betsArray[0].price.ToString (DoubleFormat)));

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
							//    to cancel all outstanding bets, and stop trading if trade-out bet has been matched and all selections are ‘green’ (p/l >0)
							/*
							if (AppSettings.TradeOutEnd != TradeOutEndMode.Continue)
							{
								int?[] markets = new int?[1];
								markets[0] = marketId;
								// check all selections are ‘green’ (p/l >0)
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
								int?[] markets = new int?[1];
								markets[0] = marketId;
								// check all selections are ‘green’ (p/l >0)
								isAllGreen = IsAllGreen ();

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
								else
									_logger.Info ("TradeOutCheck: cannot stop, not all selections are green");
							}
							else
							{
								// get p&l to avoid double tradeout bet placement when tradeout mode = Continue
								//Globals.Exchange.GetMarketProfitAndLoss (exchangeId, marketId);
							}
						}
						else
							_logger.Error ("TradeOutCheck: no largest current potential loss (biggest red)");

					}
					else
						_logger.Info ("TradeOutCheck: p&l not positive " +
							_views[selectedRP.selectionId].ProfitLiability);
				}
				else
					_logger.Info ("TradeOutCheck: trade out bet already placed for this selection");
			}
			else
				_logger.Error ("TradeOutCheck: cannot find p&l");
			//}
			//catch (Exception ex)
			//{
			//  _logger.Error ("TradeOutCheck: " + ex.ToString ());
			//}
			Globals.CheckNetworkStatusIsConnected ();
		}

		private void InternalPlaceBet (double size, int selectionId)
		{
			BetfairE.PlaceBets[] betsArray = new BetfairE.PlaceBets[1];
			betsArray[0] = new BetfairE.PlaceBets ();
			//if (isLayPrice)
			betsArray[0].price = AppSettings.TradeOutPrice;
			//else
			//  betsArray[0].price = (double) Exchange.IncrementPrice ((decimal) selectedRP.bestPricesToBack[0].price);
			betsArray[0].betCategoryType = BetfairE.BetCategoryTypeEnum.E;
			betsArray[0].betPersistenceType = BetfairE.BetPersistenceTypeEnum.NONE;
			betsArray[0].marketId = market.marketId;
			betsArray[0].size = Math.Round (size, 2);
			betsArray[0].selectionId = selectionId;
			betsArray[0].bspLiability = 0.0d;
			betsArray[0].betType = BetfairE.BetTypeEnum.L;

			_logger.Info (string.Format ("InternalPlaceBet: placing lay bet {0} @ {1}",
				betsArray[0].size.ToString (), betsArray[0].price.ToString (DoubleFormat)));

			//IList<BetfairE.PlaceBetsResult> placeResults = null;
			IList<BetfairE.PlaceBetsResult> placeResults = Globals.Exchange.PlaceBets (exchangeId, betsArray);
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
				_logger.Info ("InternalPlaceBet: aborting, trade-out bet not placed, status = " +
					placeResults[0].resultCode.ToString ());
				return;
			}

			// only 1 tradeout bet allowed per selection
			_tradeoutBetsPlaced.Add (selectionId);
			_logger.Info ("InternalPlaceBet: no more bets will be allowed on this selection " + GetRunnerName (selectionId));
		}

		private bool IsAllGreen ()
		{
			//try
			//{
				Dictionary<int, BetfairE.ProfitAndLoss> pals =
					Globals.Exchange.GetMarketProfitAndLoss (exchangeId, marketId);
				Globals.CheckNetworkStatusIsConnected ();
				bool isAllGreen = (pals.Count > 0);
				foreach (KeyValuePair<int, BetfairE.ProfitAndLoss> pandl in pals)
				{
					_logger.Info (string.Format ("IsAllGreen: selection {0} p&l {1}",
							GetRunnerName (pandl.Value.selectionId), pandl.Value.ifWin.ToString ()));
					if (pandl.Value.ifWin <= 0.0D)
					{
						isAllGreen = false;
						break;
					}
				}
				_logger.Info ("IsAllGreen: " + isAllGreen);

				return isAllGreen;
			//}
			//catch (Exception ex)
			//{
			//  _logger.Error ("IsAllGreen: " + ex);
				//return false;
			//}
		}

		private void DisplayBookPercentages (BetfairE.RunnerPrices[] runnerPrices, bool isPlaceBet)
		{
			double layBook = 0.0d;
			double backBook = 0.0d;
			bool isBackAvailable = true;
			bool isBackValid = true;
			bool isSkipBackBet = false;
			bool isSkipLayBet = false;
			double minBackSize = double.MaxValue;
			double minLaySize = double.MaxValue;
			int layPriceCount = 0;
			double amount = 0.0d;
			double minSelectionPrice = double.MaxValue;

			foreach (BetfairE.RunnerPrices rp in runnerPrices)
			{
				// 1. Check if any selections current price is <= topr
				if ((rp.bestPricesToLay.Length > 0) &&
					(rp.bestPricesToLay[0].price <= AppSettings.TradeOutTrigger))
					TradeOutCheck (runnerPrices, rp, true);
				else
					if ((rp.bestPricesToBack.Length > 0) &&
						(rp.bestPricesToBack[0].price < (double) Exchange.DecrementPrice ((decimal) AppSettings.TradeOutTrigger)))
						TradeOutCheck (runnerPrices, rp, false);

				#region Back
				if (rp.bestPricesToBack.Length > 0)
				{
					if (rp.bestPricesToBack[0].price <= 1.01D)
					{
						_logger.Info (string.Format ("invalid back: {0} {1}",
							GetRunnerName (rp.selectionId), rp.bestPricesToBack[0].price.ToString (DoubleFormat)));
						isBackValid = false;
					}
					backBook += Globals.GetBookShare (rp.bestPricesToBack[0].price);
					if (_isInPlay)
					{
						if (rp.bestPricesToBack[0].price <= AppSettings.LowBack)
						{
							_logger.Info (string.Format ("low back: {0} {1}",
								GetRunnerName (rp.selectionId), rp.bestPricesToBack[0].price.ToString (DoubleFormat)));
							isSkipBackBet = true;
						}
					}
					amount = GetAmount (rp.bestPricesToBack[0]);
					if (amount < minBackSize)
						minBackSize = amount;
				}
				else
				{
					//DisplayDebugMessage (string.Format ("back invalid {0}: {1} ",
					//  DateTime.Now.ToLongTimeString (), GetRunnerName (rp.selectionId)));
					isBackAvailable = false;
				}
				#endregion // Back

				#region Lay
				if (rp.bestPricesToLay.Length > 0)
				{
					layPriceCount++;
					if (minSelectionPrice > rp.bestPricesToLay[0].price)
						minSelectionPrice = rp.bestPricesToLay[0].price;
					layBook += Globals.GetBookShare (rp.bestPricesToLay[0].price);
					if (_isInPlay)
					{
						if (rp.bestPricesToLay[0].price <= AppSettings.LowLay)
						{
							_logger.Info (string.Format ("low lay: {0} {1}",
								GetRunnerName (rp.selectionId), rp.bestPricesToLay[0].price.ToString (DoubleFormat)));
							isSkipLayBet = true;
						}
					}
					amount = GetAmount (rp.bestPricesToLay[0]);
					if (amount < minLaySize)
						minLaySize = amount;
				}
				#endregion // Lay
			}

			backBook *= 100.0d;
			layBook *= 100.0d;
#if UNMATCHABLE_BET
			backBook *= 0.95d;
			layBook *= 0.95d;
#endif
			DisplayBook (backBook, layBook, isBackAvailable);

			if (isPlaceBet)
			{
				if (isBackAvailable && !isSkipBackBet && isBackValid)
					if (backBook < (100.0d - AppSettings.UnderMargin))
						//BeginInvoke (new PlaceBetDelegate (PlaceBet), new object[] { runnerPrices, BetType.Back, Math.Min (BackAmount, minBackSize) });
						ThreadPool.QueueUserWorkItem (new WaitCallback (PlaceBet), new PlaceBetParam (runnerPrices, BetType.Back, Math.Min (BackAmount, minBackSize), minSelectionPrice));
				//PlaceBet (runnerPrices, BetType.Back, Math.Min (BackAmount, minBackSize));

				if (layBook > (100.0d + AppSettings.OverMargin))
				{
					if (layPriceCount >= 2)
					{
						if (!isSkipLayBet)
							//BeginInvoke (new PlaceBetDelegate (PlaceBet), new object[] { runnerPrices, BetType.Lay, Math.Min (LayAmount, minLaySize) });
							ThreadPool.QueueUserWorkItem (new WaitCallback (PlaceBet), new PlaceBetParam (runnerPrices, BetType.Lay, Math.Min (LayAmount, minLaySize), minSelectionPrice));
						//PlaceBet (runnerPrices, BetType.Lay, Math.Min (LayAmount, minLaySize));
					}
					else
						_logger.Info ("lay price count < 2 ");
				}
			}
		}

		private void DisplayBook (double backBook, double layBook, bool isBackAvailable)
		{
			//Font bookFont = new Font (listViewRunners.Font, FontStyle.Regular);

			_bookView.UpdateBook (backBook, layBook, isBackAvailable);
		}

		private void AddRunnerPrices (BetfairE.Runner runner, BetfairE.RunnerPrices runnerPrices)
		{
			//string selectionName = (runner == null) ? runnerPrices.selectionId.ToString () : runner.name;

			MarketView2 view = new MarketView2 (runner, runnerPrices);
			//_logger.Info ("AddRunnerPrices: lock enter " + GetRunnerName (runner.selectionId));
			lock (forLockingPAndL)
			{
				_views.Add (runner.selectionId, view);
			}
			//_logger.Info ("AddRunnerPrices: lock release " + GetRunnerName (runner.selectionId));
			view.Dock = DockStyle.Top;
			panelRunners.Controls.Add (view);
		}

		private void AddBookPrices ()
		{
			_bookView = new BookView ();
			_bookView.Dock = DockStyle.Top;
			panelRunners.Controls.Add (_bookView);
		}

		private string GetRunnerName (int selectionId)
		{
			BetfairE.Runner runner = Exchange.FindRunner (market, selectionId);
			string runnerName = (runner == null) ? selectionId.ToString () : runner.name;

			return runnerName;
		}

		#region Bets Handling
		void HandleBetResult (IList<BetfairE.PlaceBetsResult> placeResults)
		{
			if (placeResults != null)
			{
				List<BetfairE.CancelBets> betsToCancel = new List<BetfairE.CancelBets> ();
				foreach (BetfairE.PlaceBetsResult pbr in placeResults)
				{
					//_logger.Info (string.Format ("HandleBetResult: {0} {1} {2} {3} {4}",
					//  pbr.resultCode, pbr.success,
					//  pbr.averagePriceMatched, pbr.sizeMatched, pbr.betId));
					//if (pbr.sizeMatched <= 0.0D)
					//{
					BetfairE.CancelBets cancelBet = new BetfairE.CancelBets ();
					cancelBet.betId = pbr.betId;
					_logger.Info ("HandleBetResult: added for cancelling, bet #" + pbr.betId);

					betsToCancel.Add (cancelBet);
					//}
				}
				if (betsToCancel.Count > 0)
				{
					CancelBetsParam param = new CancelBetsParam (exchangeId, betsToCancel);
					//ThreadPool.QueueUserWorkItem (new WaitCallback (CancelBets), param);
					CancelBets (param);
				}
			}
			else
				_logger.Info ("HandleBetResult: place bet failed");
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
		//void	PlaceBet (BetfairE.RunnerPrices[] runnerPrices, BetType type, double amount)
		void PlaceBet (object state)
		{
			PlaceBetParam param = state as PlaceBetParam;
			if (param == null)
			{
				_logger.Error ("PlaceBet: invalid parameter");
				return;
			}
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
				BetfairE.PlaceBets placeBet = null;
				List<BetfairE.PlaceBets> bets = new List<BetfairE.PlaceBets> ();
#endif

				DateTime now = DateTime.Now;
				_logger.Info (string.Format ("PlaceBet {0} amount {1}",
					param.type.ToString (), param.amount.ToString (DoubleFormat)));
				bool isMaxLiabilityReached = false;
				if ((pandls != null) && (pandls.Count > 0))
					isMaxLiabilityReached = Globals.IsMaxLiabilityReached (pandls.Values, Liability);
				switch (param.type)
				{
					#region Back
					case BetType.Back:
						if ((isMaxLiabilityReached) && (BackBetDealMode == BetDealMode.NoBets))
						{
							_logger.Info ("PlaceBet: bet placement aborted, Back liability too high");
							return;
						}

						double backBook = 0.0d;
						double backAmountTotal = Globals.GetBackAmountTotal (param);

						foreach (BetfairE.RunnerPrices rp in param.runnerPrices)
						{
							if ((rp.bestPricesToBack == null) || (rp.bestPricesToBack.Length <= 0))
							{
								_logger.Info (string.Format ("PlaceBet: Back {0} price not available", GetRunnerName (rp.selectionId)));
								continue;
							}
							if ((rp.bestPricesToBack[0].price == Globals.MaxPrice) & _isInPlay)
							{
								_logger.Info (string.Format ("PlaceBet: Maximum Back {0} price skipped", GetRunnerName (rp.selectionId)));
								continue;
							}
							if ((IsBackOver) && (rp.bestPricesToBack[0].price < BackOver))
							{
								_logger.Info (string.Format ("PlaceBet: back not over: {0} {1}",
									GetRunnerName (rp.selectionId), rp.bestPricesToBack[0].price.ToString (DoubleFormat)));
								continue;
							}

							//switch (LayBetMode)
							//{
							//  case LayBetMode.Payout:
							backBook = Globals.GetBookShare (rp.bestPricesToBack[0].price);
							//    break;

							//  case LayBetMode.Liability:
							//    backBook = Globals.GetBookShareLiability (rp.bestPricesToBack[0].price);
							//    break;
							//}

							double betAmount = backBook * param.amount;
							double size = betAmount;
							size = HandleSmallBackBet (rp.selectionId, betAmount);
							if (size == 0.0d)
							{
								_logger.Info (string.Format ("PlaceBet: Back wait for overbet used up {0} {1} {2}",
									GetRunnerName (rp.selectionId), rp.bestPricesToBack[0].price.ToString (DoubleFormat),
									rp.bestPricesToBack[0].amountAvailable.ToString (DoubleFormat)));
								continue;
							}

							// liability routine check
							if (isMaxLiabilityReached)
							{
								if (pandls.ContainsKey (rp.selectionId))
								{
									if ((Math.Sign (pandls[rp.selectionId].ifWin) != -1) ||
										(Math.Abs (pandls[rp.selectionId].ifWin) < Liability))
									{
										_logger.Info (string.Format ("PlaceBet: bet placement skipped for this selection, Back liability too high {0} {1} > {2}",
											GetRunnerName (rp.selectionId), pandls[rp.selectionId].ifWin.ToString (DoubleFormat), Liability.ToString (DoubleFormat)));
										continue;
									}
								}
								else
								{
									_logger.Info ("PlaceBet: Back, cannot find current liability " +
										GetRunnerName (rp.selectionId));
								}
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
								double backLiability = Globals.GetLayLiability (rp.bestPricesToBack[0].price, betAmount) -
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
												GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (DoubleFormat), Liability.ToString (DoubleFormat)));
											return;
										}
										else
										{
											_logger.Info (string.Format ("PlaceBet: bet placement skipped for this selection, Back liability too high {0} {1} > {2}",
												GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (DoubleFormat), Liability.ToString (DoubleFormat)));
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
															rp.bestPricesToBack[0].price.ToString (DoubleFormat),
															rp.bestPricesToBack[0].amountAvailable.ToString (DoubleFormat), betAmount.ToString (DoubleFormat)));
														betAmount = Globals.MinBetAmount;
													}
							*/
							size = Math.Round (size, 2);
							_logger.Info (string.Format ("PlaceBet: Back {0} {3}@{1} {2}",
								GetRunnerName (rp.selectionId), rp.bestPricesToBack[0].price.ToString (DoubleFormat),
								rp.bestPricesToBack[0].amountAvailable.ToString (DoubleFormat), size.ToString (DoubleFormat)));
							placeBet = new BetfairE.PlaceBets ();

#if SIMULATION
						if (size > 0.0d)
						{
							lvi = new ListViewItem (GetRunnerName (rp.selectionId));
							lvi.SubItems.Add (type.ToString ());
							lvi.BackColor = Globals.BACK_COLOUR;
							lvi.SubItems.Add ("Matched");
							lvi.SubItems.Add (rp.bestPricesToBack[0].price.ToString (DoubleFormat));
							lvi.SubItems.Add (size.ToString ("###0.##"));
							lvi.SubItems.Add (now.ToLongTimeString ());
							listViewBets.Items.Add (lvi);
						}
#else

#if UNMATCHABLE_BET
							size = 4.0d;
							placeBet.price = 1000.0d;
#else
							placeBet.price = rp.bestPricesToBack[0].price;
#endif

							placeBet.betCategoryType = BetfairE.BetCategoryTypeEnum.E;
							placeBet.betPersistenceType = BetfairE.BetPersistenceTypeEnum.NONE;
							placeBet.marketId = market.marketId;
							placeBet.betType = BetfairE.BetTypeEnum.B;
							placeBet.size = size;
							placeBet.selectionId = rp.selectionId;
							placeBet.bspLiability = 0.0d;
							bets.Add (placeBet);
#endif
						}
						break;
					#endregion

					#region Lay
					case BetType.Lay:
						if ((isMaxLiabilityReached) && (LayBetDealMode == BetDealMode.NoBets))
						{
							_logger.Info ("PlaceBet: bet placement aborted, Lay liability too high");
							return;
						}

						double layAmountTotal = Globals.GetLayAmountTotal (param, LayBetMode);

						foreach (BetfairE.RunnerPrices rp in param.runnerPrices)
						{
							if ((rp.bestPricesToLay == null) || (rp.bestPricesToLay.Length <= 0))
							{
								_logger.Info (string.Format ("PlaceBet: {0} Lay price NA", GetRunnerName (rp.selectionId)));
								continue;
							}
							if (rp.bestPricesToLay[0].price == Globals.MaxPrice)
							{
								_logger.Info (string.Format ("PlaceBet: {0} Maximum Lay price skipped", GetRunnerName (rp.selectionId)));
								continue;
							}
							if ((IsLayOver) && (rp.bestPricesToLay[0].price < LayOver))
							{
								_logger.Info (string.Format ("PlaceBet: lay not over: {0} {1}",
									GetRunnerName (rp.selectionId), rp.bestPricesToLay[0].price.ToString (DoubleFormat)));
								continue;
							}
							if (HighLayMode == HighLayMode.Static)
							{
								if (rp.bestPricesToLay[0].price >= HighLay)
								{
									if (!IsIgnoreHighLay)
									{
										_logger.Info (string.Format ("PlaceBet: {0} Static High Lay skipped, price {1}",
											GetRunnerName (rp.selectionId), rp.bestPricesToLay[0].price));
										continue;
									}
									else
									{
										if (rp.bestPricesToLay[0].price <= HighLayIncrease)
										{
											// HighLay exceeded, Ignore HighLay set but HighLay Increase not exceeded
											// do nothing, go on check other conditions
										}
										else
										{
											_logger.Info (string.Format ("PlaceBet: {0} High Lay Increase skipped, price {1}",
												GetRunnerName (rp.selectionId), rp.bestPricesToLay[0].price));
											continue;
										}
									}
								}
							}
							else
							{
								if (rp.bestPricesToLay[0].price >= (param.minSelectionPrice * HighLayMultiplier))
								{
									_logger.Info (string.Format ("PlaceBet: {0} Dynamic High Lay skipped, price {1}",
										GetRunnerName (rp.selectionId), rp.bestPricesToLay[0].price));
									continue;
								}
							}

							double layBook = 0.0d;
							switch (LayBetMode)
							{
								case LayBetMode.Payout:
									layBook = Globals.GetBookShare (rp.bestPricesToLay[0].price);
									break;

								case LayBetMode.Liability:
									layBook = Globals.GetBookShareLiability (rp.bestPricesToLay[0].price);
									break;
							}

							double betAmount = layBook * param.amount;
							double size = betAmount;
							if (IsSmallLay)
							{
								if (betAmount < Globals.MinBetAmount)
								{
									size = HandleSmallLayBet (rp.selectionId, betAmount, rp.bestPricesToLay[0].price);
									if (size == 0.0d)
									{
										_logger.Info ("PlaceBet: no bet placed");
										continue;
									}
								}
							}
							else
							{
								if (betAmount < Globals.MinBetAmount)
								{
									_logger.Info (string.Format ("PlaceBet: Lay skip <£2 {0} {1} {2} {3}",
										GetRunnerName (rp.selectionId), rp.bestPricesToLay[0].price.ToString (DoubleFormat),
										rp.bestPricesToLay[0].amountAvailable.ToString (DoubleFormat), betAmount.ToString (DoubleFormat)));
									continue;
								}
							}

							// liability routine check
							if (isMaxLiabilityReached)
							{
								if (pandls.ContainsKey (rp.selectionId))
								{
									if ((Math.Sign (pandls[rp.selectionId].ifWin) == -1) &&
										(Math.Abs (pandls[rp.selectionId].ifWin) >= Liability))
									{
										_logger.Info (string.Format ("PlaceBet: bet placement skipped for this selection, Lay liability too high {0} {1} > {2}",
											GetRunnerName (rp.selectionId), pandls[rp.selectionId].ifWin.ToString (DoubleFormat), Liability.ToString (DoubleFormat)));
										continue;
									}
								}
								else
								{
									_logger.Info ("PlaceBet: Lay, cannot find current liability " +
										GetRunnerName (rp.selectionId));
								}
							}

							/*
							if (pandls.ContainsKey (rp.selectionId))
							{
								double layLiability = (layAmountTotal - betAmount) -
									Globals.GetLayLiability (rp.bestPricesToLay[0].price, betAmount);
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
												GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (DoubleFormat), Liability.ToString (DoubleFormat)));
											return;
										}
										else
										{
											_logger.Info (string.Format ("PlaceBet: bet placement skipped for this selection, Lay liability too high {0} {1} > {2}",
												GetRunnerName (rp.selectionId), liabilityIfPlaced.ToString (DoubleFormat), Liability.ToString (DoubleFormat)));
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

							size = Math.Round (size, 2);
							_logger.Info (string.Format ("PlaceBet: Lay {0} {1} {2} {3}",
								GetRunnerName (rp.selectionId), rp.bestPricesToLay[0].price.ToString (DoubleFormat),
								rp.bestPricesToLay[0].amountAvailable.ToString (DoubleFormat), size.ToString (DoubleFormat)));
							placeBet = new BetfairE.PlaceBets ();

#if SIMULATION
						if (size > 0.0d)
						{
							lvi = new ListViewItem (GetRunnerName (rp.selectionId));
							lvi.SubItems.Add (type.ToString ());
							lvi.BackColor = Globals.LAY_COLOUR;
							lvi.SubItems.Add ("Matched");
							lvi.SubItems.Add (rp.bestPricesToLay[0].price.ToString (DoubleFormat));
							lvi.SubItems.Add (size.ToString ("###0.##"));
							lvi.SubItems.Add (now.ToLongTimeString ());
							listViewBets.Items.Add (lvi);
						}
#else

#if UNMATCHABLE_BET
							size = 4.0d;
							placeBet.price = 1.01d;
#else
							placeBet.price = rp.bestPricesToLay[0].price;
#endif

							placeBet.betCategoryType = BetfairE.BetCategoryTypeEnum.E;
							placeBet.betPersistenceType = BetfairE.BetPersistenceTypeEnum.NONE;
							placeBet.marketId = market.marketId;
							placeBet.betType = BetfairE.BetTypeEnum.L;
							placeBet.size = size;
							placeBet.selectionId = rp.selectionId;
							placeBet.bspLiability = 0.0d;
							bets.Add (placeBet);
#endif
						}
						break;
					#endregion
				}

#if !SIMULATION

				//bets.Clear ();
				if (bets.Count > 0)
				{
					IList<BetfairE.PlaceBetsResult> placeResults = null;

					if (bets.Count > 60)
					{
						BetfairE.PlaceBets[] source = bets.ToArray ();
						BetfairE.PlaceBets[] betsArray = new BetfairE.PlaceBets[Globals.MaxPlaceBets];
						int index = 0;
						int left = bets.Count;
						int count = Math.Min (Globals.MaxPlaceBets, left);

						do
						{
							Buffer.BlockCopy (source, index, betsArray, 0, count);
							placeResults = Globals.Exchange.PlaceBets (exchangeId, betsArray);
							left -= Globals.MaxPlaceBets;
							if (left <= 0)
								break;
							index += count;
							count = Math.Min (Globals.MaxPlaceBets, left);
						} while (true);
					}
					else
					{
						placeResults = Globals.Exchange.PlaceBets (exchangeId, bets.ToArray ());
					}
					if ((_isInPlay) && (AppSettings.IsCancelBets))
						HandleBetResult (placeResults);
					//ThreadPool.QueueUserWorkItem (new WaitCallback (DisplayBets));
				}

				Globals.CheckNetworkStatusIsConnected ();
#endif

#if SIMULATION
			// add separator
			lvi = new ListViewItem (string.Empty);
			lvi.BackColor = System.Drawing.Color.LightGray;
			lvi.UseItemStyleForSubItems = true;
			listViewBets.Items.Add (lvi);
#endif
			}
			catch (Exception ex)
			{
				//_logger.Error (ex.ToString ());
				Globals.UnhandledException (ex);
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
		/*
				private void DisplayBets (object state)
				{
					DisplayBets ();
				}
		*/
		public void DisplayBets ()
		{
			try
			{
				if (InvokeRequired)
				{
					Invoke (new MethodInvoker (DisplayBets));
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
				DisplayProfitAndLoss ();
			}
			catch (Exception ex)
			{
				Globals.UnhandledException (ex);
			}
			//TimeSpan after = DateTime.Now - before;
			//Console.WriteLine ("{0} DisplayProfitAndLoss {1} ms",
			//  market.name, after.TotalMilliseconds);

			//if (OnDisplayWallet != null)
			//  OnDisplayWallet ();
			//listViewBets.EndUpdate ();
		}

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
				cf.ToString (DoubleFormat), amount.ToString (DoubleFormat), price.ToString (DoubleFormat),
				betAmount.ToString (DoubleFormat), GetRunnerName (selectionId)));
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

		private double HandleSmallBackBet (int selectionId, double amount)
		{
			double totalAmount = amount;
			double betAmount;

			if (_smallBackAmounts.ContainsKey (selectionId))
			{
				totalAmount = amount - _smallBackAmounts[selectionId];
				if (totalAmount >= Globals.MinBetAmount)
				{
					_smallBackAmounts.Remove (selectionId);
					betAmount = totalAmount;

					_logger.Info (string.Format ("HandleSmallBackBet: used up amount {0} betAmount {1} selection {2}",
						amount.ToString (DoubleFormat), betAmount.ToString (DoubleFormat), GetRunnerName (selectionId)));
				}
				else
				{
					if (totalAmount >= 0)
					{
						_smallBackAmounts[selectionId] = Globals.MinBetAmount - totalAmount;
						betAmount = Globals.MinBetAmount;
					}
					else
					{
						_smallBackAmounts[selectionId] -= amount;
						betAmount = 0.0d;
					}

					_logger.Info (string.Format ("HandleSmallBackBet: _smallBackAmounts {0} amount {1} betAmount {2} selection {3}",
						_smallBackAmounts[selectionId].ToString (DoubleFormat), amount.ToString (DoubleFormat),
						betAmount.ToString (DoubleFormat), GetRunnerName (selectionId)));
				}
			}
			else
			{
				if (totalAmount < Globals.MinBetAmount)
				{
					double overBet = Globals.MinBetAmount - totalAmount;

					_smallBackAmounts.Add (selectionId, overBet);
					// now, compute overbet and then, return min amount
					//betAmount = Globals.MinBetAmount;
						betAmount = Globals.MinBetAmount;

					_logger.Info (string.Format ("HandleSmallBackBet: overbet {3} amount {0} betAmount {1} selection {2}",
						amount.ToString (DoubleFormat), betAmount.ToString (DoubleFormat),
						GetRunnerName (selectionId), overBet.ToString (DoubleFormat)));
				}
				else
				{
					betAmount	 = totalAmount;
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
		private void DisplayProfitAndLoss ()
		{
			try
			{
				bool isProfitReached = true;
				//DateTime before = DateTime.Now;
				pandls = Globals.Exchange.GetMarketProfitAndLoss (exchangeId, marketId);
				//TimeSpan after = DateTime.Now - before;
				//Console.WriteLine ("{1} GetMarketProfitAndLoss {0} ms",
				//  market.name, after.TotalMilliseconds);
				foreach (KeyValuePair<int, BetfairE.ProfitAndLoss> pandl in pandls)
				{
					//_logger.Info (string.Format ("P&L: {0} {1}",
					//  pandl.Value.selectionName, pandl.Value.ifWin));
					if (pandl.Value.ifWin < Profit)
						isProfitReached = false;
					if (_views.ContainsKey (pandl.Key))
					{
						//_logger.Info ("DisplayProfitAndLoss: lock enter " + GetRunnerName (pandl.Key));
						lock (forLockingPAndL)
						{
							_views[pandl.Key].ProfitLiability = pandl.Value.ifWin;
						}
						//_logger.Info ("DisplayProfitAndLoss: lock release" + GetRunnerName (pandl.Key));
					}
				}
				if (isProfitReached)
				{
					if (_isInPlay)
					{
						if (!isIPProfitReached)
						{
							_logger.Info ("DisplayProfitAndLoss: IP profit is reached");
							isIPProfitReached = true;
						}
					}
					else
					{
						if (!isPRProfitReached)
						{
							_logger.Info ("DisplayProfitAndLoss: PR profit is reached");
							isPRProfitReached = true;
						}
					}
				}
				Globals.CheckNetworkStatusIsConnected ();
			}
			catch (Exception ex)
			{
				//  _logger.Error (ex.ToString ());
				Globals.UnhandledException (ex);
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
			public int exchangeId;
			public List<BetfairE.CancelBets> betsToCancel;

			public CancelBetsParam (int exchangeId, List<BetfairE.CancelBets> betsToCancel)
			{
				this.exchangeId = exchangeId;
				this.betsToCancel = betsToCancel;
			}
		}

		private void CancelBets (object state)
		{
			//try
			//{
				CancelBetsParam param = state as CancelBetsParam;
				if (param == null)
				{
					_logger.Error ("CancelBets: invalid parameter");
					return;
				}

				IList<BetfairE.CancelBetsResult> cancelResults =
					Globals.Exchange.CancelBets (param.exchangeId, param.betsToCancel);
				Globals.CheckNetworkStatusIsConnected ();
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

			lvi.SubItems.Add (price.ToString (DoubleFormat));
			lvi.SubItems.Add (size.ToString (DoubleFormat));
			lvi.SubItems.Add (placedDate.ToLongTimeString ());
			listViewBets.Items.Add (lvi);
		}
		*/
		#endregion

		#region Settings
		private double BackAmount
		{
			get { return (_isInPlay) ? AppSettings.IPBackAmount : AppSettings.PRBackAmount; }
		}

		private double LayAmount
		{
			get { return (_isInPlay) ? AppSettings.IPLayAmount : AppSettings.PRLayAmount; }
		}

		private double Liability
		{
			get { return (_isInPlay) ? AppSettings.IPLiability : AppSettings.PRLiability; }
		}

		private LayBetMode LayBetMode
		{
			get { return (_isInPlay) ? AppSettings.IPLayBetMode : AppSettings.PRLayBetMode; }
		}

		private double Profit
		{
			get { return (_isInPlay) ? AppSettings.IPProfit : AppSettings.PRProfit; }
		}

		private bool IsSmallLay
		{
			get { return (_isInPlay) ? AppSettings.IPIsSmallLay : AppSettings.PRIsSmallLay; }
		}

		public bool IsIgnoreHighLay
		{
			get { return (_isInPlay) ? AppSettings.IPIsIgnoreHighLay : AppSettings.PRIsIgnoreHighLay; }
		}

		private int RefreshInterval
		{
			get { return (_isInPlay) ? AppSettings.IPRefreshInterval : AppSettings.PRRefreshInterval; }
		}

		private double HighLay
		{
			get { return (_isInPlay) ? AppSettings.IPHighLay : AppSettings.PRHighLay; }
		}

		private double HighLayIncrease
		{
			get { return (_isInPlay) ? AppSettings.IPHighLayIncrease : AppSettings.PRHighLayIncrease; }
		}

		private double HighLayMultiplier
		{
			get { return (_isInPlay) ? AppSettings.IPHighLayMultiplier : AppSettings.PRHighLayMultiplier; }
		}

		private BetDealMode BackBetDealMode
		{
			get { return (_isInPlay) ? AppSettings.IPBackBetDealMode : AppSettings.PRBackBetDealMode; }
		}

		private BetDealMode LayBetDealMode
		{
			get { return (_isInPlay) ? AppSettings.IPLayBetDealMode : AppSettings.PRLayBetDealMode; }
		}

		private HighLayMode HighLayMode
		{
			get { return (_isInPlay) ? AppSettings.IPHighLayMode : AppSettings.PRHighLayMode; }
		}

		private bool IsLayOver
		{
			get { return (_isInPlay) ? AppSettings.IsLayOverIP : AppSettings.IsLayOverPR; }
		}

		public double LayOver
		{
			get { return (_isInPlay) ? AppSettings.LayOverIP : AppSettings.LayOverPR; }
		}

		private bool IsBackOver
		{
			get { return (_isInPlay) ? AppSettings.IsBackOverIP : AppSettings.IsBackOverPR; }
		}

		public double BackOver
		{
			get { return (_isInPlay) ? AppSettings.BackOverIP : AppSettings.BackOverPR; }
		}
		#endregion
	}
}

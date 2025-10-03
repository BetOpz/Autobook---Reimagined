using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using BetfairG = BetfairClient.Framework.com.betfair.api6.global;
using BetfairE = BetfairClient.Framework.com.betfair.api6.exchange;
using BetfairClient.Framework;

namespace Autobook
{
  public partial class FormMain : Form
  {
    #region Private Members
		private Panel pnlTree;
		//private BetfairE.Market market;
    private delegate void PlaceBetDelegate (BetfairE.RunnerPrices[] runnerPrices, BetType type);
    private PlaceBetDelegate placeBetDelegate;
    #endregion

    public FormMain ()
    {
      InitializeComponent ();

			//SetStyle (ControlStyles.DoubleBuffer, true);
			//SetStyle (ControlStyles.ResizeRedraw, true);

      placeBetDelegate = new PlaceBetDelegate (runnersControl1_OnPlaceBet);
      runnersControl1.OnPlaceBet += new RunnersControl.PlaceBetDelegate (runnersControl1_OnPlaceBet);
    }

    void treeViewMarkets_AfterSelect (object sender, TreeViewEventArgs e)
    {
			try
			{
				Cursor = Cursors.WaitCursor;
        // close node
        if (e.Action == TreeViewAction.Unknown)
          return;
        //treeViewMarkets.BeginUpdate ();
				INodeTag nodeTag = e.Node.Tag as INodeTag;
				switch (nodeTag.Type)
				{
					case NodeType.Root:
						break;

					case NodeType.Event:
						EventNodeTag ent = nodeTag as EventNodeTag;
            //comboBoxEvent.Text = e.Node.Text;
						Exchange.EventInfo info = Globals.exchange.GetEvents (ent.Id);
						foreach (BetfairG.BFEvent bfEvent in info.Events)
						{
							TreeNode eventNode = new TreeNode (bfEvent.eventName);
							eventNode.Tag = new EventNodeTag (bfEvent.eventId);
							e.Node.Nodes.Add (eventNode);
						}
						foreach (BetfairG.MarketSummary marketSummary in info.MarketsSummaries)
						{
							TreeNode marketNode = new TreeNode (MarketNodeTag.GetName (marketSummary));
							marketNode.Tag = new MarketNodeTag (marketSummary.marketId, marketSummary.exchangeId);
							e.Node.Nodes.Add (marketNode);
						}
						e.Node.ExpandAll ();
						break;

					case NodeType.Market:
            comboBoxTreeMarkets.TreeViewNodeSelect (this, null);
						MarketNodeTag mnt = nodeTag as MarketNodeTag;
						//panelRunners.Height = 494;
						//panelRunners.Show ();
            runnersControl1.SetMarket (mnt.ExchangeId, mnt.Id);
						runnersControl1.FillMarketPrices ();
						//this.Refresh ();
						break;

					case NodeType.Selection:
						SelectionNodeTag snt = nodeTag as SelectionNodeTag;

						/*
											foreach (BetfairE.RunnerPrices runnerPrices in marketPrices.runnerPrices)
											{
												Console.WriteLine ("{0} {1}", runnerPrices.sortOrder, runnerPrices.lastPriceMatched);
											}
						*/
						//RunnersControl rc = new RunnersControl ();
						//panelTop.Show ();
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show (ex.ToString (), "Select Node", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
        buttonStartStop.Enabled = runnersControl1.IsSet;
        //treeViewMarkets.EndUpdate ();
				Cursor = Cursors.Default;
			}
    }

    private void FormMain_FormClosing (object sender, FormClosingEventArgs e)
    {
			if (timerRefresh.Enabled)
				timerRefresh.Enabled = false;
      Application.Exit ();
    }

    private void FormMain_Load (object sender, EventArgs e)
    {
			try
			{
				comboBoxRefreshRate.SelectedIndex = 3;
        comboBoxTreeMarkets.AfterSelect += new TreeViewEventHandler (treeViewMarkets_AfterSelect);
				if (textBoxPreRace.Text.Length > 0)
					Settings.BetAmount = double.Parse (textBoxPreRace.Text);
				//panelRunners.Hide ();
				//panelRunners.Height = 0;
				FillMarketsTreeView ();
				//comboBoxEvent.Layout += new LayoutEventHandler (comboBoxEvent_Layout);
				//ResizeForm ();
			}
			catch (Exception ex)
			{
				MessageBox.Show (ex.ToString (), "Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
    }

/*
		void comboBoxEvent_Layout (object sender, LayoutEventArgs e)
		{
		}
*/

		private int GetTitlebarHeight ()
		{
			int borderWidth = (this.Width - this.ClientSize.Width) / 2;
			int titlebarHeight = this.Height - this.ClientSize.Height - 2 * borderWidth;

			return titlebarHeight;
		}
		/*
    private void ResizeForm ()
    {
			this.Height = panelTop.Height + panelRunners.Height + statusStrip1.Height + GetTitlebarHeight ();
    }
		*/
    private void comboBoxEvent_SelectedIndexChanged (object sender, EventArgs e)
    {
    }
/*
    private void FillMarketsTreeView ()
    {
      Cursor = Cursors.WaitCursor;
      treeViewMarkets.BeginUpdate ();
      try
      {
        treeViewMarkets.Nodes.Clear ();
        foreach (BetfairG.EventType eventType in Globals.exchange.GetActiveEventTypes ())
        {
          treeViewMarkets.Nodes.Add (new TreeNode (eventType.name));
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show (this, ex.Message);
      }
      finally
      {
        Cursor = Cursors.Default;
        treeViewMarkets.EndUpdate ();
      }
    }
	*/
		
    private void FillMarketsTreeView ()
				{
					Cursor = Cursors.WaitCursor;
          //treeViewMarkets.BeginUpdate ();
					try
					{
            comboBoxTreeMarkets.Nodes.Clear ();
            //treeViewMarkets.Nodes.Clear ();
            //TreeNode rootNode = treeViewMarkets.Nodes.Add ("Events");
            TreeNode rootNode = comboBoxTreeMarkets.Nodes.Add ("Events");
            rootNode.Tag = new RootNodeTag ();
						foreach (BetfairG.EventType eventType in Globals.exchange.GetActiveEventTypes ())
						{
              TreeNode eventNode = new TreeNode (eventType.name);
              eventNode.Tag = new EventNodeTag (eventType.id);
              rootNode.Nodes.Add (eventNode);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show (this, ex.Message);
					}
					finally
					{
						Cursor = Cursors.Default;
            comboBoxTreeMarkets.ExpandAll ();
            //treeViewMarkets.ExpandAll ();
            //treeViewMarkets.EndUpdate ();
					}
				}
		
		private void comboBoxRefreshRate_SelectedIndexChanged (object sender, EventArgs e)
		{
			int interval = 1000;

			switch (comboBoxRefreshRate.SelectedIndex)
			{
				case 0:
					interval = 100;
					break;

				case 1:
					interval = 200;
					break;

				case 2:
					interval = 500;
					break;

				case 3:
					interval = 1000;
					break;

				case 4:
					interval = 2000;
					break;

				case 5:
					interval = 5000;
					break;

				case 6:
					interval = 10000;
					break;

				case 7:
					interval = 30000;
					break;
			}

			timerRefresh.Interval = interval;
		}

		private void buttonStartStop_Click (object sender, EventArgs e)
		{
			if (!runnersControl1.IsSet)
				return;

			timerRefresh.Enabled = !timerRefresh.Enabled;
      if (timerRefresh.Enabled)
      {
        buttonStartStop.Text = "Stop";
        buttonStartStop.Image = global::Autobook.Properties.Resources.Stop_32;
      }
      else
      {
        buttonStartStop.Text = "Start";
        buttonStartStop.Image = global::Autobook.Properties.Resources.Play1Hot_32;
      }
		}

		private void timerRefresh_Tick (object sender, EventArgs e)
		{
			if (runnersControl1.IsSet)
				runnersControl1.FillMarketPrices ();
		}


    #region Place bets
    void HandleOnPlaceBet (BetfairE.RunnerPrices[] runnerPrices, BetType type)
    {
    }

    void runnersControl1_OnPlaceBet (BetfairE.RunnerPrices[] runnerPrices, BetType type)
    {
      if (InvokeRequired)
        Invoke (new PlaceBetDelegate (runnersControl1_OnPlaceBet), new object[] { runnerPrices, type });
      else
      {
        ListViewItem lvi;
        DateTime now = DateTime.Now;

        switch (type)
        {
          case BetType.Back:
            foreach (BetfairE.RunnerPrices rp in runnerPrices)
            {
              double backBook = Global.GetBookShare (rp.bestPricesToBack[0].price);
              double amount = backBook * Settings.BetAmount;
              lvi = new ListViewItem (type.ToString ());
              lvi.BackColor = Globals.BACK_COLOUR;
              lvi.SubItems.Add (rp.bestPricesToBack[0].price.ToString ("##0.00"));
              lvi.SubItems.Add (amount.ToString ("###0.##"));
              lvi.SubItems.Add (now.ToLongTimeString ());
              listViewBets.Items.Add (lvi);
            }
            break;

          case BetType.Lay:
            foreach (BetfairE.RunnerPrices rp in runnerPrices)
            {
              double layBook = Global.GetBookShare (rp.bestPricesToLay[0].price);
              double amount = layBook * Settings.BetAmount;
              lvi = new ListViewItem (type.ToString ());
              lvi.BackColor = Globals.LAY_COLOUR;
              lvi.SubItems.Add (rp.bestPricesToLay[0].price.ToString ("##0.00"));
              lvi.SubItems.Add (amount.ToString ("###0.##"));
              lvi.SubItems.Add (now.ToLongTimeString ());
              listViewBets.Items.Add (lvi);
            }
            break;
        }
      }
    }
    #endregion

     private void comboBoxEvent_DropDownClosed (object sender, EventArgs e)
    {
      Console.WriteLine ("comboBoxEvent_DropDownClosed");
      //ToggleTreeView (this, null);
    }

    private void comboBoxEvent_DropDown (object sender, EventArgs e)
    {
      Console.WriteLine ("comboBoxEvent_DropDown");
    }
  }
}
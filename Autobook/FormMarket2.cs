using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BetfairNgClient.Json;
using BetfairNgClient.Json.Enums;

namespace Autobook
{
	public partial class FormMarket2 : Form
	{
		private const int MarketId = 7;

		public delegate void OnMarketSelectedDelegate (int marketId, int exchangeId);
        private readonly TabControl tabControlMarkets;
	    private List<EventResult> events;
	    private readonly Dictionary<string, string> countriesTable = new Dictionary<string, string>()
			        {
			            {"AU", "Australia"},
			            {"FR", "France"},
			            {"DE", "Germany"},
			            {"IE", "Ireland"},
			            {"ZA", "South Africa"},
			            {"AE", "United Arab Emirates"},
			            {"GB", "United Kingdom"},
			            {"US", "United States"}
			        };

        public FormMarket2(TabControl tabControlMarkets)
		{
			InitializeComponent ();
            this.tabControlMarkets = tabControlMarkets;
		}

		private void FormMarket2_Load (object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
                //List<string> countryCodes = new List<string> { "GB", "IE", "FR", "AU", "US", "RSA", "GER", "UAE" };
			    treeViewEvents.BeginUpdate ();
				treeViewEvents.Nodes.Clear ();
				TreeNode rootNode = treeViewEvents.Nodes.Add ("Events");
				rootNode.Tag = new RootNodeTag ();
			    int orderIndex = 0;
			    foreach (var country in countriesTable.Values)
			    {
			        TreeNode countryNode = new TreeNode(country)
			            {
			                Tag = new CountryNodeTag(country)
			            };
			        rootNode.Nodes.Add(countryNode);
			        orderIndex++;
			    }

                //foreach (var bfEvent in info)
                //{
                //    Console.WriteLine(bfEvent.Event.Name);
                //    if(!string.IsNullOrEmpty(bfEvent.Event.Venue))
                //        Console.WriteLine(bfEvent.Event.Venue);

                //    if (countryCodes.Contains (bfEvent.Event.Name))
                //    {
                //        TreeNode eventNode = new TreeNode (bfEvent.Event.Name)
                //            {
                //                Tag = new CountryNodeTag(bfEvent.Event.Id, orderIndex)
                //            };
                //        rootNode.Nodes.Add (eventNode);
                //        //Console.WriteLine (bfEvent.eventName + " " + bfEvent.startTime.ToString () + " " + bfEvent.timezone);
                //        orderIndex++;
                //    }
                //}

/*
				foreach (BetfairG.MarketSummary marketSummary in info.MarketsSummaries)
				{
					if (countries.Contains (marketSummary.marketName))
					{
						TreeNode marketNode = new TreeNode (MarketNodeTag.GetName (marketSummary));
						marketNode.Tag = new MarketNodeTag (marketSummary.marketId, marketSummary.exchangeId, marketSummary.orderIndex);
						rootNode.Nodes.Add (marketNode);
						Console.WriteLine (marketSummary.marketName + " " + marketSummary.startTime.ToString () + " " + marketSummary.timezone);
					}
				}
*/

				treeViewEvents.SelectedNode = null;
				treeViewEvents.ExpandAll ();
				treeViewEvents.EndUpdate ();
			}
			catch (Exception ex)
			{
				MessageBox.Show (ex.ToString (),
					"Error loading markets", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

        //private bool IsContainNumber (string name)
        //{
        //    if(string.IsNullOrEmpty(name))
        //        return false;
        //    return name.Any(Char.IsDigit);
        //}

        //private void OpenMarket(int marketId, int exchangeId, DateTime startTime)
        //{
        //    try
        //    {
        //        MarketTabPage mtp = new MarketTabPage(marketId, exchangeId, startTime);
        //        //mtp.OnDisplayWallet += new MethodInvoker (mtp_OnDisplayWallet);
        //        mtp.FillMarketPrices();
        //        tabControlMarkets.TabPages.Add(mtp);
        //        //tabControlMarkets.SelectTab (mtp);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}

        private void SetCountryEventsNodes()
        {
            // check for one root node
            if(treeViewEvents.Nodes.Count!=1)
                return;

            var rootNode = treeViewEvents.Nodes[0];

            var mf = new MarketFilter()
            {
                EventTypeIds = new HashSet<string>() { MarketId.ToString() },
                MarketTypeCodes = new HashSet<String>() { "WIN" }
                //MarketIds = new HashSet<string>() { MarketId.ToString() },
                //MarketCountries = new HashSet<string>(countriesTable.Keys)
            };
            events = Globals.Exchange.ListEvents(Globals.BETFAIR_EXCHANGE_UK, mf);
            events.Sort((x, y) => x.Event.OpenDate.Value.CompareTo(y.Event.OpenDate.Value));

            foreach (var eventResult in events)
            {
                //Console.WriteLine(eventResult.Event);
                if (!countriesTable.Keys.Contains(eventResult.Event.CountryCode))
                    continue; // skip country not in table

                var countryName = countriesTable[eventResult.Event.CountryCode];
                var countryNodes = from TreeNode tn in rootNode.Nodes
                         where tn.Text == countryName
                         select tn;
                var countryNodeArray = countryNodes as TreeNode[] ?? countryNodes.ToArray();
                if (countryNodeArray.Count()==1)
                {
                    countryNodeArray.First().Nodes.Add(new TreeNode(eventResult.Event.Name)
                        {
                            Tag = new EventMarket2NodeTag(eventResult.Event.Id, eventResult.Event.Name)
                        });
                }
            }

            //var marketProjections = new HashSet<MarketProjectionEnum>
            //    {
            //        MarketProjectionEnum.COMPETITION,
            //        MarketProjectionEnum.EVENT,
            //        MarketProjectionEnum.EVENT_TYPE
            //        //MarketProjectionEnum.RUNNER_METADATA
            //    };

            //var markets = Globals.Exchange.ListMarketCatalogue(Globals.BetfairExchangeUk, mf, marketProjections,
            //                                                   MarketSort.FIRST_TO_START, 100);
            //Console.WriteLine("MC");
            //foreach (var marketCatalogue in markets)
            //{
            //    Console.WriteLine(marketCatalogue);
            //}

            // remove empty country node(s)
            //var emptyNodes = from TreeNode tn in rootNode.Nodes
            //                 where tn.Nodes.Count == 0
            //                 select tn;
            //foreach (var emptyNode in emptyNodes.ToArray())
            //{
            //    rootNode.Nodes.Remove(emptyNode);
            //}
        }

		private void treeViewEvents_AfterSelect (object sender, TreeViewEventArgs e)
		{
			try
			{
				Cursor = Cursors.WaitCursor;
				// close node
				if (e.Action == TreeViewAction.Unknown)
					return;

			    if (events == null)
			    {
                    SetCountryEventsNodes();
			    }

			    treeViewEvents.BeginUpdate ();
				INodeTag nodeTag = e.Node.Tag as INodeTag;
				//Console.WriteLine (nodeTag.Type.ToString ());
				switch (nodeTag.Type)
				{
					case NodeType.Root:
						break;

					case NodeType.Country:
						e.Node.ExpandAll ();
						break;

					case NodeType.EventMarket2:
						var em2nt = nodeTag as EventMarket2NodeTag;
				        var mf = new MarketFilter()
				            {
				                EventIds = new HashSet<string>() {em2nt.Id},
                                MarketTypeCodes = new HashSet<String>() { "WIN" }
				            };
				        var mp = new HashSet<MarketProjectionEnum>
				            {
				                MarketProjectionEnum.MARKET_START_TIME
				            };

				        var eventResults = Globals.Exchange.ListMarketCatalogue
				            (Globals.BETFAIR_EXCHANGE_UK, mf, mp, MarketSortEnum.FIRST_TO_START, Globals.MAX_MARKET);

				        foreach (var marketCatalogue in eventResults)
				        {
				            if (!marketCatalogue.MarketName.ToUpper().Contains("TO BE PLACED"))
				            {
				                var marketName = Globals.GetMarketLabel(marketCatalogue.MarketStartTime, marketCatalogue.MarketName);
				                var marketNode = new TreeNode(marketName)
				                    {
				                        Tag = new MarketNodeTag
				                            (marketCatalogue.MarketId,
				                             marketName,
				                             marketCatalogue.MarketStartTime)
				                    };
				                e.Node.Nodes.Add(marketNode);
				                var toStart = marketCatalogue.MarketStartTime - DateTime.Now.ToUniversalTime();
				                if (toStart.TotalMinutes >= 0)
				                    OpenMarket(marketCatalogue.MarketId, marketCatalogue.MarketStartTime, marketName);
				            }
				        }

						e.Node.ExpandAll ();
						break;

					case NodeType.Market:
						/*
						MarketNodeTag mnt = nodeTag as MarketNodeTag;
						if (OnMarketSelected != null)
							OnMarketSelected (mnt.Id, mnt.ExchangeId);*/
						//this.Close ();
						break;

					case NodeType.Selection:
						//SelectionNodeTag snt = nodeTag as SelectionNodeTag;

						/*
											foreach (BetfairE.RunnerPrices runnerPrices in marketPrices.runnerPrices)
											{
												System.Diagnostics.Debug.WriteLine ("{0} {1}", runnerPrices.sortOrder, runnerPrices.lastPriceMatched);
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
				//buttonStartStop.Enabled = runnersControl1.IsSet;
				//treeViewEvents.TreeViewNodeSorter = null;
				//treeViewEvents.Sorted = false;
				treeViewEvents.EndUpdate ();
				Cursor = Cursors.Default;
			}
		}

        private void OpenMarket(string marketId, DateTime startTime, string marketName)
	    {
	        try
	        {
	            MarketTabPage mtp = new MarketTabPage(marketId, startTime, marketName);
	            mtp.FillMarketPrices();
	            tabControlMarkets.TabPages.Add(mtp);
	        }
	        catch (Exception ex)
	        {
	            MessageBox.Show(ex.ToString(), "OpenMarket", MessageBoxButtons.OK, MessageBoxIcon.Error);
	        }
	    }
	}
}
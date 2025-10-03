using System;
using System.Linq;
using System.Windows.Forms;
using BetfairNgClient.Json;

namespace Autobook
{
    public partial class FormMarkets : Form
    {
        //private EventsMenuResponse eventsMenuResponse;

        public delegate void OnMarketSelectedDelegate(string marketId, DateTime startTime, string marketName);

        public event OnMarketSelectedDelegate OnMarketSelected;

        public FormMarkets()
        {
            InitializeComponent();
        }

        private void FormMarkets_Load(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                treeViewEvents.BeginUpdate();
                treeViewEvents.Nodes.Clear();
                //treeViewEvents.TreeViewNodeSorter = new NodeSorterAlphabetical();
                TreeNode rootNode = treeViewEvents.Nodes.Add("Events");
                rootNode.Tag = new RootNodeTag();

                var eventsMenuResponse = Globals.Exchange.GetEventsMenu();
                foreach (var em in eventsMenuResponse.Children)
                {
                    if (em.Type == "EVENT_TYPE")
                    {
                        TreeNode eventNode = new TreeNode(em.Name)
                            {
                                Tag = new EventTypeNodeTag(em)
                            };
                        rootNode.Nodes.Add(eventNode);
                    }
                    else
                    {
                        Console.WriteLine("Unexpected event type: " + em.Type);
                    }
                }

                //var eventTypes = Globals.Exchange.ListEventTypes(Globals.BetfairExchangeUk, new MarketFilter());
                //eventTypes.Sort((x, y) => x.EventType.Name.CompareTo(y.EventType.Name));
                //foreach (var eventType in eventTypes)
                //{
                //    TreeNode eventNode = new TreeNode(eventType.EventType.Name)
                //        {
                //            Tag = new EventNodeTag(eventType.EventType.Id, eventType.EventType.Name)
                //        };
                //    //Console.WriteLine (eventType.name + " " + eventType.id);
                //    rootNode.Nodes.Add(eventNode);
                //}
                treeViewEvents.SelectedNode = null;
                treeViewEvents.ExpandAll();
                treeViewEvents.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),
                                "Error loading markets", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void treeViewEvents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                // close node
                if (e.Action == TreeViewAction.Unknown)
                    return;
                if (e.Node.IsExpanded)
                {
                    return;
                    //e.Node.Collapse(true);
                }
                treeViewEvents.BeginUpdate();
                INodeTag nodeTag = e.Node.Tag as INodeTag;
                switch (nodeTag.Type)
                {
                    case NodeType.Root:
                        break;

                        case NodeType.EventType:
                        var etnt = nodeTag as EventTypeNodeTag;
                        SetParentNode(e, etnt.EventsMenuResponse);
                        break;

                        case NodeType.Group:
                        var gnt = nodeTag as GroupNodeTag;
                        SetParentNode(e, gnt.EventsMenuResponse);
                        break;

                        case NodeType.Race:
                        var rnt = nodeTag as RaceNodeTag;
                        SetParentNode(e, rnt.EventsMenuResponse);
                        break;

                        case NodeType.Event:
                        var ent = nodeTag as EventNodeTag;
                        SetParentNode(e, ent.EventsMenuResponse);
                        break;
                        /*
                    case NodeType.Event:
                        EventNodeTag ent = nodeTag as EventNodeTag;
                        //comboBoxEvent.Text = e.Node.Text;
                        var mf = new MarketFilter
                            {
                                EventTypeIds = new HashSet<string>() {ent.Id.ToString()},
                                //MarketTypeCodes = new HashSet<String> {"WIN"}
                            };
                        var info = Globals.Exchange.ListEvents (Globals.BetfairExchangeUk, mf);
                        info.Sort((x,y) => x.Event.OpenDate.Value.CompareTo(x.Event.OpenDate.Value));

                        foreach (var bfEvent in info)
                        {
                            TreeNode eventNode = new TreeNode(bfEvent.Event.Name)
                                {
                                    Tag = new EventNodeTag(bfEvent.Event.Id, bfEvent.Event.Name)
                                };
                            int index = e.Node.Nodes.Add(eventNode);
                            if (bfEvent.MarketCount > 0)
                            {
                                mf = new MarketFilter {EventIds = new HashSet<string> {bfEvent.Event.Id}};
                                var mp = new HashSet<MarketProjectionEnum>
                                    {
                                        MarketProjectionEnum.MARKET_START_TIME
                                    };

                                var markets = Globals.Exchange.ListMarketCatalogue
                                    (Globals.BetfairExchangeUk, mf, mp, MarketSort.FIRST_TO_START, bfEvent.MarketCount);

                                //markets.Sort((x, y) => x.MarketStartTime.CompareTo(y.MarketStartTime));
                                foreach (var marketCatalogue in markets)
                                {
                                    TreeNode marketNode = new TreeNode(Globals.GetMarketLabel(marketCatalogue))
                                        {
                                            Tag = new MarketNodeTag
                                                (marketCatalogue.MarketId,
                                                 marketCatalogue.MarketName,
                                                 marketCatalogue.MarketStartTime)
                                        };
                                    e.Node.Nodes[index].Nodes.Add(marketNode);

                                }
                            }
                        }
                        //foreach (BetfairG.MarketSummary marketSummary in info.MarketsSummaries)
                        //{
                        //    TreeNode marketNode = new TreeNode(MarketNodeTag.GetName(marketSummary));
                        //    marketNode.Tag = new MarketNodeTag
                        //        (marketSummary.marketId, marketSummary.exchangeId, marketSummary.orderIndex,
                        //         marketSummary.startTime);
                        //    e.Node.Nodes.Add(marketNode);
                        //}
                        e.Node.ExpandAll();
                        break;*/

                    case NodeType.Market:
                        var mnt = nodeTag as MarketNodeTag;
                        if (OnMarketSelected != null)
                            OnMarketSelected(mnt.Id, mnt.StartTime, mnt.Name);
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
                MessageBox.Show(ex.ToString(), "Select Node", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //buttonStartStop.Enabled = runnersControl1.IsSet;
                //treeViewEvents.TreeViewNodeSorter = null;
                //treeViewEvents.Sorted = false;
                treeViewEvents.EndUpdate();
                Cursor = Cursors.Default;
            }
        }

        private bool IsParentEventTypeHorseGreyhound(TreeNode treeNode)
        {
            TreeNode currentNode = treeNode;

            while (currentNode.Parent!=null)
            {
                INodeTag parentTag = currentNode.Parent.Tag as INodeTag;
                if (parentTag.Type == NodeType.EventType)
                    return parentTag.Name == "Horse Racing" ||
                        parentTag.Name == "Greyhound Racing";

                currentNode = currentNode.Parent;
            }

            return false;
        }

        private void SetParentNode(TreeViewEventArgs treeViewEventArgs, EventsMenuResponse eventsMenuResponse)
        {
            if (eventsMenuResponse.Children != null &&
                eventsMenuResponse.Children.Any())
            {
                foreach (var em in eventsMenuResponse.Children)
                {

                    if (em.Name.ToUpper().Contains("TO BE PLACED"))
                        continue;

                    INodeTag tag;

                    switch (em.Type)
                    {
                        case "GROUP":
                            tag = new GroupNodeTag(em);
                            break;

                        case "EVENT":
                            tag = new EventNodeTag(em);
                            break;

                        case "RACE":
                            tag = new RaceNodeTag(em);
                            break;

                        case "MARKET":
                            tag = new MarketNodeTag(em);
                            if (IsParentEventTypeHorseGreyhound(treeViewEventArgs.Node))
                                ((MarketNodeTag) tag).Name = Globals.GetMarketLabel(em.MarketStartTime, em.Name);
                            break;

                        default:
                            Console.WriteLine("Unexpected event type: " + em.Type);
                            return;
                    }
                    TreeNode eventNode = new TreeNode(tag.Name)
                    {
                        Tag = tag
                    };
                    treeViewEventArgs.Node.Nodes.Add(eventNode);
                    treeViewEventArgs.Node.ExpandAll();
                }
            }
            else
            {
                if (OnMarketSelected != null)
                    OnMarketSelected(eventsMenuResponse.Id, DateTime.Now, eventsMenuResponse.Name);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
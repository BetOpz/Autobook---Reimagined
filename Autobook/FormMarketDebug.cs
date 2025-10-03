using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using BetfairNgClient.Json;
using PthLog;
using Timer = System.Threading.Timer;

namespace Autobook
{
    public partial class FormMarketDebug : Form
    {
        private readonly MarketTabPage marketTabPage;
        private readonly Timer refreshTimer;
        private bool isClosing;
        //private AutoResetEvent

        public event EventHandler OnDebugClosed;

        public FormMarketDebug(MarketTabPage marketTabPage)
        {
            this.marketTabPage = marketTabPage;

            InitializeComponent();

            Text = this.marketTabPage.Text;
            comboBoxRefreshRate.SelectedIndex = 3; // 1 sec
            refreshTimer = new Timer(RefreshTimerCallback);
        }

        #region GUI
        private void FormMarketDebug_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isClosing)
                InternalCloseWindow();
        }

        private void InternalCloseWindow()
        {
            isClosing = true;

            if (refreshTimer != null)
                refreshTimer.Dispose();

            if (OnDebugClosed != null)
                OnDebugClosed(this, null);

            Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            InternalCloseWindow();
        }


        private void comboBoxRefreshRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            // don't update the timer if it is not running
            if(buttonStartRefresh.Enabled)
                return;

            if (refreshTimer != null)
                refreshTimer.Change(0, GetRefreshRateAsInt());
        }

        private void buttonStartRefresh_Click(object sender, EventArgs e)
        {
            buttonRefresh.Enabled = false;
            buttonStartRefresh.Enabled = false;
            refreshTimer.Change(0, GetRefreshRateAsInt());
            buttonStopRefresh.Enabled = true;
        }

        private void buttonStopRefresh_Click(object sender, EventArgs e)
        {
            buttonStopRefresh.Enabled = false;
            refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
            buttonStartRefresh.Enabled = true;
            buttonRefresh.Enabled = true;
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshOutput();
        }
        #endregion GUI

        #region Fields Info
        private IEnumerable<FieldInfo> GetFieldsInfo()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            return marketTabPage.GetType().GetFields(flags);
        }

        #endregion Fields Info

        #region Refresh Output
		
        private void RefreshOutput()
        {
            Logger logger = null;

            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(RefreshOutput));
                    return;
                }

                Dictionary<int, double> smallLayAmounts = null;
                Dictionary<int, double> smallBackAmounts = null;
                List<MarketProfitAndLoss> pandls = null;

                string marketId = string.Empty;
                int exchangeId = 0;

                double dynamicLiability = 0.0d;
                double usedLiability = 0.0d;

                string marketName = string.Empty;

                bool isInPlay = false;
                bool isPrProfitReached = false;
                bool isIpProfitReached = false;
                bool isStarted = false;

                DateTime startTime = DateTime.MinValue;

                MarketStatusEnum marketStatus = MarketStatusEnum.INACTIVE;

                var fieldsInfo = GetFieldsInfo();
                foreach (var fieldInfo in fieldsInfo)
                {
                    switch (fieldInfo.Name)
                    {
                        case "_logger":
                            logger = fieldInfo.GetValue(marketTabPage) as Logger;
                            break;

                        case "_smallLayAmounts":
                            smallLayAmounts = new Dictionary<int, double>(fieldInfo.GetValue(marketTabPage) as Dictionary<int, double>);
                            break;

                        case "_smallBackAmounts":
                            smallBackAmounts = new Dictionary<int, double>(fieldInfo.GetValue(marketTabPage) as Dictionary<int, double>);
                            break;

                        case "_pandls":
                            pandls = new List<MarketProfitAndLoss>(fieldInfo.GetValue(marketTabPage) as List<MarketProfitAndLoss>);
                            break;

                        case "dynamicLiability":
                            dynamicLiability = (double)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "exchangeId":
                            exchangeId = (int)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "_isInPlay":
                            isInPlay = (bool)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "isIPProfitReached":
                            isIpProfitReached = (bool)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "isPRProfitReached":
                            isPrProfitReached = (bool)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "isStarted":
                            isStarted = (bool)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "marketId":
                            marketId = (string)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "marketName":
                            marketName = (string)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "_marketStatus":
                            marketStatus = (MarketStatusEnum)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "startTime":
                            startTime = (DateTime)fieldInfo.GetValue(marketTabPage);
                            break;

                        case "usedLiability":
                            usedLiability = (double)fieldInfo.GetValue(marketTabPage);
                            break;

                            // ignored
                        case "market":
                            break;

                        default:
                            throw new Exception("FormMarketDebug: not handled field " + fieldInfo.Name);
                    }
                    //    Console.WriteLine("{0}:{4} {1} {2} {3}",fieldInfo.Name, fieldInfo.FieldType.IsClass,
                    //        fieldInfo.FieldType.IsValueType, fieldInfo.FieldType.IsPrimitive, fieldInfo.FieldType.ToString());
                }

                //var filteredFields = fieldsInfo.OrderBy(f => f.Name);
                //var filteredFields = fieldsInfo.Where(f => (!f.FieldType.IsClass) && (f.FieldType.IsValueType || f.FieldType.IsPrimitive)).OrderBy(f => f.Name);

                #region Internals
                listViewInternals.BeginUpdate();
                listViewInternals.Items.Clear();

                listViewInternals.Items.Add(new ListViewItem(new [] { "dynamicLiability", dynamicLiability.ToString(Globals.DOUBLE_FORMAT) }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "exchangeId", exchangeId.ToString() }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "isInPlay", isInPlay.ToString() }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "isIPProfitReached", isIpProfitReached.ToString() }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "isPRProfitReached", isPrProfitReached.ToString() }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "isStarted", isStarted.ToString() }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "marketId", marketId.ToString() }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "marketName", marketName }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "marketStatus", marketStatus.ToString() }));
                listViewInternals.Items.Add(new ListViewItem(new [] { "startTime", startTime.ToLongDateString() }));
                listViewInternals.Items.Add(new ListViewItem(new[] { "usedLiability", usedLiability.ToString(Globals.DOUBLE_FORMAT) }));

                //foreach (var ff in filteredFields)
                //{
                //    listViewInternals.Items.Add(new ListViewItem(new string[] { ff.Name, ff.GetValue(_marketTabPage).ToString() }));
                //}

                listViewInternals.EndUpdate();
                #endregion Internals

                #region Small Lays

                listViewSmallLays.BeginUpdate();
                listViewSmallLays.Items.Clear();

                if (smallLayAmounts != null)
                {
                    foreach (KeyValuePair<int, double> kvp in smallLayAmounts)
                    {
                        listViewSmallLays.Items.Add(new ListViewItem(new[]
                        {
                           GetSelectionName(pandls, kvp.Key), kvp.Value.ToString(Globals.DOUBLE_FORMAT)
                        }));
                    }
                }

                listViewSmallLays.EndUpdate();

                #endregion Small Lays

                #region Small Back

                listViewSmallBacks.BeginUpdate();
                listViewSmallBacks.Items.Clear();

                if (smallBackAmounts != null)
                {
                    foreach (KeyValuePair<int, double> kvp in smallBackAmounts)
                    {
                        listViewSmallBacks.Items.Add(new ListViewItem(new[]
                        {
                           GetSelectionName(pandls, kvp.Key), kvp.Value.ToString(Globals.DOUBLE_FORMAT)
                        }));
                    }
                }
                listViewSmallBacks.EndUpdate();

                #endregion Small back

                #region Profits & Loss

                //listViewPAndL.BeginUpdate();
                //listViewPAndL.Items.Clear();
                //if (pandls != null)
                //{
                //    foreach (KeyValuePair<int, MarketProfitAndLoss> kvp in pandls)
                //    {
                //        listViewPAndL.Items.Add(
                //            new ListViewItem(new[] { kvp.Value.selectionName, kvp.Value.ifWin.ToString(Globals.DoubleFormat) }));
                //    }
                //}
                //listViewPAndL.EndUpdate();

                #endregion Profits & Loss
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Error("Debug window: exception catched: " + ex);
            }
        }

        private void RefreshTimerCallback(object state)
        {
            RefreshOutput();
        }

        private int GetRefreshRateAsInt()
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

            return interval;
        }

        private string GetSelectionName(List<MarketProfitAndLoss> pandls, int selectionId)
        {
            string result;

            // TODO check exec here
            //if ((pandls != null) && (pandls.Contains(selectionId)))
            //    result = pandls[selectionId].selectionName;
            //else
                result = selectionId.ToString();

            return result;
        }
        #endregion Refresh Output
    }
}

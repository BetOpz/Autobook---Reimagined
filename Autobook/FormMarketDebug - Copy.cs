using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace Autobook
{
    public partial class FormMarketDebug : Form
    {
        private readonly MarketTabPage _marketTabPage;
        private readonly Timer _refreshTimer;
        private bool _isClosing;
        //private AutoResetEvent

        public event EventHandler OnDebugClosed;

        public FormMarketDebug(MarketTabPage marketTabPage)
        {
            _marketTabPage = marketTabPage;

            InitializeComponent();

            Text = _marketTabPage.Text;
            comboBoxRefreshRate.SelectedIndex = 3; // 1 sec
            _refreshTimer = new Timer(RefreshTimerCallback);
        }

        #region GUI
        private void FormMarketDebug_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_isClosing)
                InternalCloseWindow();
        }

        private void InternalCloseWindow()
        {
            _isClosing = true;

            if (_refreshTimer != null)
                _refreshTimer.Dispose();

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
            if (_refreshTimer != null)
                _refreshTimer.Change(0, GetRefreshRateAsInt());
        }

        private void buttonStartRefresh_Click(object sender, EventArgs e)
        {
            buttonRefresh.Enabled = false;
            buttonStartRefresh.Enabled = false;
            _refreshTimer.Change(0, GetRefreshRateAsInt());
            buttonStopRefresh.Enabled = true;
        }

        private void buttonStopRefresh_Click(object sender, EventArgs e)
        {
            buttonStopRefresh.Enabled = false;
            _refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
            buttonStartRefresh.Enabled = true;
            buttonRefresh.Enabled = true;
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshOutput();
        }
        #endregion GUI

        #region Fields Info
        private FieldInfo[] GetFieldsInfo()
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            return _marketTabPage.GetType().GetFields(flags);
        }
        #endregion Fields Info

        #region Refresh Output
		
        private void RefreshOutput()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(RefreshOutput));
                return;
            }

            Dictionary<int, double> smallLayAmounts = null;
            Dictionary<int, double> smallBackAmounts = null;
            Dictionary<int, BetfairClient.Framework.com.betfair.api6.exchange.ProfitAndLoss> pandls = null;

            int marketId = 0;
            int exchangeId = 0;

            double dynamicLiability = 0.0d;
            double usedLiability = 0.0d;

            string marketName = string.Empty;

            bool isInPlay = false;
            bool isPRProfitReached = false;
            bool isIPProfitReached = false;
            bool isStarted = false;

            DateTime startTime = DateTime.MinValue;

            BetfairClient.Framework.com.betfair.api6.exchange.MarketStatusEnum marketStatus =
                BetfairClient.Framework.com.betfair.api6.exchange.MarketStatusEnum.INACTIVE;

            var fieldsInfo = GetFieldsInfo();
            foreach (var fieldInfo in fieldsInfo)
            {
                switch (fieldInfo.Name)
                {
                    case "_smallLayAmounts":
                        smallLayAmounts = fieldInfo.GetValue(_marketTabPage) as Dictionary<int, double>;
                        break;

                    case "_smallBackAmounts":
                        smallBackAmounts = fieldInfo.GetValue(_marketTabPage) as Dictionary<int, double>;
                        break;

                    case "_pandls":
                        pandls = fieldInfo.GetValue(_marketTabPage) as
                            Dictionary<int, BetfairClient.Framework.com.betfair.api6.exchange.ProfitAndLoss>;
                        break;

                    case "dynamicLiability":
                        dynamicLiability = (double)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "exchangeId":
                        exchangeId = (int)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "_isInPlay":
                        isInPlay = (bool)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "isIPProfitReached":
                        isIPProfitReached = (bool)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "isPRProfitReached":
                        isPRProfitReached = (bool)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "isStarted":
                        isStarted = (bool)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "marketId":
                        marketId = (int)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "marketName":
                        marketName = (string)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "_marketStatus":
                        marketStatus = (BetfairClient.Framework.com.betfair.api6.exchange.MarketStatusEnum)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "startTime":
                        startTime = (DateTime)fieldInfo.GetValue(_marketTabPage);
                        break;

                    case "usedLiability":
                        usedLiability = (double)fieldInfo.GetValue(_marketTabPage);
                        break;
                }
                //    Console.WriteLine("{0}:{4} {1} {2} {3}",fieldInfo.Name, fieldInfo.FieldType.IsClass,
                //        fieldInfo.FieldType.IsValueType, fieldInfo.FieldType.IsPrimitive, fieldInfo.FieldType.ToString());
            }

            //var filteredFields = fieldsInfo.OrderBy(f => f.Name);
            //var filteredFields = fieldsInfo.Where(f => (!f.FieldType.IsClass) && (f.FieldType.IsValueType || f.FieldType.IsPrimitive)).OrderBy(f => f.Name);

            #region Internals
            listViewInternals.BeginUpdate();
            listViewInternals.Items.Clear();

            listViewInternals.Items.Add(new ListViewItem(new string[] { "dynamicLiability", dynamicLiability.ToString(Globals.DoubleFormat) }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "exchangeId", exchangeId.ToString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "isInPlay", isInPlay.ToString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "isIPProfitReached", isIPProfitReached.ToString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "isPRProfitReached", isPRProfitReached.ToString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "isStarted", isStarted.ToString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "marketId", marketId.ToString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "marketName", marketName }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "marketStatus", marketStatus.ToString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "startTime", startTime.ToLongDateString() }));
            listViewInternals.Items.Add(new ListViewItem(new string[] { "usedLiability", usedLiability.ToString(Globals.DoubleFormat) }));

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
                    listViewInternals.Items.Add(
                        new ListViewItem(new string[] { kvp.Key.ToString(), kvp.Value.ToString(Globals.DoubleFormat) }));
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
                    listViewSmallBacks.Items.Add(
                        new ListViewItem(new string[] { kvp.Key.ToString(), kvp.Value.ToString(Globals.DoubleFormat) }));
                }
            }
            listViewSmallBacks.EndUpdate();

            #endregion Small back

            #region Profits & Loss

            listViewPAndL.BeginUpdate();
            listViewPAndL.Items.Clear();
            if (pandls != null)
            {
                foreach (KeyValuePair<int, BetfairClient.Framework.com.betfair.api6.exchange.ProfitAndLoss> kvp in pandls)
                {
                    listViewPAndL.Items.Add(
                        new ListViewItem(new string[] { kvp.Value.selectionName, kvp.Value.ifWin.ToString(Globals.DoubleFormat) }));
                }
            }
            listViewPAndL.EndUpdate();

            #endregion Profits & Loss
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
 
        #endregion Refresh Output
    }
}

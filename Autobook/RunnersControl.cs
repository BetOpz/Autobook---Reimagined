using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

#if TEST
using BetfairE = BetfairClient.Framework.Test.com.betfair.api6.exchange;
#else
using BetfairE = BetfairClient.Framework.com.betfair.api6.exchange;
#endif
using BetfairClient.Framework;
using System.Reflection;

namespace Autobook
{
    public partial class RunnersControl : UserControl
    {
        //private BetfairE.RunnerPrices[] runnerPrices;
        private BetfairE.Market market = null;
        private BetfairE.MarketPrices marketPrices;
        private int exchangeId;
        private int marketId;
        public event PlaceBetDelegate OnPlaceBet;
        public delegate void PlaceBetDelegate(BetfairE.RunnerPrices[] runnerPrices, BetType type);
        private bool isSet;

        #region Properties
        public bool IsSet
        {
            get { return isSet; }
        }

        public void SetMarket(int exchangeId, int marketId)
        {
            this.exchangeId = exchangeId;
            this.marketId = marketId;
						market = Globals.Exchange.GetMarket (exchangeId, marketId);
						Globals.CheckNetworkStatusIsConnected ();
						isSet = true;
        }

        /*
        public int ExchangeId
        {
          get { return exchangeId; }
          set { exchangeId = value; }
        }

        public int MarketId
        {
          get { return marketId; }
          set { marketId = value; }
        }
            public BetfairE.MarketPrices MarketPrices
            {
              get { return marketPrices; }
              set { marketPrices = value; }
            }

            public BetfairE.RunnerPrices[] RunnerPrices
            {
              get { return runnerPrices; }
              set
              {
                runnerPrices = value;
              }
            }

            public BetfairE.Market Market
            {
              get { return market; }
              set { market = value; }
            }
        */
        #endregion

        public RunnersControl()
        {
            InitializeComponent();
            PropertyInfo aProp = typeof(ListView).GetProperty
              ("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            aProp.SetValue(listViewRunners, true, null);
        }

        public void FillMarketPrices()
        {
            listViewRunners.BeginUpdate();
            try
            {
                //market = Globals.exchange.GetMarket (exchangeId, marketId);
							marketPrices = Globals.Exchange.GetMarketPrices (exchangeId, marketId);
							Globals.CheckNetworkStatusIsConnected ();

                listViewRunners.Items.Clear();
                foreach (BetfairE.RunnerPrices runnerPrice in marketPrices.runnerPrices)
                {
                    AddRunnerPrices(Exchange.FindRunner(market, runnerPrice.selectionId), runnerPrice);
                }
                AddBookPercentages(marketPrices.runnerPrices);
                string info = string.Empty;
                if (market == null)
                {
                    info = "; No names; exceeded API throttle";
                }
                columnHeaderSelection.Text = string.Format("Selections: {0}{1}", marketPrices.runnerPrices.Length, info);
                ResizeColumns();
            }
            finally
            {
                listViewRunners.EndUpdate();
            }
        }

        private void AddBookPercentages(BetfairE.RunnerPrices[] runnerPrices)
        {
            double layBook = 0.0d;
            double backBook = 0.0d;
            bool isBackValid = true;

            foreach (BetfairE.RunnerPrices rp in runnerPrices)
            {
                if (rp.bestPricesToBack.Length > 0)
                    backBook += Globals.GetBookShare(rp.bestPricesToBack[0].price);
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("back invalid: {0}: {1}", DateTime.Now, backBook.ToString("###.0")));
                    isBackValid = false;
                }
                if (rp.bestPricesToLay.Length > 0)
                    layBook += Globals.GetBookShare(rp.bestPricesToLay[0].price);
            }
            backBook *= 100.0d;
#if SIMULATION
			//backBook *= 0.95d;
#endif
            if (isBackValid)
                if (backBook < 100.0d)
                    if (OnPlaceBet != null)
                        OnPlaceBet.BeginInvoke(runnerPrices, BetType.Back, null, null);
            layBook *= 100.0d;
            if (layBook > 100.0d)
                if (OnPlaceBet != null)
                    OnPlaceBet.BeginInvoke(runnerPrices, BetType.Lay, null, null);
            DisplayBook(backBook, layBook, isBackValid);
        }

        private void DisplayBook(double backBook, double layBook, bool isBackValid)
        {
            Font bookFont = new Font(listViewRunners.Font, FontStyle.Regular);

            ListViewItem itemBook = listViewRunners.Items.Add(new ListViewItem(""));
            itemBook.UseItemStyleForSubItems = false;
            itemBook.SubItems.Add("");
            itemBook.SubItems.Add("");
            if (isBackValid)
                itemBook.SubItems.Add(backBook.ToString("###.0"), listViewRunners.ForeColor, Globals.BACK_COLOUR, bookFont);
            else
                itemBook.SubItems.Add("NA", listViewRunners.ForeColor, Globals.BACK_COLOUR, bookFont);
            itemBook.SubItems.Add(layBook.ToString("###.0"), listViewRunners.ForeColor, Globals.LAY_COLOUR, bookFont);
        }

        private void AddRunnerPrices(BetfairE.Runner runner, BetfairE.RunnerPrices runnerPrices)
        {
            Font selectionFont = new Font(listViewRunners.Font, FontStyle.Bold);
            Font priceFont = new Font(listViewRunners.Font, FontStyle.Bold);
            Font sizeFont = new Font(listViewRunners.Font, FontStyle.Regular);

            string selectionName = (runner == null) ? runnerPrices.selectionId.ToString() : runner.name;
            ListViewItem itemPrice = listViewRunners.Items.Add(new ListViewItem(selectionName));
            itemPrice.Font = selectionFont;
            itemPrice.UseItemStyleForSubItems = false;
            itemPrice.Tag = runnerPrices;

            ListViewItem itemSize = listViewRunners.Items.Add(new ListViewItem(""));
            itemSize.UseItemStyleForSubItems = false;
            itemSize.Tag = null;

            string price = (runnerPrices.bestPricesToBack.Length > 2) ? runnerPrices.bestPricesToBack[2].price.ToString() : "";
            string size = (runnerPrices.bestPricesToBack.Length > 2) ? FormatSize(runnerPrices.bestPricesToBack[2].amountAvailable) : "";
            itemPrice.SubItems.Add(price, listViewRunners.ForeColor, listViewRunners.BackColor, priceFont);
            itemSize.SubItems.Add(size, listViewRunners.ForeColor, listViewRunners.BackColor, sizeFont);

            price = (runnerPrices.bestPricesToBack.Length > 1) ? runnerPrices.bestPricesToBack[1].price.ToString() : "";
            size = (runnerPrices.bestPricesToBack.Length > 1) ? FormatSize(runnerPrices.bestPricesToBack[1].amountAvailable) : "";
            itemPrice.SubItems.Add(price, listViewRunners.ForeColor, listViewRunners.BackColor, priceFont);
            itemSize.SubItems.Add(size, listViewRunners.ForeColor, listViewRunners.BackColor, sizeFont);

            price = (runnerPrices.bestPricesToBack.Length > 0) ? runnerPrices.bestPricesToBack[0].price.ToString() : "";
            size = (runnerPrices.bestPricesToBack.Length > 0) ? FormatSize(runnerPrices.bestPricesToBack[0].amountAvailable) : "";
            itemPrice.SubItems.Add(price, listViewRunners.ForeColor, Globals.BACK_COLOUR, priceFont);
            itemSize.SubItems.Add(size, listViewRunners.ForeColor, Globals.BACK_COLOUR, sizeFont);

            price = (runnerPrices.bestPricesToLay.Length > 0) ? runnerPrices.bestPricesToLay[0].price.ToString() : "";
            size = (runnerPrices.bestPricesToLay.Length > 0) ? FormatSize(runnerPrices.bestPricesToLay[0].amountAvailable) : "";
            itemPrice.SubItems.Add(price, listViewRunners.ForeColor, Globals.LAY_COLOUR, priceFont);
            itemSize.SubItems.Add(size, listViewRunners.ForeColor, Globals.LAY_COLOUR, sizeFont);

            price = (runnerPrices.bestPricesToLay.Length > 1) ? runnerPrices.bestPricesToLay[1].price.ToString() : "";
            size = (runnerPrices.bestPricesToLay.Length > 1) ? FormatSize(runnerPrices.bestPricesToLay[1].amountAvailable) : "";
            itemPrice.SubItems.Add(price, listViewRunners.ForeColor, listViewRunners.BackColor, priceFont);
            itemSize.SubItems.Add(size, listViewRunners.ForeColor, listViewRunners.BackColor, sizeFont);

            price = (runnerPrices.bestPricesToLay.Length > 2) ? runnerPrices.bestPricesToLay[2].price.ToString() : "";
            size = (runnerPrices.bestPricesToLay.Length > 2) ? FormatSize(runnerPrices.bestPricesToLay[2].amountAvailable) : "";
            itemPrice.SubItems.Add(price, listViewRunners.ForeColor, listViewRunners.BackColor, priceFont);
            itemSize.SubItems.Add(size, listViewRunners.ForeColor, listViewRunners.BackColor, sizeFont);
        }

        private void ResizeColumns()
        {
            int available = listViewRunners.ClientSize.Width;
            int prices = 0;
            for (int c = 1; c < listViewRunners.Columns.Count; c++)
            {
                prices += listViewRunners.Columns[c].Width;
            }
            listViewRunners.Columns[0].Width = available - prices;
        }

        private static string FormatSize(double size)
        {
            System.Globalization.CultureInfo info = System.Globalization.CultureInfo.CurrentUICulture;
            string separator = info.NumberFormat.CurrencyDecimalSeparator;

            string formatted = size.ToString("c");
            int index = formatted.IndexOf(separator);
            if (index != -1)
            {
                formatted = formatted.Substring(0, index);
            }
            return formatted;
        }
    }
}

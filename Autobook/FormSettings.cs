using System;
using System.Globalization;
using System.Windows.Forms;

namespace Autobook
{
    public partial class FormSettings : Form
    {
        private static readonly NumberFormatInfo _nfi = new CultureInfo("en-US", false).NumberFormat;

        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            SetFilename();
            LoadSettings();
            SetIpHighLayPanels();
            SetPrHighLayPanels();
        }

        private void SetFilename()
        {
            textBoxFilename.Text = AppSettings.Filename;
        }

        private static int GetRefreshRateAsInt(ComboBox cb)
        {
            int interval = 1000;

            switch (cb.SelectedIndex)
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

        private void ComboBoxPrRefreshRateSelectedIndexChanged(object sender, EventArgs e)
        {
            AppSettings.PrRefreshInterval = GetRefreshRateAsInt(comboBoxPRRefreshRate);
        }

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                string result;
                result = textBoxFilename.Text != AppSettings.Filename ? AppSettings.SaveSettings(textBoxFilename.Text) : AppSettings.SaveSettings();
                if (result == string.Empty)
                    MessageBox.Show("Settings successfully saved", "Settings",
                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(result, "Settings saving failed",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DisplaySettings()
        {
            textBoxIPBackAmount.Text = AppSettings.IpBackAmount.ToString();
            textBoxIPLayAmount.Text = AppSettings.IpLayAmount.ToString();
            textBoxIPLiability.Text = AppSettings.IpLiability.ToString();
            textBoxIPProfit.Text = AppSettings.IpProfit.ToString();
            textBoxIPHighLay.Text = AppSettings.IpHighLay.ToString();
            textBoxPRHighLay.Text = AppSettings.PrHighLay.ToString();
            textBoxIPHighLayMultiplier.Text = AppSettings.IpHighLayMultiplier.ToString();
            textBoxPRHighLayMultiplier.Text = AppSettings.PrHighLayMultiplier.ToString();
            textBoxIPHighLayIncrease.Text = AppSettings.IpHighLayIncrease.ToString();
            textBoxPRHighLayIncrease.Text = AppSettings.PrHighLayIncrease.ToString();
            textBoxTradeOutPrice.Text = AppSettings.TradeOutPrice.ToString();
            textBoxTradeOutPercent.Text = AppSettings.TradeOutPercent.ToString();
            textBoxTradeOutTrigger.Text = AppSettings.TradeOutTrigger.ToString();
            textBoxLowBack.Text = AppSettings.LowBack.ToString();
            textBoxLowLay.Text = AppSettings.LowLay.ToString();
            textBoxDynamicStake.Text = AppSettings.DynamicStake.ToString();
            textBoxBackOverPR.Text = AppSettings.BackOverPr.ToString();
            textBoxLayOverPR.Text = AppSettings.LayOverPr.ToString();
            textBoxBackOverIP.Text = AppSettings.BackOverIp.ToString();
            textBoxLayOverIP.Text = AppSettings.LayOverIp.ToString();
            textBoxPRBackAmount.Text = AppSettings.PrBackAmount.ToString();
            textBoxPRLayAmount.Text = AppSettings.PrLayAmount.ToString();
            textBoxPRLiability.Text = AppSettings.PrLiability.ToString();
            textBoxPRImpliedLiability.Text = AppSettings.PrImpliedLiability.ToString();
            textBoxPRImpliedLiabilityPercent.Text = AppSettings.PrImpliedLiabilityPercent.ToString();
            textBoxIPImpliedLiability.Text = AppSettings.IpImpliedLiability.ToString();
            textBoxIPImpliedLiabilityPercent.Text = AppSettings.IpImpliedLiabilityPercent.ToString();
            textBoxPRProfit.Text = AppSettings.PrProfit.ToString();
            textBoxPRUnder.Text = AppSettings.PrUnderMargin.ToString();
            textBoxPROver.Text = AppSettings.PrOverMargin.ToString();
            radioButtonIPFixedUnderOver.Checked = AppSettings.IpUnderOverMode == UnderOverMode.Fixed;
            SetUnderOverVal();
            textBoxPRVariant.Text = AppSettings.PrVariant.ToString();
            textBoxPRDynamicLiabilityVariant.Text = AppSettings.PrDynamicLiabilityVariant.ToString();
            textBoxIPDynamicLiabilityVariant.Text = AppSettings.IpDynamicLiabilityVariant.ToString();
            radioButtonIPLiability.Checked = AppSettings.IpLayBetMode == LayBetMode.Liability;
            radioButtonIPPayout.Checked = AppSettings.IpLayBetMode == LayBetMode.Payout;
            radioButtonPRLiability.Checked = AppSettings.PrLayBetMode == LayBetMode.Liability;
            radioButtonPRPayout.Checked = AppSettings.PrLayBetMode == LayBetMode.Payout;
            radioButtonIPBdbBets.Checked = AppSettings.IpBackBetDealMode == BetDealMode.Bets;
            radioButtonIPBdbNoBets.Checked = AppSettings.IpBackBetDealMode == BetDealMode.NoBets;
            radioButtonIPBdlBets.Checked = AppSettings.IpLayBetDealMode == BetDealMode.Bets;
            radioButtonIPBdlNoBets.Checked = AppSettings.IpLayBetDealMode == BetDealMode.NoBets;
            radioButtonPRBdbBets.Checked = AppSettings.PrBackBetDealMode == BetDealMode.Bets;
            radioButtonPRBdbNoBets.Checked = AppSettings.PrBackBetDealMode == BetDealMode.NoBets;
            radioButtonPRBdlBets.Checked = AppSettings.PrLayBetDealMode == BetDealMode.Bets;
            radioButtonPRBdlNoBet.Checked = AppSettings.PrLayBetDealMode == BetDealMode.NoBets;
            radioButtonPRStaticHighLay.Checked = AppSettings.PrHighLayMode == HighLayMode.Static;
            radioButtonPRDynamicHighLay.Checked = AppSettings.PrHighLayMode == HighLayMode.Dynamic;
            radioButtonIPDynamicHighLay.Checked = AppSettings.IpHighLayMode == HighLayMode.Dynamic;
            radioButtonIPStaticHighLay.Checked = AppSettings.IpHighLayMode == HighLayMode.Static;
            radioButtonTradeOutCancelStop.Checked = AppSettings.TradeOutEnd == TradeOutEndMode.CancelAllStop;
            radioButtonTradeOutContinue.Checked = AppSettings.TradeOutEnd == TradeOutEndMode.Continue;
            radioButtonTradeOutStop.Checked = AppSettings.TradeOutEnd == TradeOutEndMode.Stop;
            radioButtonPRBTLapse.Checked = AppSettings.BetPersistence == BetPersistenceType.Lapse;
            radioButtonPRBTPersist.Checked = AppSettings.BetPersistence == BetPersistenceType.Persist;
            radioButtonPRBTMarketOnClose.Checked = AppSettings.BetPersistence == BetPersistenceType.MarketOnClose;
            radioButtonIPFixedUnderOver.Checked = AppSettings.IpUnderOverMode == UnderOverMode.Fixed;
            radioButtonIPDynamicUnderOver.Checked = AppSettings.IpUnderOverMode == UnderOverMode.Dynamic;
            checkBoxIPSmallSizeLay.Checked = AppSettings.IpIsSmallLay;
            checkBoxPRSmallSizeLay.Checked = AppSettings.PrIsSmallLay;
            checkBoxIPCFReset.Checked = AppSettings.IpcfReset;
            checkBoxIPIgnoreHighLay.Checked = AppSettings.IpIsIgnoreHighLay;
            checkBoxPRIgnoreHighLay.Checked = AppSettings.PrIsIgnoreHighLay;
            checkBoxLayOverPR.Checked = AppSettings.IsLayOverPr;
            checkBoxBackOverPR.Checked = AppSettings.IsBackOverPr;
            checkBoxLayOverIP.Checked = AppSettings.IsLayOverIp;
            checkBoxBackOverIP.Checked = AppSettings.IsBackOverIp;
            //textBoxBetsRefreshRate.Text = AppSettings.BetsRefreshInterval.ToString ();
            checkBoxCancelBets.Checked = AppSettings.IsCancelBets;
            checkBoxPRNextBestPrice.Checked = AppSettings.PrIsNextBestPrice;
            checkBoxIPNextBestPrice.Checked = AppSettings.IpIsNextBestPrice;
            checkBoxPRSwopPrice.Checked = AppSettings.PrIsSwopPrice;
            checkBoxIPSwopPrice.Checked = AppSettings.IpIsSwopPrice;
            checkBoxPRDynamicLiability.Checked = AppSettings.PrIsDynamicLiability;
            checkBoxIPDynamicLiability.Checked = AppSettings.IpIsDynamicLiability;
            checkBoxIPLayBookPercentage.Checked = AppSettings.IpIsLayBookPercentage;
            SetRefreshRate(comboBoxPRRefreshRate, ref AppSettings.PrRefreshInterval);
            SetRefreshRate(comboBoxIPRefreshRate, ref AppSettings.IpRefreshInterval);
            textBoxBeforeStart.Text = AppSettings.BeforeStart.ToString();
        }

        private static void SetRefreshRate(ComboBox cb, ref int interval)
        {
            switch (interval)
            {
                case 100:
                    cb.SelectedIndex = 0;
                    break;

                case 200:
                    cb.SelectedIndex = 1;
                    break;

                case 500:
                    cb.SelectedIndex = 2;
                    break;

                case 1000:
                    cb.SelectedIndex = 3;
                    break;

                case 2000:
                    cb.SelectedIndex = 4;
                    break;

                case 5000:
                    cb.SelectedIndex = 5;
                    break;

                case 10000:
                    cb.SelectedIndex = 6;
                    break;

                case 30000:
                    cb.SelectedIndex = 7;
                    break;

                default:
                    interval = 1000;
                    cb.SelectedIndex = 3;
                    break;
            }
        }

        private bool SaveSettings()
        {
            try
            {
                AppSettings.PrLayBetMode = (radioButtonPRPayout.Checked) ? LayBetMode.Payout : LayBetMode.Liability;
                AppSettings.IpLayBetMode = (radioButtonIPPayout.Checked) ? LayBetMode.Payout : LayBetMode.Liability;
                AppSettings.PrIsSmallLay = checkBoxPRSmallSizeLay.Checked;
                AppSettings.IpIsSmallLay = checkBoxIPSmallSizeLay.Checked;
                AppSettings.IpcfReset = checkBoxIPCFReset.Checked;
                AppSettings.IpIsIgnoreHighLay = checkBoxIPIgnoreHighLay.Checked;
                AppSettings.PrIsIgnoreHighLay = checkBoxPRIgnoreHighLay.Checked;
                AppSettings.IsCancelBets = checkBoxCancelBets.Checked;
                AppSettings.PrIsSwopPrice = checkBoxPRSwopPrice.Checked;
                AppSettings.IpIsSwopPrice = checkBoxIPSwopPrice.Checked;
                AppSettings.PrIsNextBestPrice = checkBoxPRNextBestPrice.Checked;
                AppSettings.IpIsNextBestPrice = checkBoxIPNextBestPrice.Checked;
                AppSettings.PrIsDynamicLiability = checkBoxPRDynamicLiability.Checked;
                AppSettings.IpIsDynamicLiability = checkBoxIPDynamicLiability.Checked;
                AppSettings.IsBackOverPr = checkBoxBackOverPR.Checked;
                AppSettings.IsLayOverPr = checkBoxLayOverPR.Checked;
                AppSettings.IsBackOverIp = checkBoxBackOverIP.Checked;
                AppSettings.IsLayOverIp = checkBoxLayOverIP.Checked;
                AppSettings.IpIsLayBookPercentage = checkBoxIPLayBookPercentage.Checked;
                AppSettings.IpBackBetDealMode = (radioButtonIPBdbBets.Checked) ? BetDealMode.Bets : BetDealMode.NoBets;
                AppSettings.IpLayBetDealMode = (radioButtonIPBdlBets.Checked) ? BetDealMode.Bets : BetDealMode.NoBets;
                AppSettings.PrBackBetDealMode = (radioButtonPRBdbBets.Checked) ? BetDealMode.Bets : BetDealMode.NoBets;
                AppSettings.PrLayBetDealMode = (radioButtonPRBdlBets.Checked) ? BetDealMode.Bets : BetDealMode.NoBets;
                AppSettings.IpHighLayMode = (radioButtonIPStaticHighLay.Checked) ? HighLayMode.Static : HighLayMode.Dynamic;
                AppSettings.PrHighLayMode = (radioButtonPRStaticHighLay.Checked) ? HighLayMode.Static : HighLayMode.Dynamic;
                AppSettings.IpUnderOverMode = (radioButtonIPFixedUnderOver.Checked) ? UnderOverMode.Fixed : UnderOverMode.Dynamic;
                AppSettings.TradeOutEnd = radioButtonTradeOutCancelStop.Checked ? TradeOutEndMode.CancelAllStop
                  : radioButtonTradeOutContinue.Checked ? TradeOutEndMode.Continue : TradeOutEndMode.Stop;
                AppSettings.BetPersistence = radioButtonPRBTLapse.Checked ? BetPersistenceType.Lapse
                    : radioButtonPRBTPersist.Checked ? BetPersistenceType.Persist : BetPersistenceType.MarketOnClose;

                AppSettings.IpHighLay = CheckDoubleInput(textBoxIPHighLay.Text);
                if (AppSettings.IpHighLay == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play High Lay' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrHighLay = CheckDoubleInput(textBoxPRHighLay.Text);
                if (AppSettings.PrHighLay == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race High Lay' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpHighLayMultiplier = CheckDoubleInput(textBoxIPHighLayMultiplier.Text);
                if (AppSettings.IpHighLayMultiplier == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play High Lay Multiplier' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrHighLayMultiplier = CheckDoubleInput(textBoxPRHighLayMultiplier.Text);
                if (AppSettings.PrHighLayMultiplier == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race High Lay Multiplier' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpBackAmount = CheckDoubleInput(textBoxIPBackAmount.Text);
                if (AppSettings.IpBackAmount == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Back Amount' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpLayAmount = CheckDoubleInput(textBoxIPLayAmount.Text);
                if (AppSettings.IpLayAmount == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Lay Amount' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpLiability = CheckDoubleInput(textBoxIPLiability.Text);
                if (AppSettings.IpLiability == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Liability' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpImpliedLiability = CheckDoubleInput(textBoxIPImpliedLiability.Text);
                if (AppSettings.IpImpliedLiability == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Implied Liability' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpImpliedLiabilityPercent = CheckDoubleInput(textBoxIPImpliedLiabilityPercent.Text);
                if (AppSettings.IpImpliedLiabilityPercent == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Implied Liability Percent' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrImpliedLiability = CheckDoubleInput(textBoxPRImpliedLiability.Text);
                if (AppSettings.PrImpliedLiability == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Implied Liability' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrImpliedLiabilityPercent = CheckDoubleInput(textBoxPRImpliedLiabilityPercent.Text);
                if (AppSettings.PrImpliedLiabilityPercent == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Implied Liability Percent' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpProfit = CheckDoubleInput(textBoxIPProfit.Text);
                if (AppSettings.IpProfit == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Profit' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.TradeOutPrice = CheckDoubleInput(textBoxTradeOutPrice.Text);
                if (AppSettings.TradeOutPrice == -1.0D)
                {
                    MessageBox.Show("Invalid 'Trade Out Price' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.TradeOutPercent = CheckDoubleInput(textBoxTradeOutPercent.Text);
                if (AppSettings.TradeOutPercent == -1.0D)
                {
                    MessageBox.Show("Invalid 'Trade Out Percent' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.TradeOutTrigger = CheckDoubleInput(textBoxTradeOutTrigger.Text);
                if (AppSettings.TradeOutTrigger == -1.0D)
                {
                    MessageBox.Show("Invalid 'Trade Out Trigger' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.LowBack = CheckDoubleInput(textBoxLowBack.Text);
                if (AppSettings.LowBack == -1.0D)
                {
                    MessageBox.Show("Invalid 'Low Back Amount' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.LowLay = CheckDoubleInput(textBoxLowLay.Text);
                if (AppSettings.LowLay == -1.0D)
                {
                    MessageBox.Show("Invalid 'Low Lay Amount' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.DynamicStake = CheckDoubleInput(textBoxDynamicStake.Text);
                if (AppSettings.DynamicStake == -1.0D)
                {
                    MessageBox.Show("Invalid 'Dynamic Stake' field", "Settings Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.LayOverPr = CheckDoubleInput(textBoxLayOverPR.Text);
                if (AppSettings.LayOverPr == -1.0D)
                {
                    MessageBox.Show("Invalid 'Lay Over Pre-Race' amount field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.BackOverPr = CheckDoubleInput(textBoxBackOverPR.Text);
                if (AppSettings.BackOverPr == -1.0D)
                {
                    MessageBox.Show("Invalid 'Back Over Pre-Race' amount field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.LayOverIp = CheckDoubleInput(textBoxLayOverIP.Text);
                if (AppSettings.LayOverIp == -1.0D)
                {
                    MessageBox.Show("Invalid 'Lay Over In-Play' amount field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.BackOverIp = CheckDoubleInput(textBoxBackOverIP.Text);
                if (AppSettings.BackOverIp == -1.0D)
                {
                    MessageBox.Show("Invalid 'Back Over In-Play' amount field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrBackAmount = CheckDoubleInput(textBoxPRBackAmount.Text);
                if (AppSettings.PrBackAmount == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Back Amount' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrLayAmount = CheckDoubleInput(textBoxPRLayAmount.Text);
                if (AppSettings.PrLayAmount == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Lay Amount' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrLiability = CheckDoubleInput(textBoxPRLiability.Text);
                if (AppSettings.PrLiability == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Liability' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrProfit = CheckDoubleInput(textBoxPRProfit.Text);
                if (AppSettings.PrProfit == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Profit' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrUnderMargin = CheckDoubleInput(textBoxPRUnder.Text);
                if (AppSettings.PrUnderMargin == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Under Margin' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrOverMargin = CheckDoubleInput(textBoxPROver.Text);
                if (AppSettings.PrOverMargin == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Over Margin' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrVariant = CheckDoubleInput(textBoxPRVariant.Text);
                if (AppSettings.PrVariant == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Variant' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                double ipUnder = CheckDoubleInput(textBoxIPUnder.Text);
                if (ipUnder == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Under Margin' field", "Settings Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (radioButtonIPFixedUnderOver.Checked)
                    AppSettings.IpUnderMarginFixed = ipUnder;
                else
                    AppSettings.IpUnderMarginDynamic = ipUnder;

                double ipOver = CheckDoubleInput(textBoxIPOver.Text);
                if (ipOver == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Over Margin' field", "Settings Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (radioButtonIPFixedUnderOver.Checked)
                    AppSettings.IpOverMarginFixed = ipOver;
                else
                    AppSettings.IpOverMarginDynamic = ipOver;

                AppSettings.PrDynamicLiabilityVariant = CheckDoubleInput(textBoxPRDynamicLiabilityVariant.Text);
                if (AppSettings.PrDynamicLiabilityVariant == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race Dynamic Liability Variant' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpDynamicLiabilityVariant = CheckDoubleInput(textBoxIPDynamicLiabilityVariant.Text);
                if (AppSettings.IpDynamicLiabilityVariant == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play Dynamic Liability Variant' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrRefreshInterval = GetRefreshRateAsInt(comboBoxPRRefreshRate);
                if (AppSettings.PrRefreshInterval == 0)
                {
                    MessageBox.Show("Invalid 'Pre-Race Refresh Interval' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpRefreshInterval = GetRefreshRateAsInt(comboBoxIPRefreshRate);
                if (AppSettings.IpRefreshInterval == 0)
                {
                    MessageBox.Show("Invalid 'In-Play Refresh Interval' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.IpHighLayIncrease = CheckDoubleInput(textBoxIPHighLayIncrease.Text);
                if (AppSettings.IpHighLayIncrease == -1.0D)
                {
                    MessageBox.Show("Invalid 'In-Play HighLay Increase' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.PrHighLayIncrease = CheckDoubleInput(textBoxPRHighLayIncrease.Text);
                if (AppSettings.PrHighLayIncrease == -1.0D)
                {
                    MessageBox.Show("Invalid 'Pre-Race HighLay Increase' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                AppSettings.BeforeStart = CheckIntInput(textBoxBeforeStart.Text);
                if (AppSettings.BeforeStart == 0)
                {
                    MessageBox.Show("Invalid 'Time Before Start' field", "Settings Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                /*
                        AppSettings.BetsRefreshInterval = CheckIntInput (textBoxBetsRefreshRate.Text);
                        if (AppSettings.BetsRefreshInterval == 0)
                        {
                          MessageBox.Show ("Invalid 'Bets Refresh Interval' field", "Settings Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                          return false;
                        }
                */
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "SaveSettings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static int CheckIntInput(string text)
        {
            if (text.Length <= 0)
            {
                return 0;
            }

            return int.TryParse(text, out var val) ? val : 0;
        }

        private static double CheckDoubleInput(string text)
        {
            if (text.Length <= 0)
            {
                return -1.0D;
            }

            if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, _nfi, out var val))
                return val;

            return -1.0D;
        }

        private void ComboBoxIpRefreshRateSelectedIndexChanged(object sender, EventArgs e)
        {
            AppSettings.IpRefreshInterval = GetRefreshRateAsInt(comboBoxIPRefreshRate);
        }

        private void buttonDefaultFilename_Click(object sender, EventArgs e)
        {
            AppSettings.SetDefaultFile();
            SetFilename();
            LoadSettings();
        }

        private void buttonChangeFilename_Click(object sender, EventArgs e)
        {
            string filename = GetFilename();
            if (filename != string.Empty)
            {
                AppSettings.ReadSettings(filename);
                SetFilename();
                DisplaySettings();
            }
        }

        private string GetFilename()
        {
            openFileDialogSettings.InitialDirectory = Environment.CurrentDirectory;
            openFileDialogSettings.FilterIndex = 1;
            openFileDialogSettings.RestoreDirectory = true;
            openFileDialogSettings.Multiselect = false;
            return openFileDialogSettings.ShowDialog() == DialogResult.OK ? openFileDialogSettings.FileName : string.Empty;
        }

        private bool LoadSettings()
        {
            try
            {
                string result = AppSettings.ReadSettings();
                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(this, result, "Load Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisplaySettings();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "LoadSettings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void buttonLoadSettings_Click(object sender, EventArgs e)
        {
            if (textBoxFilename.Text.Length <= 0)
                MessageBox.Show("Please set filename", "Settings loading failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                AppSettings.ReadSettings(textBoxFilename.Text);
                LoadSettings();
            }
        }

        private void SetPrHighLayPanels()
        {
            panelPRHighLayDynamic.Enabled = radioButtonPRDynamicHighLay.Checked;
            panelPRHighLayStatic.Enabled = radioButtonPRStaticHighLay.Checked;
        }

        private void radioButtonPRStaticHighLay_CheckedChanged(object sender, EventArgs e)
        {
            SetPrHighLayPanels();
        }

        private void radioButtonPRDynamicHighLay_CheckedChanged(object sender, EventArgs e)
        {
            SetPrHighLayPanels();
        }

        private void SetIpHighLayPanels()
        {
            panelIPHighLayStatic.Enabled = radioButtonIPStaticHighLay.Checked;
            panelIPHighLayDynamic.Enabled = radioButtonIPDynamicHighLay.Checked;
        }

        private void radioButtonIPStaticHighLay_CheckedChanged(object sender, EventArgs e)
        {
            SetIpHighLayPanels();
        }

        private void radioButtonIPDynamicHighLay_CheckedChanged(object sender, EventArgs e)
        {
            SetIpHighLayPanels();
        }

        private void checkBoxBackOverPR_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxBackOverPR.Text = (checkBoxBackOverPR.Checked) ? "On" : "Off";
        }

        private void checkBoxLayOverPR_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxLayOverPR.Text = (checkBoxLayOverPR.Checked) ? "On" : "Off";
        }

        private void checkBoxBackOverIP_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxBackOverIP.Text = (checkBoxBackOverIP.Checked) ? "On" : "Off";
        }

        private void checkBoxLayOverIP_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxLayOverIP.Text = (checkBoxLayOverIP.Checked) ? "On" : "Off";
        }

        private void radioButtonIPFixedUnderOver_CheckedChanged(object sender, EventArgs e)
        {
            SetUnderOverVal();
        }

        private void radioButtonIPDynamicUnderOver_CheckedChanged(object sender, EventArgs e)
        {
            SetUnderOverVal();
        }

        private void SetUnderOverVal()
        {
            if (radioButtonIPFixedUnderOver.Checked)
            {
                textBoxIPUnder.Text = AppSettings.IpUnderMarginFixed.ToString();
                textBoxIPOver.Text = AppSettings.IpOverMarginFixed.ToString();
            }
            else
            {
                textBoxIPUnder.Text = AppSettings.IpUnderMarginDynamic.ToString();
                textBoxIPOver.Text = AppSettings.IpOverMarginDynamic.ToString();

            }
        }
    }
}
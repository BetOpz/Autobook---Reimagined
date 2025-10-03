using System;

namespace Autobook
{
    public enum LayBetMode
    {
        Payout, Liability
    }

    public enum BetDealMode
    {
        NoBets, Bets
    }

    public enum HighLayMode
    {
        Static, Dynamic, Percent
    }

    public enum TradeOutEndMode
    {
        Continue, CancelAllStop, Stop
    }

    public enum UnderOverMode
    {
        Fixed, Dynamic
    }

    public enum BetPersistenceType
    {
        Lapse, Persist, MarketOnClose
    }

    public static class AppSettings
    {
        private static readonly Settings _settings = new Settings();

        public static int PrRefreshInterval = 1000;
        public static int IpRefreshInterval = 1000;
        public static int BeforeStart = 15; // in minutes
        public static double TradeOutPrice = 1.5d;
        public static double TradeOutPercent = 50.0d;
        public static double TradeOutTrigger = 2.0d;
        public static double LowBack = 1.2d;
        public static double LowLay = 1.2d;
        public static double DynamicStake = 0.0d;
        public static double BackOverPr = 10.0d;
        public static double LayOverPr = 10.0d;
        public static double BackOverIp = 10.0d;
        public static double LayOverIp = 10.0d;
        public static bool IsBackOverPr;
        public static bool IsLayOverPr;
        public static bool IsBackOverIp;
        public static bool IsLayOverIp;
        public static double PrUnderMargin = 0.05d;
        public static double PrOverMargin = 0.05d;
        public static double IpUnderMarginFixed = 0.05d;
        public static double IpOverMarginFixed = 0.05d;
        public static double IpUnderMarginDynamic = 0.5d;
        public static double IpOverMarginDynamic = 0.8d;
        public static double PrVariant = 0.5d;
        public static double IpHighLay = 10.0d;
        public static double PrHighLay = 10.0d;
        public static double IpHighLayMultiplier = 5.0d;
        public static double PrHighLayMultiplier = 5.0d;
        public static double PrProfit = 100.0d;
        public static double PrHighLayIncrease = 100.0d;
        public static double IpHighLayIncrease = 100.0d;
        public static double IpProfit = 100.0d;
        public static double PrLiability = 100.0d;
        public static double IpLiability = 100.0d;
        public static double PrImpliedLiability = 10.0d;
        public static double PrImpliedLiabilityPercent;
        public static double IpImpliedLiability = 10.0d;
        public static double IpImpliedLiabilityPercent;
        public static double PrBackAmount = 100.0d;
        public static double IpBackAmount = 100.0d;
        public static double IpLayAmount = 100.0d;
        public static double PrLayAmount = 100.0d;
        public static double PrDynamicLiabilityVariant;
        public static double IpDynamicLiabilityVariant;
        public static LayBetMode IpLayBetMode = LayBetMode.Payout;
        public static LayBetMode PrLayBetMode = LayBetMode.Payout;
        public static BetDealMode PrLayBetDealMode = BetDealMode.Bets;
        public static BetDealMode PrBackBetDealMode = BetDealMode.Bets;
        public static BetDealMode IpLayBetDealMode = BetDealMode.Bets;
        public static BetDealMode IpBackBetDealMode = BetDealMode.Bets;
        public static HighLayMode IpHighLayMode = HighLayMode.Static;
        public static HighLayMode PrHighLayMode = HighLayMode.Static;
        public static TradeOutEndMode TradeOutEnd = TradeOutEndMode.Continue;
        public static BetPersistenceType BetPersistence = BetPersistenceType.Lapse;
        public static UnderOverMode IpUnderOverMode = UnderOverMode.Fixed;
        public static bool IpIsSmallLay;
        public static bool PrIsSmallLay;
        public static bool IpcfReset = true;
        public static bool IpIsIgnoreHighLay = true;
        public static bool PrIsIgnoreHighLay = true;
        //public static int BetsRefreshInterval = 3000;
        public static bool IsCancelBets;
        public static bool PrIsNextBestPrice;
        public static bool IpIsNextBestPrice;
        public static bool PrIsDynamicLiability;
        public static bool IpIsDynamicLiability;
        public static bool IpIsLayBookPercentage;
        public static bool IpIsSwopPrice = true;
        public static bool PrIsSwopPrice;
        public static bool IsPriceVirtualise;
        public static string BetfairUser = string.Empty;
        public static string BetfairPassword = string.Empty;
        public static string BetfairNgKey = "ykEM0nRo08oMkmCw";

        public static string Filename => _settings.Filename;

        public static void SetDefaultFile()
        {
            _settings.SetDefaultFile();
        }

        public static string SaveSettings(string filename)
        {
            var result = _settings.LoadXml(filename);

            return result != string.Empty ? result : SaveSettings();
        }

        public static string SaveSettings()
        {
            try
            {
                _settings.PutSetting("PRRefreshInterval", PrRefreshInterval);
                _settings.PutSetting("IPRefreshInterval", IpRefreshInterval);
                _settings.PutSetting("BeforeStart", BeforeStart);
                _settings.PutSetting("IPHighLay", IpHighLay);
                _settings.PutSetting("PRHighLay", PrHighLay);
                _settings.PutSetting("PRHighLayMultiplier", PrHighLayMultiplier);
                _settings.PutSetting("IPHighLayMultiplier", IpHighLayMultiplier);
                _settings.PutSetting("IPHighLayIncrease", IpHighLayIncrease);
                _settings.PutSetting("PRHighLayIncrease", PrHighLayIncrease);
                _settings.PutSetting("LowBack", LowBack);
                _settings.PutSetting("LowLay", LowLay);
                _settings.PutSetting("BackOverPR", BackOverPr);
                _settings.PutSetting("LayOverPR", LayOverPr);
                _settings.PutSetting("BackOverIP", BackOverIp);
                _settings.PutSetting("LayOverIP", LayOverIp);
                _settings.PutSetting("IsBackOverPR", IsBackOverPr);
                _settings.PutSetting("IsLayOverPR", IsLayOverPr);
                _settings.PutSetting("IsBackOverIP", IsBackOverIp);
                _settings.PutSetting("IsLayOverIP", IsLayOverIp);
                _settings.PutSetting("PRUnderMargin", PrUnderMargin);
                _settings.PutSetting("PROverMargin", PrOverMargin);
                _settings.PutSetting("IPUnderMarginFixed", IpUnderMarginFixed);
                _settings.PutSetting("IPOverMarginFixed", IpOverMarginFixed);
                _settings.PutSetting("IPUnderMarginDynamic", IpUnderMarginDynamic);
                _settings.PutSetting("IPOverMarginDynamic", IpOverMarginDynamic);
                _settings.PutSetting("PRVariant", PrVariant);
                _settings.PutSetting("PRProfit", PrProfit);
                _settings.PutSetting("IPProfit", IpProfit);
                _settings.PutSetting("PRLiability", PrLiability);
                _settings.PutSetting("IPLiability", IpLiability);
                _settings.PutSetting("PRImpliedLiability", PrImpliedLiability);
                _settings.PutSetting("PRImpliedLiabilityPercent", PrImpliedLiabilityPercent);
                _settings.PutSetting("IPImpliedLiability", IpImpliedLiability);
                _settings.PutSetting("IPImpliedLiabilityPercent", IpImpliedLiabilityPercent);
                _settings.PutSetting("PRBackAmount", PrBackAmount);
                _settings.PutSetting("IPBackAmount", IpBackAmount);
                _settings.PutSetting("IPLayAmount", IpLayAmount);
                _settings.PutSetting("PRLayAmount", PrLayAmount);
                _settings.PutSetting("IPLayBetMode", IpLayBetMode.ToString());
                _settings.PutSetting("PRLayBetMode", PrLayBetMode.ToString());
                _settings.PutSetting("IPIsSmallLay", IpIsSmallLay);
                _settings.PutSetting("PRIsSmallLay", PrIsSmallLay);
                _settings.PutSetting("IPCFReset", IpcfReset);
                _settings.PutSetting("IPIsIgnoreHighLay", IpIsIgnoreHighLay);
                _settings.PutSetting("PRIsIgnoreHighLay", PrIsIgnoreHighLay);
                _settings.PutSetting("PRLayBetDealMode", PrLayBetDealMode.ToString());
                _settings.PutSetting("PRBackBetDealMode", PrBackBetDealMode.ToString());
                _settings.PutSetting("IPLayBetDealMode", IpLayBetDealMode.ToString());
                _settings.PutSetting("IPBackBetDealMode", IpBackBetDealMode.ToString());
                _settings.PutSetting("IPHighLayMode", IpHighLayMode.ToString());
                _settings.PutSetting("PRHighLayMode", PrHighLayMode.ToString());
                _settings.PutSetting("TradeOutEnd", TradeOutEnd.ToString());
                _settings.PutSetting("BetPersistence", BetPersistence.ToString());
                _settings.PutSetting("IPUnderOverMode", IpUnderOverMode.ToString());
                //_settings.PutSetting ("BetsRefreshInterval", BetsRefreshInterval);
                _settings.PutSetting("IsCancelBets", IsCancelBets);
                _settings.PutSetting("PRIsNextBestPrice", PrIsNextBestPrice);
                _settings.PutSetting("IPIsNextBestPrice", IpIsNextBestPrice);
                _settings.PutSetting("PRIsSwopPrice", PrIsSwopPrice);
                _settings.PutSetting("IPIsSwopPrice", IpIsSwopPrice);
                _settings.PutSetting("PRIsDynamicLiability", PrIsDynamicLiability);
                _settings.PutSetting("IPIsDynamicLiability", IpIsDynamicLiability);
                _settings.PutSetting("IPIsLayBookPercentage", IpIsLayBookPercentage);
                _settings.PutSetting("IsPriceVirtualise", IsPriceVirtualise);
                _settings.PutSetting("BetfairUser", BetfairUser);
                var encryptedPwd = CryptoString.Encrypt(BetfairPassword);
                _settings.PutSetting("BetfairPassword", encryptedPwd);
                _settings.PutSetting("TradeOutPrice", TradeOutPrice);
                _settings.PutSetting("TradeOutPercent", TradeOutPercent);
                _settings.PutSetting("TradeOutTrigger", TradeOutTrigger);
                _settings.PutSetting("BetfairNgKey", BetfairNgKey);
                _settings.PutSetting("PRDynamicLiabilityVariant", PrDynamicLiabilityVariant);
                _settings.PutSetting("IPDynamicLiabilityVariant", IpDynamicLiabilityVariant);
                _settings.PutSetting("DynamicStake", DynamicStake);

                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("SaveSettings: " + ex);
                return ex.Message;
            }
        }

        public static string ReadSettings(string filename)
        {
            _settings.LoadXml(filename);
            return ReadSettings();
        }

        public static string ReadSettings()
        {
            try
            {
                PrRefreshInterval = _settings.GetSetting("PRRefreshInterval", 1000);
                IpRefreshInterval = _settings.GetSetting("IPRefreshInterval", 1000);
                BeforeStart = _settings.GetSetting("BeforeStart", 15);
                IpHighLay = _settings.GetSetting("IPHighLay", 10.0D);
                PrHighLay = _settings.GetSetting("PRHighLay", 10.0D);
                IpHighLayMultiplier = _settings.GetSetting("IPHighLayMultiplier", 5.0D);
                PrHighLayMultiplier = _settings.GetSetting("PRHighLayMultiplier", 5.0D);
                IpHighLayIncrease = _settings.GetSetting("IPHighLayIncrease", 100.0D);
                PrHighLayIncrease = _settings.GetSetting("PRHighLayIncrease", 100.0D);
                LowBack = _settings.GetSetting("LowBack", 1.2D);
                LowLay = _settings.GetSetting("LowLay", 1.2D);
                DynamicStake =_settings.GetSetting("DynamicStake", 0.0D);
                BackOverPr = _settings.GetSetting("BackOverPR", 10.0D);
                LayOverPr = _settings.GetSetting("LayOverPR", 10.0D);
                BackOverIp = _settings.GetSetting("BackOverIP", 10.0D);
                LayOverIp = _settings.GetSetting("LayOverIP", 10.0D);
                IsBackOverPr = _settings.GetSetting("IsBackOverPR", false);
                IsLayOverPr = _settings.GetSetting("IsLayOverPR", false);
                IsBackOverIp = _settings.GetSetting("IsBackOverIP", false);
                IsLayOverIp = _settings.GetSetting("IsLayOverIP", false);
                PrUnderMargin = _settings.GetSetting("PRUnderMargin", 0.05D);
                PrOverMargin = _settings.GetSetting("PROverMargin", 0.05D);
                IpUnderMarginFixed = _settings.GetSetting("IPUnderMarginFixed", 0.05D);
                IpOverMarginFixed = _settings.GetSetting("IPOverMarginFixed", 0.05D);
                IpUnderMarginDynamic = _settings.GetSetting("IPUnderMarginDynamic", 0.5D);
                IpOverMarginDynamic = _settings.GetSetting("IPOverMarginDynamic", 0.8D);
                PrVariant = _settings.GetSetting("PRVariant", 0.5D);
                PrProfit = _settings.GetSetting("PRProfit", 3.0D);
                IpProfit = _settings.GetSetting("IPProfit", 5.0D);
                PrLiability = _settings.GetSetting("PRLiability", 2.0D);
                PrDynamicLiabilityVariant = _settings.GetSetting("PRDynamicLiabilityVariant", 0.0D);
                if (Math.Sign(PrDynamicLiabilityVariant) > 0)
                    PrDynamicLiabilityVariant = -PrDynamicLiabilityVariant;
                IpDynamicLiabilityVariant = _settings.GetSetting("IPDynamicLiabilityVariant", 0.0D);
                if (Math.Sign(IpDynamicLiabilityVariant) > 0)
                    IpDynamicLiabilityVariant = -IpDynamicLiabilityVariant;
                // liability setting should be negative
                if (Math.Sign(PrLiability) > 0)
                    PrLiability = -PrLiability;
                IpLiability = _settings.GetSetting("IPLiability", 3.0D);
                // liability setting should be negative
                if (Math.Sign(IpLiability) > 0)
                    IpLiability = -IpLiability;
                PrImpliedLiability = _settings.GetSetting("PRImpliedLiability", 10.0D);
                PrImpliedLiabilityPercent = _settings.GetSetting("PRImpliedLiabilityPercent", 0.0D);
                IpImpliedLiability = _settings.GetSetting("IPImpliedLiability", 10.0D);
                IpImpliedLiabilityPercent = _settings.GetSetting("IPImpliedLiabilityPercent", 0.0D);
                PrBackAmount = _settings.GetSetting("PRBackAmount", 15.0D);
                IpBackAmount = _settings.GetSetting("IPBackAmount", 20.0D);
                IpLayAmount = _settings.GetSetting("IPLayAmount", 100.0D);
                PrLayAmount = _settings.GetSetting("PRLayAmount", 20.0D);
                TradeOutPercent = _settings.GetSetting("TradeOutPercent", 50.0D);
                TradeOutPrice = _settings.GetSetting("TradeOutPrice", 1.5D);
                TradeOutTrigger = _settings.GetSetting("TradeOutTrigger", 2.0D);
                PrLayBetMode = (LayBetMode)Enum.Parse(typeof(LayBetMode), _settings.GetSetting("PRLayBetMode", LayBetMode.Payout.ToString()));
                IpLayBetMode = (LayBetMode)Enum.Parse(typeof(LayBetMode), _settings.GetSetting("IPLayBetMode", LayBetMode.Payout.ToString()));
                IpIsSmallLay = _settings.GetSetting("IPIsSmallLay", false);
                PrIsSmallLay = _settings.GetSetting("PRIsSmallLay", false);
                IpcfReset = _settings.GetSetting("IPCFReset", true);
                IpIsIgnoreHighLay = _settings.GetSetting("IPIsIgnoreHighLay", true);
                PrIsIgnoreHighLay = _settings.GetSetting("PRIsIgnoreHighLay", true);
                IpIsLayBookPercentage = _settings.GetSetting("IPIsLayBookPercentage", false);
                PrLayBetDealMode = (BetDealMode)Enum.Parse(typeof(BetDealMode), _settings.GetSetting("PRLayBetDealMode", BetDealMode.Bets.ToString()));
                PrBackBetDealMode = (BetDealMode)Enum.Parse(typeof(BetDealMode), _settings.GetSetting("PRBackBetDealMode", BetDealMode.Bets.ToString()));
                IpLayBetDealMode = (BetDealMode)Enum.Parse(typeof(BetDealMode), _settings.GetSetting("IPLayBetDealMode", BetDealMode.Bets.ToString()));
                IpBackBetDealMode = (BetDealMode)Enum.Parse(typeof(BetDealMode), _settings.GetSetting("IPBackBetDealMode", BetDealMode.Bets.ToString()));
                PrHighLayMode = (HighLayMode)Enum.Parse(typeof(HighLayMode), _settings.GetSetting("PRHighLayMode", HighLayMode.Static.ToString()));
                IpHighLayMode = (HighLayMode)Enum.Parse(typeof(HighLayMode), _settings.GetSetting("IPHighLayMode", HighLayMode.Static.ToString()));
                TradeOutEnd = (TradeOutEndMode)Enum.Parse(typeof(TradeOutEndMode), _settings.GetSetting("TradeOutEnd", TradeOutEndMode.Continue.ToString()));
                BetPersistence = (BetPersistenceType)Enum.Parse(typeof(BetPersistenceType), _settings.GetSetting("BetPersistence", BetPersistenceType.Lapse.ToString()));
                IpUnderOverMode = (UnderOverMode)Enum.Parse(typeof(UnderOverMode), _settings.GetSetting("IPUnderOverMode", UnderOverMode.Fixed.ToString()));
                //BetsRefreshInterval = _settings.GetSetting ("BetsRefreshInterval", 3);
                IsCancelBets = _settings.GetSetting("IsCancelBets", false);
                IpIsSwopPrice = _settings.GetSetting("IPIsSwopPrice", false);
                PrIsSwopPrice = _settings.GetSetting("PRIsSwopPrice", false);
                PrIsNextBestPrice = _settings.GetSetting("PRIsNextBestPrice", false);
                IpIsNextBestPrice = _settings.GetSetting("IPIsNextBestPrice", false);
                PrIsDynamicLiability = _settings.GetSetting("IsDynamicLiability", true);
                IpIsDynamicLiability = _settings.GetSetting("IPIsDynamicLiability", true);
                IsPriceVirtualise = _settings.GetSetting("IsPriceVirtualise", true);
                BetfairUser = _settings.GetSetting("BetfairUser", string.Empty);
                if (!string.IsNullOrEmpty(BetfairUser))
                {
                    var decryptedPwd = CryptoString.Decrypt(_settings.GetSetting("BetfairPassword", string.Empty));
                    BetfairPassword = decryptedPwd;
                }
                BetfairNgKey = _settings.GetSetting("BetfairNgKey", "ykEM0nRo08oMkmCw");

                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("ReadSettings: " + ex);
                return ex.Message;
            }
        }
    }
}
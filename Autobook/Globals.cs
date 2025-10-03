using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using BetfairNgClient.Json;
using BetfairNgClient.Json.Enums;
using PthLog;

namespace Autobook
{
    public enum BetType
    {
        Back, Lay
    }

    public class Globals
    {
        public const int BETFAIR_EXCHANGE_UK = 1;
        public const int MAX_MARKET = 100;

        private static BetfairNgClient.BetfairNgExchange _exchange;
        private const int KEEP_ALIVE_TIMER = 19; //  19 minutes
        //private const int KeepAliveTimer = 1140000; // 1000 * 60 * 19 => 19 minutes
        //private const int KeepAliveTimer = 20000;
        public static readonly Color BackColor = Color.FromArgb(200, 208, 228);
        public static readonly Color LayColor = Color.FromArgb(228, 184, 215);
        public static readonly Color BackColorDisable = Color.AliceBlue;
        public static readonly Color LayColorDisable = Color.LavenderBlush;
        //public static readonly Color BACK_COLOUR_DARK = Color.FromArgb (70, 90, 142);
        //public static readonly Color LAY_COLOUR_DARK = Color.FromArgb (136, 51, 111);
        public static double MinBetAmount = 1.0D;
        public static double MaxPrice = 1000.0D;
        //public const int DisplayBetsRefreshRate = 2000;
        private const string LOG_DIR_NAME = @"logs/";
        public const int MAX_PLACE_BETS = 200;
        public static double UkWallet;
        //private static Timer timer= new Timer (TimerProc, null, KeepAliveTimer, KeepAliveTimer);
        private static Logger _logger;
        private static MarketCatalogue _market;
        //private static DateTime lastExchangeCall = DateTime.Now;
        public const string DOUBLE_FORMAT = "###0.00";
        private static readonly ManualResetEvent _stopKeepAliveHandling = new ManualResetEvent(false);
        private static Thread _keepAliveThread;

        public delegate void OnKeepAliveErrorDelegate(string errorMsg);
        public static event OnKeepAliveErrorDelegate OnKeepAliveError;
        private static Stopwatch _stopwatch;

        private Globals()
        {
        }

        public static string GetMarketLabel(DateTime startTime, string marketName)
        {
            return startTime.ToLocalTime().ToShortTimeString() + " " + marketName;
        }

        public static int ApiFailCount { get; set; }

        public static bool IsNetworkOnline { get; private set; }

        public static bool IsBetsPlaced(PlaceExecutionReport betsResults)
        {
            if (betsResults == null)
                return false;

            return (betsResults.Status == ExecutionReportStatusEnum.SUCCESS);
        }

        public static void SetLogger(Logger mainLogger)
        {
            _logger = mainLogger;
        }

        public static void SetMarket(MarketCatalogue mainMarket)
        {
            _market = mainMarket;
        }

        public static BetfairNgClient.BetfairNgExchange Exchange
        {
            get
            {
                if (_exchange == null)
                {
                    _exchange = new BetfairNgClient.BetfairNgExchange(AppSettings.BetfairNgKey);
                    _stopwatch = Stopwatch.StartNew();
                    _keepAliveThread = new Thread(KeepAliveProc)
                    {
                        IsBackground = true,
                        Name = "KeepAliveProc"
                    };
                    _keepAliveThread.Start();
                }
                // reset timer
                //if (timer != null)
                //  timer.Change (KeepAliveTimer, KeepAliveTimer);
                return _exchange;
            }
        }

        private static void KeepAliveProc(object state)
        {
            try
            {
                while (!_stopKeepAliveHandling.WaitOne(1000, false))
                {
                    if (_stopwatch.Elapsed.Minutes >= KEEP_ALIVE_TIMER)
                    {
                        _logger.Info($"Exchange: more than {_stopwatch.Elapsed.Minutes} idle minutes, calling keep alive");
                        if (_exchange == null)
                        {
                            _logger.Error("KeepAliveProc: exchange is null");
                            return;
                        }

                        if (_exchange.IsLoggedIn)
                        {
                            _exchange.KeepAlive();
                            CheckNetworkStatusIsConnected();
                        }
                        else
                            _logger.Error("KeepAliveProc: not logged in");

                        _stopwatch.Restart();
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    ApiFailCount++;
                    _logger.Error($"KeepAlive failed #{ApiFailCount}, {ex}");
                }
                IsNetworkOnline = false;
                OnKeepAliveError?.Invoke(ex.Message);
            }
            finally
            {
                _keepAliveThread = null;
            }
        }

        public static void CheckNetworkStatusIsConnected()
        {
            if (!IsNetworkOnline)
            {
                IsNetworkOnline = true;
                OnKeepAliveError?.Invoke(string.Empty);
            }
        }

        public static void UnhandledException(Exception e)
        {
            // TODO check exception returned from BF
            //if ((e is System.Net.WebException) ||
            //    (e is BetfairClient.Framework.ExchangeException) ||
            //    (e is BetfairClient.Framework.ExchangeThrottleException))
            if (e is System.Net.WebException)
            {
                ApiFailCount++;
                IsNetworkOnline = false;
            }
            else
                Console.WriteLine("UnhandledException: " + e);

            _logger.Error(e.ToString());
            OnKeepAliveError?.Invoke(e.Message);
        }

        public static class CryptoString
        {
            private static byte[] _savedKey;
            private static byte[] _savedIv;

            public static byte[] Key
            {
                get => _savedKey;
                set => _savedKey = value;
            }

            public static byte[] Iv
            {
                get => _savedIv;
                set => _savedIv = value;
            }

            private static void RdGenerateSecretKey(RijndaelManaged rdProvider)
            {
                if (_savedKey == null)
                {
                    rdProvider.KeySize = 256;
                    rdProvider.GenerateKey();
                    _savedKey = rdProvider.Key;
                }
            }

            private static void RdGenerateSecretInitVector(RijndaelManaged rdProvider)
            {
                if (_savedIv == null)
                {
                    rdProvider.GenerateIV();
                    _savedIv = rdProvider.IV;
                }
            }

            public static string Encrypt(string originalStr)
            {
                // Encode data string to be stored in memory
                byte[] originalStrAsBytes = Encoding.ASCII.GetBytes(originalStr);

                // Create MemoryStream to contain output
                var memStream = new MemoryStream(originalStrAsBytes.Length);

                var rijndael = new RijndaelManaged();

                // Generate and save secret key and init vector
                RdGenerateSecretKey(rijndael);
                RdGenerateSecretInitVector(rijndael);

                if (_savedKey == null || _savedIv == null)
                {
                    throw (new NullReferenceException(
                            "savedKey and savedIV must be non-null."));
                }

                // Create encryptor, and stream objects
                var rdTransform = rijndael.CreateEncryptor((byte[])_savedKey.Clone(), (byte[])_savedIv.Clone());
                var cryptoStream = new CryptoStream(memStream, rdTransform, CryptoStreamMode.Write);

                // Write encrypted data to the MemoryStream
                cryptoStream.Write(originalStrAsBytes, 0, originalStrAsBytes.Length);
                cryptoStream.FlushFinalBlock();
                var originalBytes = memStream.ToArray();

                // Release all resources
                memStream.Close();
                cryptoStream.Close();
                rdTransform.Dispose();
                rijndael.Clear();

                // Convert encrypted string
                var encryptedStr = Convert.ToBase64String(originalBytes);
                return (encryptedStr);
            }

            public static string Decrypt(string encryptedStr)
            {
                // Unconvert encrypted string
                var encryptedStrAsBytes = Convert.FromBase64String(encryptedStr);
                var initialText = new Byte[encryptedStrAsBytes.Length];

                var rijndael = new RijndaelManaged();
                var memStream = new MemoryStream(encryptedStrAsBytes);

                if (_savedKey == null || _savedIv == null)
                {
                    throw (new NullReferenceException("savedKey and savedIV must be non-null."));
                }

                // Create decryptor, and stream objects
                var rdTransform = rijndael.CreateDecryptor((byte[])_savedKey.Clone(), (byte[])_savedIv.Clone());
                var cryptoStream = new CryptoStream(memStream, rdTransform, CryptoStreamMode.Read);

                // Read in decrypted string as a byte[]
                cryptoStream.Read(initialText, 0, initialText.Length);

                // Release all resources
                memStream.Close();
                cryptoStream.Close();
                rdTransform.Dispose();
                rijndael.Clear();

                // Convert byte[] to string
                var decryptedStr = Encoding.ASCII.GetString(initialText);
                return (decryptedStr);
            }
        }

        public static void StopKeepAlive()
        {
            //timer.Dispose ();
            _stopwatch?.Stop();
            _stopKeepAliveHandling.Set();
            _keepAliveThread?.Join(1000);
        }

        public static double GetBookShare(double price)
        {
            return 1.0d / price;
        }

        public static double GetBookShareLiability(double price)
        {
            return 1.0d / (price - 1.0d);
        }

        public static double GetLayLiability(double price, double amount)
        {
            return (price - 1.0d) * amount;
        }

        public static bool GetMaxMinLiability(List<RunnerProfitAndLoss> pandls,
            out RunnerProfitAndLoss minLiabilityPAndL, out RunnerProfitAndLoss maxLiabilityPAndL, out bool isAnyLiability)
        {
            var minLiability = double.MaxValue;
            var maxLiabilty = double.MinValue;
            minLiabilityPAndL = null;
            maxLiabilityPAndL = null;
            isAnyLiability = false;

            if ((pandls == null) || (pandls.Count <= 0))
                return false;

            foreach (RunnerProfitAndLoss pandl in pandls)
            {
                if (!isAnyLiability)
                    if (pandl.IfWin != 0.0D)
                        isAnyLiability = true;

                if (pandl.IfWin < minLiability)
                {
                    minLiability = pandl.IfWin;
                    minLiabilityPAndL = pandl;
                }
                if (pandl.IfWin > maxLiabilty)
                {
                    maxLiabilty = pandl.IfWin;
                    maxLiabilityPAndL = pandl;
                }
            }

            return true;
        }

        /*
        public static MarketProfitAndLoss GetBiggestLiability(Dictionary<int, MarketProfitAndLoss>.ValueCollection pandls)
        {
          double minLiability = double.MaxValue;
          MarketProfitAndLoss result = null;

          if ((pandls == null) || (pandls.Count <= 0))
            return result;

          foreach (MarketProfitAndLoss pandl in pandls)
          {
            if (pandl.ifWin < minLiability)
            {
              minLiability = pandl.ifWin;
              result = pandl;
            }
          }

          return result;
        }
        public static bool IsMaxLiabilityReached (Dictionary<int, MarketProfitAndLoss>.ValueCollection pandls, double maxLiability)
        {
          foreach (MarketProfitAndLoss pandl in pandls)
          {
            if (pandl.ifWin < maxLiability)
            {
              logger.Info (string.Format ("IsMaxLiabilityReached: reached {0} < {1}",
                pandl.ifWin.ToString (DoubleFormat), maxLiability.ToString (DoubleFormat)));
              return true;
            }
          }

          return false;
        }
        */

        public static bool IsLiabilityExceeded(double runnerLiability, double maxLiability, bool isAllGreen, BetType betType)
        {
            // For Lay bets, liability is negative. More negative = worse.
            // e.g., maxLiability = -5, runnerLiability = -24 means we've exceeded by £19
            // So runnerLiability < maxLiability means exceeded (e.g., -24 < -5 = true)
            if (betType == BetType.Lay)
            {
                return runnerLiability < maxLiability;
            }

            // For Back bets (not currently used but keeping for completeness)
            return runnerLiability <= maxLiability;
        }

        //  public static double GetBackAmountTotal (PlaceBetParam param, int nbpIndex)
        //{
        //  double total = 0.0d;
        //  double backBook = 0.0d;
        //  double betAmount = 0.0d;
        //        //BetfairE.Price[] prices;
        //  foreach (Runner runner in param.runnerPrices)
        //  {
        //    backBook = GetBookShare (runner.BackPrice);
        //    betAmount = backBook * param.amount;
        //    total += betAmount;
        //  }

        //  return total;
        //}

        //public static double GetLayAmountTotal (PlaceBetParam param, LayBetMode lbm, int nbpIndex)
        //{
        //  double total = 0.0d;
        //  double layBook = 0.0d;
        //  double betAmount = 0.0d;
        //  foreach (Runner runner in param.runnerPrices)
        //  {
        //    switch (lbm)
        //    {
        //      case LayBetMode.Payout:
        //        layBook = GetBookShare (runner.LayPrice);
        //        break;

        //      case LayBetMode.Liability:
        //        layBook = GetBookShareLiability (runner.LayPrice);
        //        break;
        //    }

        //    betAmount = layBook * param.amount;
        //    total += betAmount;
        //  }

        //  return total;
        //}

        public static double GetNextBestBackPrice(double backPrice, bool isSwopPrice)
        {
            return backPrice;
        }

        public static double GetNextBestLayPrice(double backPrice, bool isSwopPrice)
        {
            return ((double)((isSwopPrice)
              ? IncrementPrice((decimal)backPrice)
              : IncrementPrice((decimal)backPrice)));
        }

        #region Price Increment/Decrement

        // Acceptable Odds
        //
        // All Betfair bets must be placed at 'acceptable' Odds increments. This
        // ensures that the market remains disciplined. For example, if you would
        // like to leave an order on the system, you can only change the current
        // Odds by the increment, or multiple of the increment, shown below.
        //
        // From   To 	Increment
        // 1      2     0.01
        // 2.02   3     0.02
        // 3.05   4     0.05
        // 4.1    6     0.1
        // 6.2    10    0.2
        // 10.5   20    0.5
        // 21     30    1
        // 32     50    2
        // 55     100   5
        // 110    1000  10
        // 1000+  Not Allowed
        //
        // The odds increment on Asian Handicap markets is 0.01 for all odds ranges.

        public const decimal MINIMUM_PRICE = 1.01m;
        public const decimal MAXIMUM_PRICE = 1000m;


        public static decimal IncrementPrice(decimal price)
        {
            if (!IsPipPrice(price, out var remainder))
            {
                price = Round(price - remainder, 2);
            }

            if (price < 1.01m) return price;
            if (price < 2.00m) return price + 0.01m;
            if (price < 3.00m) return price + 0.02m;
            if (price < 4.00m) return price + 0.05m;
            if (price < 6.00m) return price + 0.10m;
            if (price < 10.00m) return price + 0.20m;
            if (price < 20.00m) return price + 0.50m;
            if (price < 30.00m) return price + 1.00m;
            if (price < 50.00m) return price + 2.00m;
            if (price < 100.00m) return price + 5.00m;
            if (price < 1000.00m) return price + 10.00m;
            return price;
        }

        public static decimal IncrementPrice(decimal price, int pips)
        {
            if (pips == int.MaxValue)
            {
                return MAXIMUM_PRICE;
            }

            if (pips == 0)
            {
                return price;
            }

            if (pips > 1)
            {
                return IncrementPrice(IncrementPrice(price), pips - 1);
            }

            return IncrementPrice(price);
        }

        public static decimal DecrementPrice(decimal price)
        {
            if (!IsPipPrice(price, out var remainder))
            {
                return Round(price - remainder, 2);
            }

            if (price <= 1.01m) return price;
            if (price <= 2.00m) return price - 0.01m;
            if (price <= 3.00m) return price - 0.02m;
            if (price <= 4.00m) return price - 0.05m;
            if (price <= 6.00m) return price - 0.10m;
            if (price <= 10.00m) return price - 0.20m;
            if (price <= 20.00m) return price - 0.50m;
            if (price <= 30.00m) return price - 1.00m;
            if (price <= 50.00m) return price - 2.00m;
            if (price <= 100.00m) return price - 5.00m;
            if (price <= 1000.00m) return price - 10.00m;
            return price;
        }

        public static decimal DecrementPrice(decimal price, int pips)
        {
            if (pips == int.MaxValue)
            {
                return MINIMUM_PRICE;
            }

            if (pips == 0)
            {
                return price;
            }

            if (pips > 1)
            {
                return DecrementPrice(DecrementPrice(price), pips - 1);
            }

            return DecrementPrice(price);
        }

        public static bool IsValidPrice(decimal price)
        {
            return ((price >= 1.01m) && (price <= 1000.00m));
        }

        public static bool IsPipPrice(decimal price)
        {
            return IsPipPrice(price, out _);
        }

        public static bool IsPipPrice(decimal price, out decimal remainder)
        {
            remainder = 0m;
            if (price < 1.01m) return false;
            if (price < 2.00m) return ((remainder = (price % 0.01m)) == 0m);
            if (price < 3.00m) return ((remainder = (price % 0.02m)) == 0m);
            if (price < 4.00m) return ((remainder = (price % 0.05m)) == 0m);
            if (price < 6.00m) return ((remainder = (price % 0.10m)) == 0m);
            if (price < 10.00m) return ((remainder = (price % 0.20m)) == 0m);
            if (price < 20.00m) return ((remainder = (price % 0.50m)) == 0m);
            if (price < 30.00m) return ((remainder = (price % 1.00m)) == 0m);
            if (price < 50.00m) return ((remainder = (price % 2.00m)) == 0m);
            if (price < 100.00m) return ((remainder = (price % 5.00m)) == 0m);
            if (price <= 1000.00m) return ((remainder = (price % 10.00m)) == 0m);
            return false;
        }

        public static decimal ClosestPrice(decimal price)
        {
            if (!IsPipPrice(price, out var remainder))
            {
                if (price < 1.01m) return price;
                if (price < 2.00m) return (remainder < 0.005m) ? Round(price - remainder, 2) : Round(price + 0.01m - remainder, 2);
                if (price < 3.00m) return (remainder < 0.010m) ? Round(price - remainder, 2) : Round(price + 0.02m - remainder, 2);
                if (price < 4.00m) return (remainder < 0.025m) ? Round(price - remainder, 2) : Round(price + 0.05m - remainder, 2);
                if (price < 6.00m) return (remainder < 0.050m) ? Round(price - remainder, 1) : Round(price + 0.10m - remainder, 1);
                if (price < 10.00m) return (remainder < 0.100m) ? Round(price - remainder, 1) : Round(price + 0.20m - remainder, 1);
                if (price < 20.00m) return (remainder < 0.250m) ? Round(price - remainder, 1) : Round(price + 0.50m - remainder, 1);
                if (price < 30.00m) return (remainder < 0.500m) ? Round(price - remainder, 0) : Round(price + 1.00m - remainder, 0);
                if (price < 50.00m) return (remainder < 1.000m) ? Round(price - remainder, 0) : Round(price + 2.00m - remainder, 0);
                if (price < 100.00m) return (remainder < 2.500m) ? Round(price - remainder, 0) : Round(price + 5.00m - remainder, 0);
                if (price < 1000.00m) return (remainder < 5.000m) ? Round(price - remainder, 0) : Round(price + 10.00m - remainder, 0);
            }
            return price;
        }

        public static int PriceDecimals(decimal price)
        {
            if (price < 4.00m) return 2;
            if (price < 20.00m) return 1;
            return 0;
        }

        public static int PricePips(decimal firstPrice, decimal secondPrice)
        {
            if (!IsValidPrice(firstPrice) || !IsValidPrice(secondPrice)) return 0;

            int pips = 0;
            if (firstPrice > secondPrice)
            {
                while (Round(firstPrice, 2) > Round(secondPrice, 2))
                {
                    pips++;
                    firstPrice = DecrementPrice(firstPrice);
                }
            }
            else
            {
                while (Round(firstPrice, 2) < Round(secondPrice, 2))
                {
                    pips++;
                    firstPrice = IncrementPrice(firstPrice);
                }
            }
            return pips;
        }

        public static decimal MidpointPrice(decimal firstPrice, decimal secondPrice)
        {
            var pips = PricePips(firstPrice, secondPrice);
            var lowerPrice = Math.Min(firstPrice, secondPrice);
            var floorPrice = IncrementPrice(lowerPrice, (int)Math.Floor(pips / 2m));
            var ceilingPrice = IncrementPrice(lowerPrice, (int)Math.Ceiling(pips / 2m));
            return Round((floorPrice + ceilingPrice) / 2m, 4);
        }

        #endregion Price Increment/Decrement

        #region Rounding

        public static decimal Round(decimal number)
        {
            return decimal.Round(number, 2);
        }

        public static decimal Round(decimal number, int decimals)
        {
            return decimal.Round(number, decimals);
        }

        public static double Round(double number)
        {
            return (double)decimal.Round((decimal)number, 2);
        }

        public static double Round(double number, int decimals)
        {
            return (double)decimal.Round((decimal)number, decimals);
        }

        #endregion
        public static string MakeLogName(string name)
        {
            CreateLogsDir();
            var result = LOG_DIR_NAME + DateTime.Now.ToString("yyyyMMdd_HHmmssfff_") + name.Replace("?", "");
            return result;
        }

        public static string MakeMainLogName(string name)
        {
            CreateLogsDir();
            var result = LOG_DIR_NAME + name;
            return result;
        }

        private static void CreateLogsDir()
        {
            if (!Directory.Exists(LOG_DIR_NAME))
            {
                Directory.CreateDirectory(LOG_DIR_NAME);
            }
        }

        public static double GetTotalImpliedLiability(long selectionId, CurrentOrderSummaryReport unmatchedBets)
        {
            var liabilityTotal = 0.0D;

            foreach (var bet in unmatchedBets.CurrentOrders)
            {
                switch (bet.Side)
                {
                    case SideEnum.BACK:
                        if (bet.SelectionId != selectionId)
                            liabilityTotal += bet.PriceSize.Size;
                        break;

                    case SideEnum.LAY:
                        if (bet.SelectionId == selectionId)
                            liabilityTotal += GetLayLiability(bet.PriceSize.Price, bet.PriceSize.Size);
                        break;
                }
                _logger.Info($"GetTotalImpliedLiability: unmatched bet {bet.PriceSize.Price}@{bet.PriceSize.Size} {bet.SelectionId}, current implied liability = {liabilityTotal.ToString(DOUBLE_FORMAT)}");
            }

            _logger.Info($"GetTotalImpliedLiability: {selectionId}, total implied liability = {liabilityTotal.ToString(DOUBLE_FORMAT)}");
            return liabilityTotal;
        }

        internal static double GetImpliedLiability(double impliedLiability, double impliedLiabilityPercent, double pandl)
        {
            return (impliedLiability + (pandl * (impliedLiabilityPercent / 100.0D)));
        }
    }
}
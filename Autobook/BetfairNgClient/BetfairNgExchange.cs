using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;
using BetfairNgClient.Json;
using System.Linq;
using BetfairNgClient.Json.Enums;
using BetfairNgClient.Json.LiveTennis;
using System.Diagnostics;

namespace BetfairNgClient
{
    internal enum EndPointEnum
    {
        Betting,
        Account,
        LiveTennis,
        RaceStatus
    }

    public class BetfairNgExchange : HttpWebClientProtocol
    {
        //private const string betfairNgKey = "JYheFAFiOMVSIYEw";
        public const int BetfairExchangeUk = 1;
        private const int maxPlaceBets = 200;

        #region End Points
        private const string loginEndPoint = "https://identitysso.betfair.com/api";
        private const string ukAccountEndPoint = "https://api.betfair.com/exchange/account/json-rpc/v1";
        private const string auAccountEndPoint = "https://api-au.betfair.com/exchange/account/json-rpc/v1";
        private const string ukBettingEndPoint = "https://api.betfair.com/exchange/betting/json-rpc/v1";
        private const string auBettingEndPoint = "https://api-au.betfair.com/exchange/betting/json-rpc/v1";
        private const string eventsMenuEndPoint = "https://api.betfair.com/exchange/betting/rest/v1/en/navigation/menu.json";
            //"https://d1zgsxlgpxt59q.cloudfront.net/exchange/betting/rest/v1/en/navigation/lhm.json";
        private const string liveTennisEndPoint = "https://api.betfair.com/exchange/scores/json-rpc/v1";
        private const string raceStatusEndPoint = "https://api.betfair.com/exchange/scores/json-rpc/v1";
        #endregion End Points

        #region Headers constants
        private const string appkeyHeader = "X-Application";
        private const string sessionTokenHeader = "X-Authentication";
        private const string filterHeader = "filter";
        private const string marketProjectionHeader = "marketProjection";
        private const string sortHeader = "sort";
        private const string maxResultsHeader = "maxResults";
        private const string marketIdsHeader = "marketIds";
        private const string includeSettledBetsHeader = "includeSettledBets";
        private const string includeBspBetsHeader = "includeBspBets";
        private const string netOfCommissionHeader = "netOfCommission";
        private const string priceProjectionHeader = "priceProjection";
        private const string orderProjectionHeader = "orderProjection";
        private const string matchProjectionHeader = "matchProjection";
        private const string marketIdHeader = "marketId";
        private const string instructionsHeader = "instructions";
        private const string customerReferenceHeader = "customerRef";
        private const string betIdsHeader = "betIds";
        private const string betStatusHeader = "betStatus";
        private const string placedDateRangeHeader = "placedDateRange";
        private const string dateRangeHeader = "dateRange";
        private const string orderByHeader = "orderBy";
        private const string sortDirHeader = "sortDir";
        private const string fromRecordHeader = "fromRecord";
        private const string recordCountHeader = "recordCount";
        private const string updateKeyHeader = "updateKeys";
        private const string eventIdHeader = "eventIds";
        private const string eventTypeIdHeader = "eventTypeIds";
        private const string eventStatusHeader = "eventStatus";
        private const string runnerIdsHeader = "runnerIds";
        private const string sideHeader = "side";
        private const string settledDateRangeHeader = "settledDateRange";
        private const string groupByHeader = "groupBy";
        private const string includeItemDescriptionHeader = "includeItemDescription";
        private const string localeHeader = "locale";
        private const string meetingIdsHeader = "meetingIds";
        private const string raceIdsHeader = "raceIds";
        #endregion Headers constants

        #region Method constants
        private const string getAccountFundsMethod = "AccountAPING/v1.0/getAccountFunds";
        private const string getAccountDetailsMethod = "AccountAPING/v1.0/getAccountDetails";
        private const string getAccountStatement = "AccountAPING/v1.0/getAccountStatement";

        private const string listEventsMethod = "SportsAPING/v1.0/listEvents";
        private const string listEventTypesMethod = "SportsAPING/v1.0/listEventTypes";
        private const string listMarketCatalogueMethod = "SportsAPING/v1.0/listMarketCatalogue";
        private const string listMarketProfitAndLoss = "SportsAPING/v1.0/listMarketProfitAndLoss";
        private const string listMarketBookMethod = "SportsAPING/v1.0/listMarketBook";
        private const string cancelOrdersMethod = "SportsAPING/v1.0/cancelOrders";
        private const string placeOrdersMethod = "SportsAPING/v1.0/placeOrders";
        private const string replaceOrdersMethod = "SportsAPING/v1.0/replaceOrders";
        private const string listCurrentOrdersMethod = "SportsAPING/v1.0/listCurrentOrders";
        private const string listClearedOrdersMethod = "SportsAPING/v1.0/listClearedOrders";

        private const string listAvailableEventsMethod = "ScoresAPING/v1.0/listAvailableEvents";
        private const string listScoresMethod = "ScoresAPING/v1.0/listScores";

        private const string listRaceDetailsMethod = "ScoresAPING/v1.0/listRaceDetails";

        private const string keepAliveMethod = "keepAlive";
        #endregion Method constants

        private NameValueCollection CustomHeaders { get; set; }
        private readonly string[] accountEndPoints = {ukAccountEndPoint, auAccountEndPoint};
        private readonly string[] bettingEndPoints = {ukBettingEndPoint, auBettingEndPoint};
        private string sessionToken;
        private AccountDetailsResponse accountDetailsResponse;

        #region Properties

        public bool IsLoggedIn { get; private set; }

        #endregion Properties

        public BetfairNgExchange(string betfairNgKey)
        {
            if (string.IsNullOrEmpty(betfairNgKey))
                Debug.Print("BetfairNgExchange:: error, betfairNgKey is not set.");
                //throw new ArgumentNullException(nameof(betfairNgKey));

            CustomHeaders = new NameValueCollection();
            CustomHeaders[appkeyHeader] = betfairNgKey;
        }

        protected WebRequest CreateWebRequest(string endPoint, string method = "POST",
                                              bool isAutomaticDecompression = false)
        {
            var request = (HttpWebRequest) WebRequest.Create(endPoint);
            request.Method = method;
            request.Accept = "application/json";
            // From the week commencing 20th July we are making a change to the Betfair API so that
            // it responds to the HTTP header “Expect: 100-continue” with an HTTP1.1 compliant response
            // of "417 Expectation Failed”. 
            request.ServicePoint.Expect100Continue = false;
            if (isAutomaticDecompression)
                request.AutomaticDecompression = DecompressionMethods.GZip;
            request.ContentType = "application/json-rpc; charset=UTF-8";
            //request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8");
            request.Headers.Add(CustomHeaders);
            return request;
        }

        private T Invoke<T>(string method, int exchangeId, EndPointEnum endPointEnum,
            IDictionary<string, object> args = null)
        {
            if (string.IsNullOrEmpty( method ))
                throw new ArgumentException(nameof(method));

            string url;
            switch (endPointEnum)
            {
                case EndPointEnum.Account:
                    url = accountEndPoints[exchangeId - 1];
                    break;

                case EndPointEnum.Betting:
                    url = bettingEndPoints[exchangeId - 1];
                    break;

                case EndPointEnum.LiveTennis:
                    url = liveTennisEndPoint;
                    break;

                case EndPointEnum.RaceStatus:
                    url = raceStatusEndPoint;
                    break;

                default:
                    throw new Exception("Invalid EndPointEnum: " + endPointEnum);
            }

            var request = CreateWebRequest(url);

            using (Stream stream = request.GetRequestStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var call = new JsonRequest {Method = method, Id = 1, Params = args};
                JsonConvert.Export(call, writer);
            }

            using (WebResponse response = GetWebResponse(request))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var jsonResponse = JsonConvert.Import<T>(reader);
                //Console.WriteLine("\nGot Response: " + JsonConvert.Serialize(jsonResponse));
                if (jsonResponse.HasError)
                {
                    throw new Exception($"Error: ({jsonResponse.Error.Code}) : {jsonResponse.Error.Message}");
                }

                return jsonResponse.Result;
            }
        }

        private static Exception ReconstituteException(Exception ex)
        {
            var data = ex.Data;

            // API-NG exception -- it must have "data" element to tell us which exception
            //var exceptionName = data.Property("exceptionname").Value.ToString();
            //var exceptionData = data.Property(exceptionName).Value.ToString();

            return ex;
        }

        #region Properties
        public string Currency
        {
            get
            {
                if (accountDetailsResponse == null)
                {
                    accountDetailsResponse = GetAccountDetails(BetfairExchangeUk);
                }

                return accountDetailsResponse.CurrencyCode;
            }
        }

        public string PointsBalance
        {
            get
            {
                if (accountDetailsResponse == null)
                {
                    accountDetailsResponse = GetAccountDetails(BetfairExchangeUk);
                }

                return accountDetailsResponse.PointsBalance.ToString();
            }
        }
        #endregion Properties

        #region Account Methods
        public AccountFundsResponse GetAccountFunds(int exchangeId)
        {
            var accountFunds = Invoke<AccountFundsResponse>(getAccountFundsMethod, exchangeId, EndPointEnum.Account);

            return accountFunds;
        }

        public AccountDetailsResponse GetAccountDetails(int exchangeId)
        {
            return Invoke<AccountDetailsResponse>(getAccountDetailsMethod, exchangeId, EndPointEnum.Account);
        }

        public AccountStatementReport GetAccountStatement(int exchangeId)
        {
            return Invoke<AccountStatementReport>(getAccountStatement, exchangeId, EndPointEnum.Account);
        }

    #endregion Account Methods

    #region Betting Methods
    public List<EventResult> ListEvents(int exchangeId, MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[filterHeader] = marketFilter;
            return Invoke<List<EventResult>>(listEventsMethod, exchangeId, EndPointEnum.Betting, args);
        }

        public List<EventTypeResult> ListEventTypes(int exchangeId, MarketFilter marketFilter)
        {
            var args = new Dictionary<string, object>();
            args[filterHeader] = marketFilter;
            return Invoke<List<EventTypeResult>>(listEventTypesMethod, exchangeId, EndPointEnum.Betting, args);
        }

        public List<MarketCatalogue> ListMarketCatalogue(
            int exchangeId,
            MarketFilter marketFilter,
            ISet<MarketProjectionEnum> marketProjections = null,
            MarketSortEnum? marketSort = null,
            int maxResult = 1)
        {
            var args = new Dictionary<string, object>();
            args[filterHeader] = marketFilter;
            args[marketProjectionHeader] = marketProjections;
            args[sortHeader] = marketSort;
            args[maxResultsHeader] = maxResult;
            return Invoke<List<MarketCatalogue>>(listMarketCatalogueMethod, exchangeId, EndPointEnum.Betting, args);
        }

        //public List<MarketProfitAndLoss> ListMarketProfitAndLoss(
        //    int exchangeId,
        //    ISet<string> marketIds,
        //    bool includeSettledBets = false,
        //    bool includeBsbBets = false,
        //    bool netOfCommission = false)
        //{
        //    var args = new Dictionary<string, object>();
        //    args[BetfairNgExchange.marketIds] = marketIds;
        //    args[BetfairNgExchange.includeSettledBets] = includeSettledBets;
        //    args[includeBspBets] = includeBsbBets;
        //    args[BetfairNgExchange.netOfCommission] = netOfCommission;
        //    return Invoke<List<MarketProfitAndLoss>>(listMarketProfitAndLoss, exchangeId, EndPointEnum.Betting, args);
        //}

        public MarketProfitAndLoss GetMarketProfitAndLoss(
            int exchangeId,
            string marketId,
            bool includeSettledBets=true,
            bool includeBsbBets=true,
            bool netOfCommission=true)
        {
            var mid = new HashSet<string> { marketId };
            var args = new Dictionary<string, object>();
            args[marketIdsHeader] = mid;
            args[includeSettledBetsHeader] = includeSettledBets;
            args[includeBspBetsHeader] = includeBsbBets;
            args[netOfCommissionHeader] = netOfCommission;

            var pandls = Invoke<List<MarketProfitAndLoss>>(listMarketProfitAndLoss, exchangeId, EndPointEnum.Betting, args);

            return pandls.Single(pl => pl.MarketId == marketId);
        }

        //public List<MarketBook> ListMarketBook(
        //    int exchangeId,
        //    IEnumerable<string> marketIds,
        //    PriceProjection priceProjection = null,
        //    OrderProjectionEnum? orderProjection = null,
        //    MatchProjectionEnum? matchProjection = null)
        //{
        //    var args = new Dictionary<string, object>();
        //    args[BetfairNgExchange.marketIds] = marketIds;
        //    args[BetfairNgExchange.priceProjection] = priceProjection;
        //    args[BetfairNgExchange.orderProjection] = orderProjection;
        //    args[BetfairNgExchange.matchProjection] = matchProjection;
        //    return Invoke<List<MarketBook>>(listMarketBookMethod, exchangeId, EndPointEnum.Betting, args);
        //}

        public MarketBook ListMarketBook(
            int exchangeId,
            string marketId,
            PriceProjection priceProjection = null,
            OrderProjectionEnum? orderProjection = null,
            MatchProjectionEnum? matchProjection = null)
        {
            var mid = new HashSet<string> { marketId };
            var args = new Dictionary<string, object>();
            args[marketIdsHeader] = mid;
            args[priceProjectionHeader] = priceProjection;
            args[orderProjectionHeader] = orderProjection;
            args[matchProjectionHeader] = matchProjection;
            var mbs = Invoke<List<MarketBook>>(listMarketBookMethod, exchangeId, EndPointEnum.Betting, args);

            return mbs.Single(mb => mb.MarketId == marketId);
        }

        //public CurrentOrderSummaryReport ListCurrentOrders(
        //    int exchangeId,
        //    ISet<string> betIds = null,
        //    ISet<string> marketIds = null,
        //    OrderProjectionEnum? orderProjection = null,
        //    TimeRange placedDateRange = null,
        //    TimeRange dateRange = null,
        //    OrderByEnum? orderBy = null,
        //    SortDir? sortDir = null,
        //    int? fromRecord = null,
        //    int? recordCount = null)
        //{
        //    var args = new Dictionary<string, object>();
        //    args[BetfairNgExchange.betIds] = betIds;
        //    args[BetfairNgExchange.marketIds] = marketIds;
        //    args[BetfairNgExchange.orderProjection] = orderProjection;
        //    args[BetfairNgExchange.placedDateRange] = placedDateRange;
        //    args[BetfairNgExchange.dateRange] = dateRange;
        //    args[BetfairNgExchange.orderBy] = orderBy;
        //    args[BetfairNgExchange.sortDir] = sortDir;
        //    args[BetfairNgExchange.fromRecord] = fromRecord;
        //    args[BetfairNgExchange.recordCount] = recordCount;
        //    return Invoke<CurrentOrderSummaryReport>(listCurrentOrdersMethod, exchangeId, EndPointEnum.Betting, args);
        //}

        public CurrentOrderSummaryReport ListCurrentOrders(
    int exchangeId,
    ISet<string> betIds = null,
    string marketId = null,
    OrderProjectionEnum? orderProjection = null,
    TimeRange placedDateRange = null,
    TimeRange dateRange = null,
    OrderByEnum? orderBy = null,
    SortDir? sortDir = null,
    int? fromRecord = null,
    int? recordCount = null)
        {
            var mid = new HashSet<string>();
            if (!string.IsNullOrEmpty(marketId))
                mid.Add(marketId);

            var args = new Dictionary<string, object>();
            args[betIdsHeader] = betIds;
            args[marketIdsHeader] = mid;
            args[orderProjectionHeader] = orderProjection;
            args[placedDateRangeHeader] = placedDateRange;
            args[dateRangeHeader] = dateRange;
            args[orderByHeader] = orderBy;
            args[sortDirHeader] = sortDir;
            args[fromRecordHeader] = fromRecord;
            args[recordCountHeader] = recordCount;
            return Invoke<CurrentOrderSummaryReport>(listCurrentOrdersMethod, exchangeId, EndPointEnum.Betting, args);
        }

        public ClearedOrderSummaryReport listClearedOrders(int exchangeId, BetStatusEnum betStatus,
            ISet<string> eventTypeIds = null, ISet<string> eventIds = null, ISet<string> marketIds = null,
            ISet<RunnerId> runnerIds = null, ISet<string> betIds = null, SideEnum? side = null,
            TimeRange settledDateRange = null, GroupByEnum? groupBy = null, bool? includeItemDescription = null,
            String locale = null, int? fromRecord = null, int? recordCount = null)
        {
            var args = new Dictionary<string, object>();
            args[betStatusHeader] = betStatus;
            args[eventTypeIdHeader] = eventTypeIds;
            args[eventIdHeader] = eventIds;
            args[marketIdHeader] = marketIds;
            args[runnerIdsHeader] = runnerIds;
            args[betIdsHeader] = betIds;
            args[sideHeader] = side;
            args[settledDateRangeHeader] = settledDateRange;
            args[groupByHeader] = groupBy;
            args[includeItemDescriptionHeader] = includeItemDescription;
            args[localeHeader] = locale;
            args[fromRecordHeader] = fromRecord;
            args[recordCountHeader] = recordCount;

            return Invoke<ClearedOrderSummaryReport>(listClearedOrdersMethod, exchangeId, EndPointEnum.Betting, args);
        }

        public CancelExecutionReport CancelOrders(
            int exchangeId,
            string marketId = null,
            IList<CancelInstruction> instructions = null,
            string customerRef = null)
        {
            var args = new Dictionary<string, object>();

            args[instructionsHeader] = instructions;
            args[marketIdHeader] = marketId;
            args[customerReferenceHeader] = customerRef;

            return Invoke<CancelExecutionReport>(cancelOrdersMethod, exchangeId, EndPointEnum.Betting, args);
        }

        public PlaceExecutionReport PlaceOrders(
            int exchangeId,
            string marketId,
            IList<PlaceInstruction> placeInstructions,
            string customerRef = null)
        {
            PlaceExecutionReport result;
            var args = new Dictionary<string, object>();

            args[marketIdHeader] = marketId;
            args[customerReferenceHeader] = customerRef;

            if (placeInstructions.Count > maxPlaceBets)
            {
                int index = 0;
                int left = placeInstructions.Count;
                int count = Math.Min(maxPlaceBets, left);
                var sourceArray = placeInstructions.ToArray();

                do
                {
                    //if (!isStarted)
                    //{
                    //    logger.Info("PlaceBet: abort bets placement, thread stopped");
                    //    return;
                    //}

                    var pis = new PlaceInstruction[count];
                    Array.Copy(sourceArray, index, pis, 0, count);
                    args[instructionsHeader] = pis;
#if !NOBET
                    result = Invoke<PlaceExecutionReport>(placeOrdersMethod, exchangeId, EndPointEnum.Betting, args);
                    if (result.Status != ExecutionReportStatusEnum.SUCCESS)
                    {
                        return result;
                    }
#endif
                    left -= maxPlaceBets;
                    if (left <= 0)
                        break;
                    index += count;
                    count = Math.Min(maxPlaceBets, left);
                } while (true);
            }
            else
            {
                args[instructionsHeader] = placeInstructions;
                result = Invoke<PlaceExecutionReport>(placeOrdersMethod, exchangeId, EndPointEnum.Betting, args);
            }

            return result;
        }

        public ReplaceExecutionReport ReplaceOrders(
            int exchangeId,
            string marketId,
            IList<ReplaceInstruction> replaceInstructions,
            string customerRef = null)
        {
            var args = new Dictionary<string, object>();

            args[marketIdHeader] = marketId;
            args[instructionsHeader] = replaceInstructions;
            args[customerReferenceHeader] = customerRef;

            return Invoke<ReplaceExecutionReport>(replaceOrdersMethod, exchangeId, EndPointEnum.Betting, args);
        }

        #endregion Betting Methods

        #region Login Methods
        public void KeepAlive()
        {
            var request = CreateWebRequest(loginEndPoint + "/" + keepAliveMethod);

            using (WebResponse response = GetWebResponse(request))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var jsonResponse = JsonConvert.Import(reader);
                if (jsonResponse.HasError)
                {
                    throw new Exception(jsonResponse.Error);
                }

                if (jsonResponse.Status != "SUCCESS")
                {
                    throw new Exception(String.Format("KeepAlive failed {0}:{1}",
                                                      jsonResponse.Status, jsonResponse.Error));
                }
            }
        }

        public string Login(string name, string pwd)
        {
            IsLoggedIn = false;
            var request = CreateWebRequest (loginEndPoint + $"/login?username={name}&password={pwd}");

            using (var response = GetWebResponse(request))
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var jsonResponse = JsonConvert.Import(reader);
                if (jsonResponse.HasError)
                {
                    return $"Login failed, {jsonResponse.Error}";
                }

                if (jsonResponse.Status == "SUCCESS")
                {
                    sessionToken = jsonResponse.Token;
                    IsLoggedIn = true;
                    CustomHeaders[sessionTokenHeader] = sessionToken;
                }
                else
                {
                    return $"Login failed {jsonResponse.Status}:{jsonResponse.Error}";
                }
            }

            return string.Empty;
        }

        #endregion Login Methods

        #region Events Menu
        public EventsMenuResponse GetEventsMenu()
        {
            var request = CreateWebRequest(eventsMenuEndPoint, "GET", true);

            using (WebResponse response = GetWebResponse(request))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var jsonResponse = JsonConvert.ImportMenu(reader);

                return jsonResponse;
            }
        }
        #endregion Events Menu

        #region Live Tennis
        public List<Score> ListScores (int exchangeId, List<UpdateKey> updateKeys)
        {
            var args = new Dictionary<string, object>();
            args[updateKeyHeader] = updateKeys;

            return Invoke<List<Score>>(listScoresMethod, exchangeId, EndPointEnum.LiveTennis, args);
        }

        public List<AvailableEvent> ListAvailableEvents(int exchangeId, ISet<string> eventIds, ISet<string> eventTypeIds, ISet<string> eventStatuses)
        {
            var args = new Dictionary<string, object>();
            args[eventIdHeader] = eventIds;
            args[eventTypeIdHeader] = eventTypeIds;
            args[eventStatusHeader] = eventStatuses;

            return Invoke<List<AvailableEvent>>(listAvailableEventsMethod, exchangeId, EndPointEnum.LiveTennis, args);
        }
        #endregion // Live Tennis

        #region Race Status

        public List<RaceDetails> ListRaceDetails(ISet<string> meetingIds = null, ISet<string> raceIds = null)
        {
            var args = new Dictionary<string, object>();
            args[meetingIdsHeader] = meetingIds;
            args[raceIdsHeader] = raceIds;

            return Invoke<List<RaceDetails>>(listRaceDetailsMethod, BetfairExchangeUk, EndPointEnum.RaceStatus, args);
        }

        #endregion Race Status

        public Runner FindRunner(MarketBook marketBook, long selectionId)
        {
            if (marketBook == null)
            {
                throw new Exception("FindRunner: invalid market");
            }
            foreach (var runner in marketBook.Runners)
            {
                if (runner.SelectionId == selectionId)
                {
                    return runner;
                }
            }

            throw new Exception("FindRunner: cannot find selection " + selectionId);
        }
    }
}

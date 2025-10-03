using System.Text;
using BetfairNgClient.Json.Enums;
using Newtonsoft.Json;

namespace BetfairNgClient.Json
{
    public class PlaceInstruction
    {
        [JsonProperty(PropertyName = "orderType")]
        public OrderTypeEnum OrderType { get; set; }

        [JsonProperty(PropertyName = "selectionId")]
        public long SelectionId { get; set; }

        [JsonProperty(PropertyName = "handicap")]
        public double? Handicap { get; set; }

        [JsonProperty(PropertyName = "side")]
        public SideEnum Side { get; set; }

        [JsonProperty(PropertyName = "limitOrder")]
        public LimitOrder LimitOrder { get; set; }

        [JsonProperty(PropertyName = "limitOnCloseOrder")]
        public LimitOnCloseOrder LimitOnCloseOrder { get; set; }

        [JsonProperty(PropertyName = "marketOnCloseOrder")]
        public MarketOnCloseOrder MarketOnCloseOrder { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder()
                .AppendFormat("OrderType={0}", OrderType)
                .AppendFormat(" : SelectionId={0}", SelectionId)
                .AppendFormat(" : Handicap={0}", Handicap)
                .AppendFormat(" : Side={0}", Side);

            switch (OrderType)
            {
                case OrderTypeEnum.LIMIT:
                    sb.AppendFormat(" : LimitOrder={0}", LimitOrder);
                    break;
                case OrderTypeEnum.LIMIT_ON_CLOSE:
                    sb.AppendFormat(" : LimitOnCloseOrder={0}", LimitOnCloseOrder);
                    break;
                case OrderTypeEnum.MARKET_ON_CLOSE:
                    sb.AppendFormat(" : MarketOnCloseOrder={0}", MarketOnCloseOrder);
                    break;
            }

            return sb.ToString();
        }
    }
}

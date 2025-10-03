using System;
using BetfairNgClient.Json;

namespace Autobook
{
    public enum NodeType
    {
        Root,
        Market,
        Event,
        EventMarket2,
        EventType,
        Group,
        Race,
        Selection,
        Country
    }

    public interface INodeTag
    {
        NodeType Type { get; }
        string Id { get; set; }
        string Name { get; set; }
    }

    public class RootNodeTag : INodeTag
    {
        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.Root; }
        }

        public string Id { get; set; }
        public string Name { get; set; }

        #endregion
    }

    public class EventMarket2NodeTag : INodeTag
    {
        public EventMarket2NodeTag(string id, string name)
        {
            Id = id;
            Name = name;
        }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.EventMarket2; }
        }

        public string Id { get; set; }
        public string Name { get; set; }

        #endregion
    }


    public class EventNodeTag : INodeTag
    {
        public EventNodeTag(EventsMenuResponse eventsMenuResponse)
        {
            EventsMenuResponse = eventsMenuResponse;
            Id = eventsMenuResponse.Id;
            Name = eventsMenuResponse.Name;
            CountryCode = eventsMenuResponse.CountryCode;
        }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.Event; }
        }

        public EventsMenuResponse EventsMenuResponse { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; private set; }

        #endregion
    }

    public class EventTypeNodeTag : INodeTag
    {
        public EventTypeNodeTag(EventsMenuResponse eventsMenuResponse)
        {
            EventsMenuResponse = eventsMenuResponse;
            Id = eventsMenuResponse.Id;
            Name = eventsMenuResponse.Name;
        }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.EventType; }
        }

        public EventsMenuResponse EventsMenuResponse { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }

        #endregion
    }

    public class GroupNodeTag:INodeTag
    {
        public GroupNodeTag(EventsMenuResponse eventsMenuResponse)
        {
            EventsMenuResponse = eventsMenuResponse;
            Id = eventsMenuResponse.Id;
            Name = eventsMenuResponse.Name;
        }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.Group; }
        }

        public EventsMenuResponse EventsMenuResponse { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }

        #endregion
    }

    public class RaceNodeTag : INodeTag
    {
        public RaceNodeTag(EventsMenuResponse eventsMenuResponse)
        {
            EventsMenuResponse = eventsMenuResponse;
            Id = eventsMenuResponse.Id;
            Name = eventsMenuResponse.Name;
            Venue = eventsMenuResponse.Venue;
            StartTime = eventsMenuResponse.StartTime;
            CountryCode = eventsMenuResponse.CountryCode;
        }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.Group; }
        }

        public EventsMenuResponse EventsMenuResponse { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Venue { get; private set; }
        public DateTime StartTime { get; private set; }
        public string CountryCode { get; private set; }

        #endregion
    }

    public class CountryNodeTag : INodeTag
    {
        public CountryNodeTag(string name)
        {
            Name = name;
        }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.Country; }
        }

        public string Id { get; set; }
        public string Name { get; set; }

        #endregion
    }

    public class MarketNodeTag : INodeTag
    {
        public MarketNodeTag(string id, string name, DateTime startTime)
        {
            Id = id;
            StartTime = startTime;
            Name = name;
        }

        public MarketNodeTag(EventsMenuResponse eventsMenuResponse)
            : this(eventsMenuResponse.Id, eventsMenuResponse.Name, eventsMenuResponse.MarketStartTime)
        {
        }

        public DateTime StartTime { get; private set; }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.Market; }
        }

        public string Id { get; set; }
        public string Name { get; set; }

        #endregion

        //public void SetTimeName()
        //{
        //    if (StartTime != DateTime.MinValue)
        //        Name = string.Format("{0:HH:mm} {1}", StartTime.ToLocalTime(), Name);
        //}
    }

    public class SelectionNodeTag : INodeTag
    {
        public SelectionNodeTag(int aMarketId, int anExchangeId, int aSelectionId)
        {
            MarketId = aMarketId;
            ExchangeId = anExchangeId;
            SelectionId = aSelectionId;
        }

        public int ExchangeId { get; set; }

        public int MarketId { get; set; }

        public int SelectionId { get; set; }

        #region INodeTag Members

        public NodeType Type
        {
            get { return NodeType.Selection; }
        }

        public string Id { get; set; }
        public string Name { get; set; }

        #endregion
    }
}
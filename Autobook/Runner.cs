using BetfairNgClient.Json;

namespace Autobook
{
  public class Runner
  {
    private PriceSize backPrice;
    private PriceSize layPrice;
    private readonly long selectionId;
    private readonly double lastPriceMatched;

    #region Properties

    public long SelectionId
    {
      get { return selectionId; }
    }

    public double LastPriceMatched
    {
      get { return lastPriceMatched; }
    }

    public double BackPrice
    {
      get { return backPrice.Price; }
    }

    public double BackAmount
    {
      get { return backPrice.Size; }
    }

    public double LayPrice
    {
      get { return layPrice.Price; }
    }

    public double LayAmount
    {
      get { return layPrice.Size; }
    }

    #endregion Properties

    public Runner (BetfairNgClient.Json.Runner runnerPrices, bool isSwopPrice, bool isNextBestPrice)
    {
      selectionId = runnerPrices.SelectionId;
        lastPriceMatched = runnerPrices.LastPriceTraded.HasValue ? runnerPrices.LastPriceTraded.Value : 0.0D;
      SetPrice (runnerPrices, isSwopPrice, isNextBestPrice);
    }

    private void SetPrice(BetfairNgClient.Json.Runner runnerPrices, bool isSwopPrice, bool isNextBestPrice)
    {
      var backPrices = (isSwopPrice)
       ? runnerPrices.ExchangePrices.AvailableToLay
       : runnerPrices.ExchangePrices.AvailableToBack;
      var layPrices = (isSwopPrice)
        ? runnerPrices.ExchangePrices.AvailableToBack
        : runnerPrices.ExchangePrices.AvailableToLay;

      if (backPrices.Count > 0)
      {
        backPrice = backPrices[0];
        if (isNextBestPrice)
          backPrice.Price = Globals.GetNextBestBackPrice (backPrice.Price, isSwopPrice);
      }
      else
      {
        backPrice = new PriceSize {Price = double.NaN};
      }

      if (layPrices.Count > 0)
      {
        layPrice = layPrices[0];
        if (isNextBestPrice)
          layPrice.Price = Globals.GetNextBestLayPrice (layPrice.Price, isSwopPrice);
        else if (!isSwopPrice && !double.IsNaN(backPrice.Price))
        {
          // When next best and swop are unticked, show back price + 1 tick in lay column
          layPrice.Price = (double)Globals.IncrementPrice((decimal)backPrice.Price);
        }
      }
      else if (!isSwopPrice && !double.IsNaN(backPrice.Price))
      {
        // No lay prices available from Betfair, but we can create one from back price + 1 tick
        layPrice = new PriceSize
        {
          Price = (double)Globals.IncrementPrice((decimal)backPrice.Price),
          Size = 0
        };
      }
      else
      {
        layPrice = new PriceSize {Price = double.NaN};
      }
    }
  }
}
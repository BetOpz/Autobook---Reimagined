using System.Collections.Generic;

namespace Autobook
{
    public class PlaceBetParam
    {
        public List<Runner> RunnerPrices { get; }
        public BetType Type { get; }
        public double Amount { get; }
        public double MinSelectionPrice { get; }
        public double BookPercentage { get; }
        public HashSet<long> ExcludedRunnerIds { get; }

        public PlaceBetParam(List<Runner> runnerPrices, BetType type, double amount, double minPrice, double bookPercentage, HashSet<long> excludedRunnerIds = null)
        {
            RunnerPrices = runnerPrices;
            Type = type;
            Amount = amount;
            MinSelectionPrice = minPrice;
            BookPercentage = bookPercentage;
            ExcludedRunnerIds = excludedRunnerIds ?? new HashSet<long>();
        }
    }
}
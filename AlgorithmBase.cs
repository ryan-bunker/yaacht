using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alpaca.Markets;

namespace TradeBot
{
    public abstract class AlgorithmBase
    {
        public int InitialIntervalPeriods { get; protected set; }

        public AggregationPeriod AggregationPeriod { get; protected set; }
            = new AggregationPeriod(1, AggregationPeriodUnit.Day);

        public virtual void Initialize(IReadOnlyList<IAgg> history)
        {
        }

        public virtual void Open(DateTime closingTime, IAccount account, IPosition position, ITrader trader)
        {
        }

        public abstract Task Perform(IAgg agg, IAccount account, IPosition position, ITrader trader);
    }
}
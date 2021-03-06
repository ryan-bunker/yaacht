using System;
using Alpaca.Markets;

namespace TradeBot
{
    internal class FakePosition : IPosition
    {
        public Guid AccountId { get; set; }
        public Guid AssetId { get; set; }
        public string Symbol { get; set; }
        public Exchange Exchange { get; set; }
        public AssetClass AssetClass { get; set; }
        public decimal AverageEntryPrice { get; set; }
        public int Quantity { get; set; }
        public PositionSide Side { get; set; }
        public decimal MarketValue { get; set; }
        public decimal CostBasis { get; set; }
        public decimal UnrealizedProfitLoss { get; set; }
        public decimal UnrealizedProfitLossPercent { get; set; }
        public decimal IntradayUnrealizedProfitLoss { get; set; }
        public decimal IntradayUnrealizedProfitLossPercent { get; set; }
        public decimal AssetCurrentPrice { get; set; }
        public decimal AssetLastPrice { get; set; }
        public decimal AssetChangePercent { get; set; }
    }
}
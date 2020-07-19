using System;
using Alpaca.Markets;

namespace TradeBot
{
    internal class FakeAccount : IAccount
    {
        public Guid AccountId { get; set; }
        public AccountStatus Status { get; set; }
        public string? Currency { get; set; }
        public decimal TradableCash { get; set; }
        public decimal WithdrawableCash { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AccountNumber { get; set; }
        public bool IsDayPatternTrader { get; set; }
        public bool IsTradingBlocked { get; set; }
        public bool IsTransfersBlocked { get; set; }
        public bool TradeSuspendedByUser { get; set; }
        public bool ShortingEnabled { get; set; }
        public long Multiplier { get; set; }
        public decimal BuyingPower { get; set; }
        public decimal DayTradingBuyingPower { get; set; }
        public decimal RegulationBuyingPower { get; set; }
        public decimal LongMarketValue { get; set; }
        public decimal ShortMarketValue { get; set; }
        public decimal Equity { get; set; }
        public decimal LastEquity { get; set; }
        public decimal InitialMargin { get; set; }
        public decimal MaintenanceMargin { get; set; }
        public decimal LastMaintenanceMargin { get; set; }
        public long DayTradeCount { get; set; }
        public decimal Sma { get; set; }
        public bool IsAccountBlocked { get; set; }
    }
}
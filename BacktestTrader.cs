using System;
using System.Threading.Tasks;
using Alpaca.Markets;

namespace TradeBot
{
    internal class BacktestTrader : ITrader
    {
        private decimal _cash;
        private int _quantity;

        public BacktestTrader(string symbol, decimal cash)
        {
            Symbol = symbol;
            _cash = cash;
        }

        public Task<IOrder> SubmitOrderAsync(int quantity, decimal? price, OrderSide side)
        {
            Console.WriteLine($"{side}ing {quantity} @ {price:C2}");
            if (side == OrderSide.Sell)
                quantity *= -1;

            _quantity += quantity;
            _cash -= quantity * (price ?? MarketPrice);
            Trades++;

            return Task.FromResult<IOrder>(new DummyOrder());
        }
        
        public string Symbol { get; }
        
        public decimal MarketPrice { get; set; }
        
        public int Trades { get; private set; }

        public IAccount Account => new FakeAccount {Equity = _cash + Position.MarketValue, BuyingPower = _cash, Multiplier = 1};

        public IPosition Position => new FakePosition {Quantity = _quantity, MarketValue = _quantity * MarketPrice};

        private class DummyOrder : IOrder
        {
            public Task<bool> DeleteAsync() => Task.FromResult(true);
        }
    }
}
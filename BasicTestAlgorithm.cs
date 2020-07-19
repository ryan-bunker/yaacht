using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alpaca.Markets;

namespace TradeBot
{
    public class BasicTestAlgorithm : AlgorithmBase
    {
        private const decimal Scale = 200;
        
        private readonly List<decimal> _closingPrices = new List<decimal>();
        private IOrder _lastOrder;
        private DateTime _closingTime;
        private bool _closed;
        
        public BasicTestAlgorithm()
        {
            InitialIntervalPeriods = 20;
        }

        public override void Initialize(IReadOnlyList<IAgg> history)
        {
            _closingPrices.AddRange(history.Select(agg => agg.Close));
        }

        public override void Open(DateTime closingTime, IAccount account, IPosition position, ITrader trader)
        {
            _closingTime = closingTime;
            _closed = false;
        }

        public override async Task Perform(IAgg agg, IAccount account, IPosition position, ITrader trader)
        {
            if (_closed) return;
            
            if (agg.Time.AddMinutes(15) > _closingTime)
            {
                // market is closing, liquidate our positions and reset data
                Console.WriteLine("Market closing soon, closing positions...");
                if (_lastOrder != null)
                    await _lastOrder.DeleteAsync();
                if (position.Quantity > 0)
                    await trader.SubmitOrderAsync(position.Quantity, null, OrderSide.Sell);
                else
                    await trader.SubmitOrderAsync(-1 * position.Quantity, null, OrderSide.Buy);
                _closingPrices.Clear();
                _closed = true;
                return;
            }
            
            _closingPrices.Add(agg.Close);
            if (_closingPrices.Count <= 20)
            {
                Console.WriteLine($"Waiting for more data ({_closingPrices.Count} of 20)");
                return;
            }
            _closingPrices.RemoveAt(0);

            decimal avg = _closingPrices.Average();
            decimal diff = avg - agg.Close;
            
            // cancel the last order to ensure we don't have long and short orders
            // open at the same time (if the order already fulfilled, delete does nothing)
            if (_lastOrder != null)
                await _lastOrder.DeleteAsync();

            // Make sure we know how much we should spend on our position.
            var buyingPower = account.BuyingPower;
            var equity = account.Equity;
            long multiplier = account.Multiplier;
            
            // Check how much we currently have in this position.
            int positionQuantity = position?.Quantity ?? 0;
            decimal positionValue = position?.MarketValue ?? 0;

            if (diff <= 0)
            {
                Console.WriteLine($"Price is above average; we want to short. (diff == {diff:C2})");
                if (positionQuantity > 0)
                {
                    // There is an existing long position we need to dispose of first
                    Console.WriteLine($"Removing {positionValue:C2} long position.");
                    _lastOrder = await trader.SubmitOrderAsync(positionQuantity, agg.Close, OrderSide.Sell);
                }
                else
                {
                    // Allocate a percent of portfolio to short position
                    decimal portfolioShare = -1 * diff / agg.Close * Scale;
                    decimal targetPositionValue = -1 * equity * multiplier * portfolioShare;
                    decimal amountToShort = targetPositionValue - positionValue;
                    
                    Console.WriteLine($"Target position: {targetPositionValue:C2}");
                    
                    if (amountToShort < 0)
                    {
                        // We want to expand our existing short position.
                        amountToShort *= -1;
                        if (amountToShort > buyingPower)
                        {
                            amountToShort = buyingPower;
                        }
                        int qty = (int)(amountToShort / agg.Close);
                        Console.WriteLine($"Adding {qty * agg.Close:C2} to short position.");
                        _lastOrder = await trader.SubmitOrderAsync(qty, agg.Close, OrderSide.Sell);
                    }
                    else
                    {
                        // We want to shrink our existing short position.
                        int qty = (int)(amountToShort / agg.Close);
                        if (qty > -1 * positionQuantity)
                        {
                            qty = -1 * positionQuantity;
                        }
                        Console.WriteLine($"Removing {qty * agg.Close:C2} from short position");
                        _lastOrder = await trader.SubmitOrderAsync(qty, agg.Close, OrderSide.Buy);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Price is below average; we want to go long. (diff == {diff:C2})");
                // Allocate a percent of our portfolio to long position.
                decimal portfolioShare = diff / agg.Close * Scale;
                decimal targetPositionValue = equity * multiplier * portfolioShare;
                decimal amountToLong = targetPositionValue - positionValue;
                
                Console.WriteLine($"Target position: {targetPositionValue:C2}");

                if (positionQuantity < 0)
                {
                    // There is an existing short position we need to dispose of first
                    Console.WriteLine($"Removing {positionValue:C2} short position.");
                    _lastOrder = await trader.SubmitOrderAsync(positionQuantity * -1, agg.Close, OrderSide.Buy);
                }
                else if (amountToLong > 0)
                {
                    // We want to expand our existing long position.
                    if (amountToLong > buyingPower)
                    {
                        amountToLong = buyingPower;
                    }
                    int qty = (int)(amountToLong / agg.Close);

                    _lastOrder = await trader.SubmitOrderAsync(qty, agg.Close, OrderSide.Buy);
                    Console.WriteLine($"Adding {qty * agg.Close:C2} to long position.");
                }
                else
                {
                    // We want to shrink our existing long position.
                    amountToLong *= -1;
                    int qty = (int)(amountToLong / agg.Close);
                    if (qty > positionQuantity)
                    {
                        qty = positionQuantity;
                    }

                    _lastOrder = await trader.SubmitOrderAsync(qty, agg.Close, OrderSide.Sell);
                    Console.WriteLine($"Removing {qty * agg.Close:C2} from long position");
                }
            }
        }
    }
}
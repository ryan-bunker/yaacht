using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alpaca.Markets;
using Microsoft.VisualBasic;

namespace TradeBot
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var secretDoc = JsonDocument.Parse(await File.ReadAllTextAsync("secret.json"));
            var keyId = secretDoc.RootElement.GetProperty("key_id").GetString();
            var secretKey = secretDoc.RootElement.GetProperty("secret_key").GetString();

            var tradingClient = Environments.Paper.GetAlpacaTradingClient(new SecretKey(keyId, secretKey));
            var dataClient = Environments.Live.GetPolygonDataClient(keyId);

            var algo = new BasicTestAlgorithm();

            var request = new AggregatesRequest("AAPL", new AggregationPeriod(1, AggregationPeriodUnit.Minute));
            //request.SetInclusiveTimeInterval(DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-5));
            request.SetInclusiveTimeInterval(
                new DateTime(2019, 11, 1), 
                new DateTime(2019, 11, 30, 11, 59, 59));
            var agg = await dataClient.ListAggregatesAsync(
                request);
            Console.WriteLine($"{agg.ResultsCount} results");

            var trader = new BacktestTrader("AAPL", 50_000);
            foreach (var g in agg.Items.GroupBy(a => a.Time.Date))
            {
                var calendar = (await tradingClient.ListCalendarAsync(
                    new CalendarRequest().SetTimeInterval(g.Key.GetInclusiveIntervalFromThat()))).First();
                Console.WriteLine($"Starting {g.Key:M/d/yyyy} -- Open: {calendar.TradingOpenTime:h:mm tt zzz}  Close: {calendar.TradingCloseTime:h:mm tt zzz}");
                algo.Open(calendar.TradingCloseTime, trader.Account, trader.Position, trader);
                
                foreach (var item in g.Where(a => a.Time >= calendar.TradingOpenTime && a.Time <= calendar.TradingCloseTime))
                {
                    Console.WriteLine(
                        $"{item.Time:M/d/yyyy h:mm tt zzz}  O: ${item.Open,6:N2}  H: ${item.High,6:N2}  L: ${item.Low,6:N2}  C: ${item.Close,6:N2}  V: {item.Volume,5:N0}");
                    trader.MarketPrice = item.Close;
                    await algo.Perform(item, trader.Account, trader.Position, trader);

                    Console.WriteLine($"  Equity: {trader.Account.Equity:C2}  BP: {trader.Account.BuyingPower:C2}");
                    Console.WriteLine($"  {trader.Position.Quantity:N0} shares (MV: {trader.Position.MarketValue:C2})");
                    Console.WriteLine("----------");
                }
                
                Console.WriteLine($"Closing {g.Key:d}");
                Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            }
           
            
            Console.WriteLine();
            Console.WriteLine($"{trader.Trades:N0} trades");
            var profit = trader.Account.Equity - 50_000;
            Console.WriteLine($"Net profit:          {profit,10:C2} ({profit / 50_000,7:P2})");
            
            var holdQty = (int) (50_000 / agg.Items.First().Close);
            var holdEquity = holdQty * agg.Items.Last().Close;
            profit = holdEquity - 50_000;
            Console.WriteLine($"Buy and hold profit: {profit,10:C2} ({profit / 50_000,7:P2})");
        }
    }
}

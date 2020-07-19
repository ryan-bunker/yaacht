using System.Threading.Tasks;
using Alpaca.Markets;

namespace TradeBot
{
    public interface ITrader
    {
        Task<IOrder> SubmitOrderAsync(int quantity, decimal? price, OrderSide side);
    }

    public interface IOrder
    {
        Task<bool> DeleteAsync();
    }
}
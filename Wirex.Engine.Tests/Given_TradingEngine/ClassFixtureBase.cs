using System.Collections.Generic;
using System.Linq;
using Troschuetz.Random;

namespace Wirex.Engine.Tests.Given_TradingEngine
{
    public abstract class ClassFixtureBase
    {
        protected IList<Order> GetRandomOrders()
        {
            return GenerateRandom(10000, "USD", "EUR", 0.93, 0.99).ToList();
        }

        private static IEnumerable<Order> GenerateRandom(
            int orderCount,
            string baseCurrency,
            string quoteCurrency,
            double minPrice,
            double maxPrice)
        {
            var price = new TRandom();
            var amount = new TRandom();

            for (var i = 0; i < orderCount; i++)
            {
                yield return new Order(
                    new CurrencyPair(baseCurrency, quoteCurrency),
                    Side.Buy,
                    decimal.Round((decimal)price.NextDouble(minPrice, maxPrice), 4),
                    amount.Next(1, 100));

                yield return new Order(
                    new CurrencyPair(baseCurrency, quoteCurrency),
                    Side.Sell,
                    decimal.Round((decimal)price.NextDouble(minPrice, maxPrice), 4),
                    amount.Next(1, 100));
            }
        }
    }
}

namespace Wirex.Engine.Services
{
    /// <summary>
    /// Represents the rule of matching two orders.
    /// </summary>
    public class OrdersMatchingRule : IOrdersMatchingRule
    {
        /// <summary>
        /// Determines whether the specified order1 is matched to order2 to be processed.
        /// </summary>
        /// <param name="order1">The order1 to match with order2.</param>
        /// <param name="order2">The order2 to match with order1.</param>
        /// <returns>
        ///   <c>true</c> if the specified order1 is matched to order2; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatched(Order order1, Order order2)
        {
            // orders should not be closed
            if (order1.IsClosed || order2.IsClosed)
            {
                return false;
            }

            // currency pairs should be equal
            if (!Equals(order1.CurrencyPair, order2.CurrencyPair))
            {
                return false;
            }

            // Buy order can be matched with Sell order,
            // which have Price equal or lower than Buy order price
            if (order1.Side == Side.Buy
                && order2.Side == Side.Sell
                && order2.Price <= order1.Price)
            {
                return true;
            }

            // and in oposite to previous one
            if (order1.Side == Side.Sell
                && order2.Side == Side.Buy
                && order2.Price >= order1.Price)
            {
                return true;
            }

            // otherwise
            return false;
        }
    }
}

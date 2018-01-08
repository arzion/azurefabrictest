namespace Wirex.Engine.Services
{
    /// <summary>
    /// Represents the rule of matching two orders.
    /// </summary>
    public interface IOrdersMatchingRule
    {
        /// <summary>
        /// Determines whether the specified order1 is matched to order2 to be processed.
        /// </summary>
        /// <param name="order1">The order1 to match with order2.</param>
        /// <param name="order2">The order2 to match with order1.</param>
        /// <returns>
        ///   <c>true</c> if the specified order1 is matched to order2; otherwise, <c>false</c>.
        /// </returns>
        bool IsMatched(Order order1, Order order2);
    }
}

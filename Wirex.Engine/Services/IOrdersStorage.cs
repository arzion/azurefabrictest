namespace Wirex.Engine.Services
{
    /// <summary>
    /// Represents storage of the orders.
    /// </summary>
    public interface IOrdersStorage
    {
        /// <summary>
        /// Adds the specified order into the storage.
        /// </summary>
        /// <param name="order">The order to add.</param>
        void Add(Order order);

        /// <summary>
        /// Removes the specified order from the storage.
        /// </summary>
        /// <param name="order">The order to remove.</param>
        void Remove(Order order);

        /// <summary>
        /// Gets the next matched order according to matching rules.
        /// </summary>
        /// <param name="current">The current order to start search from.</param>
        /// <param name="target">The target order for which matching rules should be applied.</param>
        /// <returns>Found order that is matched with target, or null if there is no such.</returns>
        Order GetNextMatched(Order current, Order target);
    }
}

using System;
using System.Threading.Tasks;

namespace Wirex.Engine.Engine
{
    /// <summary>
    /// Represents the engine of trading operations.
    /// </summary>
    public interface ITradingEngine
    {
        /// <summary>
        /// Places the specified order to process.
        /// </summary>
        /// <param name="order">The order.</param>
        Task PlaceAsync(Order order);

        /// <summary>
        /// Occurs when order is opened.
        /// </summary>
        event EventHandler<OrderArgs> OrderOpened;

        /// <summary>
        /// Occurs when order is closed.
        /// </summary>
        event EventHandler<OrderArgs> OrderClosed;
    }
}
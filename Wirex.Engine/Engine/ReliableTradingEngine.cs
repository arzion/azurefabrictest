using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Wirex.Engine.Services;

namespace Wirex.Engine.Engine
{
    public class ReliableTradingEngine : ITradingEngine
    {
        private readonly IReliableStateManager _stateManager;
        private readonly IOrdersMatchingRule _ordersMatchingRule;

        private const string OrdersCollectionName = "Orders";

        public ReliableTradingEngine(
            IReliableStateManager stateManager,
            IOrdersMatchingRule ordersMatchingRule)
        {
            _stateManager = stateManager;
            _ordersMatchingRule = ordersMatchingRule;
        }

        public async Task PlaceAsync(Order order)
        {
            var storage = await _stateManager
                .GetOrAddAsync<IReliableDictionary<OrderQueueKey, OrderQueue>>(OrdersCollectionName);

            var orderQueueKey = new OrderQueueKey(order.CurrencyPair, order.Side);
            var oppositeOrderQueueKey = new OrderQueueKey(order.CurrencyPair, order.Side == Side.Buy ? Side.Sell : Side.Buy);

            await AddOrderAsync(order, storage, orderQueueKey);

            RaiseOrderOpened(order);

            await ProcessOrdersAsync(order, storage, orderQueueKey, oppositeOrderQueueKey);
        }

        private async Task ProcessOrdersAsync(
            Order order,
            IReliableDictionary<OrderQueueKey, OrderQueue> storage,
            OrderQueueKey orderQueueKey,
            OrderQueueKey oppositeOrderQueueKey)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                var orderQueue = await storage.GetOrAddAsync(
                    tx,
                    orderQueueKey,
                    new OrderQueue(order.CurrencyPair, order.Side));

                var oppositeOrderQueue = await storage.GetOrAddAsync(
                    tx,
                    oppositeOrderQueueKey,
                    new OrderQueue(order.CurrencyPair, order.Side == Side.Buy ? Side.Sell : Side.Buy));

                while (true)
                {
                    var oppositeOrder = oppositeOrderQueue.Peek();
                    if (oppositeOrder == null)
                    {
                        break;
                    }

                    var matched = _ordersMatchingRule.IsMatched(order, oppositeOrder);

                    if (matched)
                    {
                        matched = _ordersMatchingRule.IsMatched(order, oppositeOrder);
                        if (matched)
                        {
                            ProcessOrders(order, orderQueue, oppositeOrder, oppositeOrderQueue);
                        }
                        else
                        {
                            // process further if some thread is here
                            continue;
                        }
                    }

                    if (order.IsClosed || !matched || oppositeOrderQueue.IsEmpty)
                    {
                        break;
                    }
                }

                await storage.AddOrUpdateAsync(tx, orderQueueKey, orderQueue, (key, old) => orderQueue);
                await storage.AddOrUpdateAsync(tx, oppositeOrderQueueKey, oppositeOrderQueue, (key, old) => orderQueue);

                await tx.CommitAsync();
            }
        }

        private async Task AddOrderAsync(Order order, IReliableDictionary<OrderQueueKey, OrderQueue> storage, OrderQueueKey orderQueueKey)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                var orderQueue = await storage.GetOrAddAsync(
                    tx,
                    orderQueueKey,
                    new OrderQueue(order.CurrencyPair, order.Side));

                orderQueue.Add(order);

                await storage.AddOrUpdateAsync(tx, orderQueueKey, orderQueue, (key, old) => orderQueue);

                await tx.CommitAsync();
            }
        }

        public event EventHandler<OrderArgs> OrderOpened;
        public event EventHandler<OrderArgs> OrderClosed;

        private void ProcessOrders(
            Order order1,
            OrderQueue order1Queue,
            Order order2,
            OrderQueue order2Queue)
        {
            var amountToSubtract = Math.Min(order1.RemainingAmount, order2.RemainingAmount);
            order1.RemainingAmount -= amountToSubtract;
            order2.RemainingAmount -= amountToSubtract;

            if (order1.IsClosed)
            {
                order1Queue.Remove(order1);
                RaiseOrderClosed(order1);
            }
            if (order2.IsClosed)
            {
                order2Queue.Remove(order2);
                RaiseOrderClosed(order2);
            }
        }

        private void RaiseOrderClosed(Order order)
        {
            OrderClosed?.Invoke(this, new OrderArgs(order));
        }

        private void RaiseOrderOpened(Order order)
        {
            OrderOpened?.Invoke(this, new OrderArgs(order));
        }
    }
}

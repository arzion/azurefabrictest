using System;

namespace Wirex.Engine.Engine
{
    public class OrderQueueKey : IComparable<OrderQueueKey>, IEquatable<OrderQueueKey>
    {
        private readonly CurrencyPair _currencyPair;
        private readonly Side _side;

        public OrderQueueKey(CurrencyPair currencyPair, Side side)
        {
            _currencyPair = currencyPair;
            _side = side;
        }

        public CurrencyPair CurrencyPair => _currencyPair;

        public Side Side => _side;

        #region equality

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return obj.GetType() == GetType() && Equals((OrderQueueKey)obj);
        }

        public bool Equals(OrderQueueKey other)
        {
            return other != null
                   && _currencyPair.Equals(other._currencyPair)
                   && _side == other._side;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_currencyPair.GetHashCode() * 397) ^ _side.GetHashCode();
            }
        }

        #endregion

        public int CompareTo(OrderQueueKey other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return _side.CompareTo(other._side);
        }

        public override string ToString()
        {
            return $"Pair: {_currencyPair}, Side: {_side}";
        }
    }
}

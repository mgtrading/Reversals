using System;
using Reversals.Backtests.Enums;

namespace Reversals.Backtests
{
    public class Order
    {
        #region Properties

        private readonly int _account;
        private readonly string _symbol;
        private readonly int _id;
        private readonly DateTime _time;
        private readonly Operation _operation;
        private readonly OrderType _type;
        private int _size;
        private readonly double _price;
        private OrderState _state;
        private string _comment;
        private string _addInfo;

        #endregion

        public Order(int account, string symbol, int id, DateTime time, Operation operation, OrderType type, int size, double price, string comment, string addInfo)
        {
            _account = account;
            _symbol = symbol;
            _id = id;
            _time = time;
            _operation = operation;
            _type = type;
            _size = size;
            _price = price;
            _state = _type == OrderType.Market ? OrderState.Filled : OrderState.Working;
            _comment = comment;
            _addInfo = addInfo;
        }

        public int Account
        {
            get { return _account; }
        }

        public string Symbol
        {
            get { return _symbol; }
        }

        public int Id
        {
            get { return _id; }
        }

        public DateTime Time
        {
            get { return _time; }
        }

        public Operation Operation
        {
            get { return _operation; }
        }

        public OrderType Type
        {
            get { return _type; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public double Price
        {
            get { return _price; }
        }

        public OrderState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public string AddInfo
        {
            get { return _addInfo; }
            set { _addInfo = value; }
        }

        public static Order operator ++(Order order)
        {
            order._size++;
            return order;
        }

        public static Order operator --(Order order)
        {
            order._size--;
            return order;
        }

        public static Order operator +(Order order, int value)
        {
            return new Order(order._account, order._symbol, order._id, order.Time, order._operation, order._type, order._size + value, order._price, order._comment, order._addInfo);
        }

        public static Order operator -(Order order, int value)
        {
            return new Order(order._account, order._symbol, order._id, order.Time, order._operation, order._type, order._size - value, order._price, order._comment, order._addInfo);
        }

        public static bool operator ==(Order order, int value)
        {
            return order != null && (order._size == value);
        }

        public static bool operator !=(Order order, int value)
        {
            return order != null && order._size != value;
        }

        public override bool Equals(object obj)
        {
            return (_size == Convert.ToInt32(obj));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

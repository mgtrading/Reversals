using System;
using Reversals.Backtests.Enums;

namespace Reversals.Backtests
{
    public class Position
    {
        #region VARIABLES

        private readonly int _account;
        private readonly string _symbol;
        private readonly int _openOrderId;
        private int _closeOrderId;
        private readonly DateTime _timeOpen;
        private DateTime _timeClose;
        private readonly Operation _operation;
        private int _size;
        private readonly double _price;
        private double _last;
        private double _trades;
        private double _margin;
        private string _comment;
        private string _addInfo;
        private bool _isPremium;
        private double _commission;
        private double _posPnl;
        private double _closePnl;

        #endregion

        #region CONSTRUCTOR
        public Position(int account, string symbol, int openOrderId, DateTime timeOpen, Operation operation, int size, double price, double last, string comment, string addInfo, bool isPremium)
        {
            _account = account;
            _symbol = symbol;
            _openOrderId = openOrderId;
            _closeOrderId = 0;
            _timeOpen = timeOpen;
            _timeClose = new DateTime();
            _operation = operation;
            _size = size;
            _price = price;
            _last = last;
            _trades = (_operation == Operation.Buy) ? Math.Round((_last - _price) * _size, 1) : (_operation == Operation.Sell) ? Math.Round((_price - _last) * _size, 1) : 0;
            _margin = _last * _size;
            _comment = comment;
            _addInfo = addInfo;
            _isPremium = isPremium;
        }
        #endregion

        #region PROPERTIES
        public int Account
        {
            get { return _account; }
        }

        public string Symbol
        {
            get { return _symbol; }
        }

        public int OpenOrderId
        {
            get { return _openOrderId; }
        }

        public int CloseOrderId
        {
            get { return _closeOrderId; }
            set { _closeOrderId = value; }
        }

        public DateTime TimeOpen
        {
            get { return _timeOpen; }
        }

        public DateTime TimeClose
        {
            get { return _timeClose; }
            set { _timeClose = value; }
        }

        public Double Commission
        {
            get { return _commission; }
            set { _commission = value; }
        }

        public Double PosPNL
        {
            get { return _posPnl; }
            set { _posPnl = value; }
        }

        public Double ClosePNL
        {
            get { return _closePnl; }
            set { _closePnl = value; }
        }

        public Operation Operation
        {
            get { return _operation; }
        }

        public int Size
        {
            get { return _size; }
        }

        public double Price
        {
            get { return _price; }
        }

        public double Last
        {
            get { return _last; }
            set 
            { 
                _last = value;
                _trades = (_operation == Operation.Buy) ? Math.Round((_last - _price) * _size, 1) : (_operation == Operation.Sell) ? Math.Round((_price - _last) * _size, 1) : 0;
                _margin = _last * _size;
            }
        }

        public double Trades
        {
            get { return _trades; }
            set { _trades = value; }
        }

        public double Margin
        {
            get { return _margin; }
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

        public static Position operator ++(Position position)
        {
            position._size++;
            return position;
        }

        public static Position operator --(Position position)
        {
            position._size--;
            return position;
        }

        public bool IsPremium
        {
            get { return _isPremium; }
            set { _isPremium = value; }
        }

        public static Position operator +(Position position, int value)
        {
            return new Position(position._account, position._symbol, position._openOrderId, position._timeOpen, position._operation, position._size + value, position._price, position._last, position._comment, position._addInfo, position._isPremium);   
        }

        public static Position operator -(Position position, int value)
        {
            return new Position(position._account, position._symbol, position._openOrderId, position._timeOpen, position._operation, position._size - value, position._price, position._last, position._comment, position._addInfo, position._isPremium);
        }
        #endregion
    }
}

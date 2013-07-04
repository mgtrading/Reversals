using System.Collections.Generic;
using Reversals.Backtests.Enums;
using Reversals.DataContainer;

namespace Reversals.Backtests
{
    public class Backtest
    {
        private readonly List<Order> _orders = new List<Order>();
        private readonly List<Position> _positions = new List<Position>();
        private readonly List<Position> _trades = new List<Position>();

        private bool _isStop;
        private bool _isBuy;
        private bool _isSell;
        private bool _isHighOverPrice;
        private bool _isPriceOverLow;
        private bool _isTime;
       
        public Backtest(List<Order> orders, List<Position> positions, List<Position> trades)
        {
            _orders = orders;
            _positions = positions;
            _trades = trades;
        }

        public void DispatchOrders(Tick tick, out List<Order> orders, out List<Position> positions, out List<Position> trades)
        {
            for (int i = 0; i < _orders.Count; i++)
            {
                Order order = _orders[i];
                switch (order.State)
                {
                    case OrderState.Completed:
                        break;

                    case OrderState.Filled:
                        DispatchPositions(order, tick, i);
                        break;

                    case OrderState.Initialized:
                        break;

                    case OrderState.Working:
                        _isStop = order.Type == OrderType.Stop;
                        _isBuy = order.Operation == Operation.Buy;
                        _isSell = order.Operation == Operation.Sell;
                        _isTime = order.Time < tick.Time;
                        if (_isStop && _isBuy && _isHighOverPrice && _isTime)
                            DispatchPositions(order, tick, i);
                        else if (_isStop && _isSell && _isPriceOverLow && _isTime)
                            DispatchPositions(order, tick, i);
                        break;

                    case OrderState.Undefined:
                        break;
                }
            }

            orders = _orders;
            positions = _positions;
            trades = _trades;

            foreach (Position t in _positions)
                t.Last = tick.Close;
        }

        public int DispatchPositions(Order order, Tick tick, int j)
        {
            for (int i = _positions.Count - 1; i >= 0; i--)
            {
                Position position = _positions[i];

                if (position.Operation != order.Operation)
                {
                    if (position.Size <= order.Size)
                    {
                        order -= position.Size;
                        _trades.Add(position);
                        ClosePosition(i, position.Size, tick, order.Id, order.AddInfo, order.Comment);
                        if (order == 0) break;
                    }
                    else
                    {
                        _trades.Add(position);
                        ClosePosition(i, order.Size, tick, order.Id, order.AddInfo, order.Comment);
                        order.Size = 0;
                        break;
                    }
                }
            }

            if (order.Size > 0)
            {
                _positions.Add(new Position(order.Account, order.Symbol, order.Id, tick.Time, order.Operation, order.Size, order.Price, tick.Close, order.Comment, order.AddInfo, false));
                int size = order.Size;
                _orders.RemoveAt(j);
                return size;
            }
                _orders.RemoveAt(j);           

            return 0;
        }

        private void ClosePosition(int j, int size, Tick tick, int orderId, string orderAdditionalInfo, string orderComment)
        {
            if (_positions[j].Size <= size)
            {
                Position pos = _positions[j];
                pos.TimeClose = tick.Time;
                pos.Last = tick.Close;
                pos.CloseOrderId = orderId;
                //if (OrderComment != "CloseByTrLf")
                //    pos.Comment = "ClbyRevSig";
                //else
                pos.Comment = orderComment;

                pos.AddInfo += " " + orderAdditionalInfo;
                _positions.Remove(pos);
            }
            else
            {
                Position pos = _positions[j];
                pos -= size;
                _positions[j] = pos;
            }
        }
    }
}

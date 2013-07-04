using System;
using System.Collections.Generic;
using OptionPR.Indicators;
using OptionPR.DataContainer;
using OptionPR.Backtests;
namespace OptionPR.Strategies
{
    class OptionsTime:Strategy
    {
        private bool _isTrade = false;
        private bool _isFirst = true;
       
        public OptionsTime(StrategyParameters _parameters, List<StrategyAdditionalParameter> _additional, ref Data _data, ref IndicatorPortfolio _indicators)
        {
            parameters = _parameters;
            additional = _additional;
            bars = _data.Bars;
            indicators = _indicators;
        }

        public override void Start(int StartBarIndex)
        {
            for (int i = StartBarIndex + 1; i < bars.Count; i++)
            {
                Bar bar = bars[i];

                if (parameters.Range.IsUseRange)
                {
                    if (bar.IntradayIndex == 1)
                    {
                        order_id++;
                        orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Sell, OrderType.Market, 1, bar.Open, "", ""));
                    }                  
                    else if (bar.IntradayIndex == 2)
                    {
                        if(_isFirst)
                        {
                            _isFirst = false;
                            Trade(ref positions, bar);
                            positions.Clear();
                            orders.Clear();
                        }
                       if (!_isTrade)
                       {
                           _isTrade=true;
                           order_id++;
                           orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Buy, OrderType.Market, 1, bar.Open, "", ""));  
                       }
                    }
                    else if (bar.IntradayIndex == 3)
                    {
                        _isTrade = false;
                        Trade(ref positions, bar);
                        positions.Clear();
                        orders.Clear();
                        continue;
                    }
                    else if (bar.IntradayIndex == 18)
                    {
                        if (!_isTrade)
                        {
                            _isTrade = true;
                            order_id++;
                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Buy, OrderType.Market, 1, bar.Open, "", ""));
                        }
                    }
                    else if (bar.IntradayIndex == 19)
                    {
                        _isTrade = false;
                        Trade(ref positions, bar);
                        positions.Clear();
                        orders.Clear();
                        continue;
                    }
                    else if (bar.IntradayIndex == 20)
                    {
                       if (!_isTrade)
                       {
                           _isTrade=true;
                           order_id++;
                           orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Buy, OrderType.Market, 1, bar.Open, "", ""));  
                       }
                    }
                    else if (bar.IntradayIndex == 21)
                    {
                        _isTrade = false;
                        Trade(ref positions, bar);
                        positions.Clear();
                        orders.Clear();
                        continue;
                    }
                    else if (bar.IntradayIndex == 22)
                    {
                        if (!_isTrade)
                        {
                            _isTrade = true;
                            order_id++;
                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Buy, OrderType.Market, 1, bar.Open, "", ""));
                        }
                    }
                    else if (bar.IntradayIndex == 23)
                    {
                        _isTrade = false;
                        Trade(ref positions, bar);
                        positions.Clear();
                        orders.Clear();
                        continue;
                    }

                }

                // This strategy does not contains Stoploss/Profit Target logic. The logic is disabled
                //DispatchPnL(bar, info.Stoploss, info.Profit);

                DispatchOrders(bar);
            }

        }

        public override void Trade(ref List<Position> _positions, Bar bar)
        {
            //foreach (Position _position in _positions)
            for (int i = 0; i < _positions.Count; i++)
            {
               
                Position _position = _positions[i];
                 order_id++;
                _position.Comment = "Closed for Exit";
                _position.TimeClose = bar.Time;
                _position.Last = bar.Close;
                _position.CloseOrderID = order_id;
                trades.Add(_position);
                if (!_isFirst)
                {
                    _isFirst = true;
                    trades[trades.Count - 1].FPL = 500;
                }
            }
        }

        public override void Clear()
        {
            backtest = null;
            orders.Clear();
            positions.Clear();
            trades.Clear();
        }
    }
}

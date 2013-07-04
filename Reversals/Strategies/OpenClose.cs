using System;
using System.Collections.Generic;
using OptionPR.Indicators;
using OptionPR.DataContainer;
using OptionPR.Backtests;

namespace OptionPR.Strategies
{
    class OpenClose : Strategy
    {

        private bool long_ = false;

        public OpenClose(StrategyParameters _parameters, List<StrategyAdditionalParameter> _additional, ref Data _data, ref IndicatorPortfolio _indicators)
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
                    if (bar.IntradayIndex < parameters.Range.RangeStartTime) continue;
                    else if (bar.IntradayIndex >= parameters.Range.RangeEndTime)
                    {
                        Trade(ref positions, bar);
                        positions.Clear();
                        orders.Clear();
                        continue;
                    }
                }

                // This strategy does not contains Stoploss/Profit Target logic. The logic is disabled
                //DispatchPnL(bar, info.Stoploss, info.Profit);

                if (long_)
                {
                    if (bar.Open < bar.Close)
                    {
                        long_ = false;
                        order_id++;
                        orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Sell, OrderType.Market, 1, bar.Close, "", ""));
                    }
                }
                else
                {
                    if (bar.Open > bar.Close)
                    {
                        long_ = true;
                        order_id++;
                        orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Buy, OrderType.Market, 1, bar.Open, "", ""));
                    }
                }

                DispatchOrders(bar);
            }

        }


        public override void Clear()
        {
            backtest = null;
            orders.Clear();
            positions.Clear();
            trades.Clear();
        }

        private int PositionPerDirection(List<Position> Positions)
        {
            int countBuy = 0;
            int countSell = 0;
            for (int i = 0; i < Positions.Count; i++)
            {
                if (Positions[i].Operation == Operation.Buy) countBuy++;
                if (Positions[i].Operation == Operation.Sell) countSell++;
            }
            if (countBuy != 0 && countSell != 0)
            {
                throw new NotImplementedException("We have long and short orders");
            }
            else
                if (countBuy == 0 && countSell != 0)
                {
                    return -countSell;
                }
                else
                    if (countBuy != 0 && countSell == 0)
                    {
                        return countBuy;
                    }
                    else
                        return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using OptionPR.Indicators;
using OptionPR.DataContainer;
using OptionPR.Backtests;

namespace OptionPR.Strategies
{
    public class AntiEmaPrcBL: Strategy
    {
        private int MaxIntradayIndex;

        public enum AdditionalParametersEnum
        {
            MaxPositionsPerDirection,
            MAChange,
            TradeLife
        }

        public AntiEmaPrcBL(StrategyParameters _parameters, List<StrategyAdditionalParameter> _additional, ref Data _data, ref IndicatorPortfolio _indicators)
        {
            parameters = _parameters;
            additional = _additional;
            bars = _data.Bars;
            indicators = _indicators;
            MaxIntradayIndex = _data.GetIndexDictionary().Values.ToList<int>().Max();
        }

        public override void Start(int StartBarIndex)
        {
            MovingAverage _ema = indicators[0] as MovingAverage;

            int MaxPositionsPerDirection = Convert.ToInt32(AdditionalParameter(AdditionalParametersEnum.MaxPositionsPerDirection).Value);
            int MAChange = Convert.ToInt32(AdditionalParameter(AdditionalParametersEnum.MAChange).Value);
            int TradeLife = Convert.ToInt32(AdditionalParameter(AdditionalParametersEnum.TradeLife).Value);

            Dictionary<int, int> closeIndex = new Dictionary<int, int>();

            logCounter++;
            Log("<--------------- S T A R T --------------->");
            Log("{0} - {1}", bars[StartBarIndex].Time.ToString("dd/MM/yyyy HH:mm:ss"), bars[bars.Count - 1].Time.ToString("dd/MM/yyyy HH:mm:ss"));

            for (int i = StartBarIndex; i < (parameters.OrderPlacingMode == OrderPlacing.ForCurrentClose ? bars.Count : bars.Count-1); i++)
            {
                Log(" ");

                Bar bar = bars[i];
                Log("Time = {0}, O={1}, H={2}, L={3}, C={4}", bar.Time, bar.Open, bar.High, bar.Low, bar.Close);
                Log("EMA = {0}", _ema.Data[i].Value1);

                Log("Start DispatchPnL, Last OrderId = {0}", order_id);
                DispatchPnL(bar, parameters.Stoploss * parameters.TickSize, parameters.Profit * parameters.TickSize);
                Log("Finish DispatchPnL, Last OrderId = {0}", order_id);

                int pos = PositionPerDirection(positions);
                Log("Position = {0}", pos);

                bool buyCond = bar.Close < _ema.Data[i].Value1;
                bool sellCond = bar.Close > _ema.Data[i].Value1;
                Log("BuyCond = {0}, SellCond = {1}", buyCond, sellCond);

                //if (parameters.Range.IsUseRange)
                {
                    if (parameters.Range.RangeStartTime <= bar.IntradayIndex
                        && bar.IntradayIndex <= parameters.Range.RangeEndTime)
                    // In Range
                    {
                        Log("Bar in Range");

                        bool isFridayEnd = bar.DayOWeekIndex == 5 && bar.IntradayIndex >= 20;

                        if (buyCond)
                        {
                            if (pos >= 0 && pos < MaxPositionsPerDirection)
                                if (MAChange != bar.DayOWeekIndex)
                                    if (!isFridayEnd)
                                {
                                    order_id++;
                                    if (parameters.OrderPlacingMode == OrderPlacing.ForCurrentClose)
                                        orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Buy, OrderType.Market, 1, bar.Close, "Buy Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                                    else
                                        orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bars[i + 1].Time, Operation.Buy, OrderType.Market, 1, bars[i + 1].Open, "Buy Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));


                                    closeIndex.Add(order_id, bar.IntradayIndex + TradeLife >= MaxIntradayIndex + 1 ? bar.IntradayIndex + TradeLife - MaxIntradayIndex - 1 : bar.IntradayIndex + TradeLife);
                                    Log("Add open order: id = {0}, operation = {1}", order_id, "Buy");
                                }
                        }
                        else
                        if (sellCond)
                        {
                            if (pos <= 0 && (-pos) < MaxPositionsPerDirection)
                                if (MAChange != bar.DayOWeekIndex)
                                    if (!isFridayEnd)
                                    {
                                        order_id++;
                                        if (parameters.OrderPlacingMode == OrderPlacing.ForCurrentClose)
                                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Sell, OrderType.Market, 1, bar.Close, "Sell Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                                        else
                                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bars[i + 1].Time, Operation.Sell, OrderType.Market, 1, bars[i + 1].Open, "Sell Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));

                                        closeIndex.Add(order_id, bar.IntradayIndex + TradeLife >= MaxIntradayIndex + 1 ? bar.IntradayIndex + TradeLife - MaxIntradayIndex - 1 : bar.IntradayIndex + TradeLife);
                                        Log("Add open order: id = {0}, operation = {1}", order_id, "Sell");
                                    }
                        }

                    }
                    else
                    // Out of Range
                    {
                        Log("Bar out of Range");
                        if (parameters.ClosePositionEoR)
                        {
                            Trade(ref positions, bar);
                            positions.Clear();
                            orders.Clear();
                            pos = PositionPerDirection(positions);
                            Log("ClosePositionEoR");
                        }
                    }
                }

                if (pos < 0)
                {
                    if (buyCond)
                    {
                        order_id++;
                        if (parameters.OrderPlacingMode == OrderPlacing.ForCurrentClose)
                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Buy, OrderType.Market, 1, bar.Close, "Buy Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                        else
                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bars[i + 1].Time, Operation.Buy, OrderType.Market, 1, bars[i + 1].Open, "Buy Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                        Log("Add close order: id = {0}, operation = {1}", order_id, "Buy");
                    }
                }
                else
                if (pos > 0)
                {
                    if (sellCond)
                    {
                        order_id++;
                        if (parameters.OrderPlacingMode == OrderPlacing.ForCurrentClose)
                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, Operation.Sell, OrderType.Market, 1, bar.Close, "Sell Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                        else
                            orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bars[i + 1].Time, Operation.Sell, OrderType.Market, 1, bars[i + 1].Open, "Sell Market", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                        Log("Add close order: id = {0}, operation = {1}", order_id, "Sell");
                    }
                }

                DispatchOrders(bar);

                if (closeIndex.Count > 0)
                {
                    List<int> keys = closeIndex.Keys.ToList<int>();
                    for (int j = 0; j < keys.Count; j++)
                    {
                        Position checkPosition = positions.Find(delegate(Position findPos)
                                            {
                                                return findPos.OpenOrderID == keys[j];
                                            }
                                        );

                        if (checkPosition == null)
                        {
                            closeIndex.Remove(keys[j]);
                            keys.Remove(keys[j]);
                            j--;
                        }
                        else 
                        {
                            if (closeIndex[checkPosition.OpenOrderID] == bar.IntradayIndex)
                            {
                                order_id++;
                                Operation op = checkPosition.Operation == Operation.Buy ? Operation.Sell : Operation.Buy;
                                if (parameters.OrderPlacingMode == OrderPlacing.ForCurrentClose)
                                    orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bar.Time, op, OrderType.Market, 1, bar.Close, "CloseByTrLf", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                                else
                                    orders.Add(new Order(parameters.Account, parameters.Symbol, order_id, bars[i + 1].Time, op, OrderType.Market, 1, bars[i + 1].Open, "CloseByTrLf", "Ema=" + _ema.Data[i].Value1.ToString() + "|" + _ema.Data[i].Time));
                            }
                        }
                    }

                    DispatchOrders(bar);
                }

                pos = PositionPerDirection(positions);

                if (pos != 0)
                    if (bar.DayOWeekIndex == 5)
                        if (bar.IntradayIndex >= 20)
                        {
                            Trade(ref positions, bar);
                            positions.Clear();
                            orders.Clear();
                            pos = PositionPerDirection(positions);
                            Log("ClosePositionEoR");
                        }
            }

            Bar lastBar = parameters.OrderPlacingMode == OrderPlacing.ForCurrentClose ? bars[bars.Count - 1] : bars[bars.Count - 2];
            Trade(ref positions, lastBar);
            positions.Clear();
            orders.Clear();
            Log("Close Position for Last Bar");
        }

        public StrategyAdditionalParameter AdditionalParameter(AdditionalParametersEnum parameter)
        {
            switch (parameter)
            {
                case AdditionalParametersEnum.MaxPositionsPerDirection:
                    {
                        return (from item in additional where (item.Name == "MaxPositionsPerDirection") select item).ElementAt(0);
                    }
                case AdditionalParametersEnum.MAChange:
                    {
                        return (from item in additional where (item.Name == "MAChange") select item).ElementAt(0);
                    }
                case AdditionalParametersEnum.TradeLife:
                    {
                        return (from item in additional where (item.Name == "TradeLife") select item).ElementAt(0);
                    }
                default:
                    {
                        throw new ArgumentException("Unknown AdditionalParameter in EmaForMarket");
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

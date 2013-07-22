using System;
using System.Collections.Generic;
using Reversals.Backtests;
using Reversals.DataContainer;

namespace Reversals.Strategies
{

    public class Strategy
    {
        #region PARAMETERS STRUCTURES
        public struct StrategyParameters
        {
            public int Account;
            public string Symbol;
            public string Name;

            public StrategyParameters(int account, string symbol, string name)
            {
                Account = account;
                Symbol = symbol;
                Name = name;
            }
        }

        public struct StrategyAdditionalParameter
        {
            public string Strategy;
            public string Name;
            public object Value;
            public Type Type;
            public bool Optimizable;

            public StrategyAdditionalParameter(string strategy, string name, object value, Type type, bool optimizable)
            {
                Strategy = strategy;
                Name = name;
                Value = value;
                Type = type;
                Optimizable = optimizable;
            }
        }

        #endregion

        #region EVENT HANDLERS
        public delegate void ProgressEventHandler();
        public event ProgressEventHandler ProgressEvent;
        #endregion

        #region VARIABLES
        public bool IsOptimisation;
        protected List<Tick> ticks = new List<Tick>();
        protected StrategyParameters parameters = new StrategyParameters();
        protected List<StrategyAdditionalParameter> additional = new List<StrategyAdditionalParameter>();
        protected List<Position> positions = new List<Position>();
        protected List<Position> trades = new List<Position>();
        protected List<Order> orders = new List<Order>();
        protected List<Order> orderOperationsList = new List<Order>(); 
        protected Backtest backtest;
        protected int order_id = 0;
        protected bool initialized;
        protected bool EnableLogging;
        #endregion

        #region CONSTRUCTORS
        public Strategy()
        {
            ticks = new List<Tick>();
            parameters = new StrategyParameters();
        }

        //public Strategy(StrategyParameters _parameters, List<StrategyAdditionalParameter> _additional, ref Data _data)
        //{
        //    parameters = _parameters;
        //    additional = _additional;
        //    ticks = _data.Bars;
        //}
        #endregion

        #region MAIN METHODS
        public virtual void Start(int startBarIndex)
        {

        }

        public virtual void Clear()
        {
            backtest = null;
            orders.Clear();
            positions.Clear();
            trades.Clear();


        }

        public virtual void Trade(ref List<Position> pos, Tick tick)
        {
            //foreach (Position _position in _positions)
            for (int i = 0; i < pos.Count; i++)
            {
                var position = pos[i];
                order_id++;
                position.Comment = "Closed for Exit";
                position.TimeClose = tick.Time;
                position.Last = tick.Close;
                position.CloseOrderId = order_id;
                trades.Add(position);
            }
        }

        protected virtual void DispatchOrders(Tick tick)
        {
            if (backtest == null)
                backtest = new Backtest(orders, positions, trades);
            backtest.DispatchOrders(tick, out orders, out positions, out trades);
        }

        public void Log(string format, params object[] args)
        {
            if (EnableLogging)
            {
                string msg = string.Format(format + "\r\n", args);
                System.IO.File.AppendAllText(@"LogStrategy.txt", msg);
            }
        }
        #endregion

        #region PROPERTIES
        public StrategyParameters Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        public List<StrategyAdditionalParameter> AdditionalParameters
        {
            get { return additional; }
            set { additional = value; }
        }

        public StrategyAdditionalParameter AdditionalParameter()
        {
            return new StrategyAdditionalParameter();
        }

        public bool Initialized
        {
            get { return initialized; }
            set { initialized = value; }
        }

        public virtual List<Position> Positions
        {
            get { return positions; }
        }

        public virtual List<Order> Orders
        {
            get { return orders; }
        }

        public virtual List<Order> OrderOperationsList
        {
            get { return orderOperationsList; }
        }

        public virtual List<Position> Trades
        {
            get { return trades; }
        }

        public virtual List<Tick> Ticks
        {
            get { return ticks; }
            set { ticks = value; }
        }
        #endregion

        #region EVENTS

        protected void NotifyProgress()
        {
            if (ProgressEvent != null)
            {
                ProgressEvent();
            }
        }

        #endregion
    }
}

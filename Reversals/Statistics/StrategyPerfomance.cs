using System.Collections.Generic;
using Reversals.Backtests;
using Reversals.Backtests.Enums;

namespace Reversals.Statistics
{
    public class StrategyPerfomance
    {
        #region VARIABLES
        private int _entireTradesCount;
        private int _lossTradesCount;
        private int _profitTradesCount;
        private double _entireTradesSum;  
     
        private readonly double _lossTradesSum;
        private readonly double _profitTradesSum;         
        private readonly double _profitability;
        private readonly double _profitFactor;
        private readonly double _averageTrade;

        private int _reversals;
        #endregion

        #region CONSTRUCTOR
        public StrategyPerfomance(List<Position> data)
        {
            CalculateTradesCount(data);
            CalculateTradesSum(data);

            _profitability = (_entireTradesCount != 0) ? (100 * _profitTradesCount) / _entireTradesCount : 0.0;
            _profitFactor = (_lossTradesSum != 0) ? (-_profitTradesSum / _lossTradesSum) : 0.0;
            _averageTrade = (_entireTradesCount != 0) ? (_entireTradesSum / _entireTradesCount) : 0;
        }
        #endregion

        #region CALCULATING METHODS
        private void CalculateTradesCount(List<Position> data)
        {
            int ltc = 0;
            int ptc = 0;

            for (int i = 0; i < data.Count; i++)
            {
                var position = data[i];
                if (position.Trades > 0) ptc++;
                if (position.Trades < 0) ltc++;
            }

            int etc = ptc + ltc;

            _entireTradesCount = etc;
            _lossTradesCount = ltc;
            _profitTradesCount = ptc;
        }

        private void CalculateTradesSum(List<Position> data)
        {
            double ets = 0;
            _reversals = 0;
            for (int i = 0; i < data.Count; i++)
            {
                var position = data[i];

                if (position.Operation != Operation.PNL)
                {
                    ets += position.Trades;
                }
                else if (position.Operation == Operation.PNL)
                {
                    ets += position.Commission + position.ClosePNL + position.PosPNL;
                }

                if (!position.IsPremium && position.Comment != "Closed for Exit")
                {
                    _reversals++;
                }
            }
                _entireTradesSum = ets;
        }
        #endregion

        #region PROPERTIES
        public int EntireTradesCount
        {
            get { return _entireTradesCount; }
        }

        public int Reversals
        {
            get { return _reversals; }
        }

        public int LossTradesCount
        {
            get { return _lossTradesCount; }
        }

        public int ProfitTradesCount
        {
            get { return _profitTradesCount; }
        }

        public double EntireTradesSum
        {
            get { return _entireTradesSum; }
        }

        public double LossTradesSum
        {
            get { return _lossTradesSum; }
        }

        public double ProfitTradesSum
        {
            get { return _profitTradesSum; }
        }

        public double Profitability
        {
            get { return _profitability; }
        }

        public double ProfitFactor
        {
            get { return _profitFactor; }
        }

        public double AverageTrade
        {
            get { return _averageTrade; }
        }
        #endregion
    }
}

using System;
using Reversals.DbDataManager.Structs;

namespace Reversals.DataSetManager
{
    class ParametersDataSet
    {
        #region VARIABLES
        private string _dataSetName;
        private double _timeValue;
        private double _commission;
        private int _multiplier;
        private int _contractSize;
        private double _pointValue;
        private double _zim;
        private double _tickSize;

        private double _stopLevel;
        private double _reversalLevel;
        private double _defaultMinValue;
        private double _defaultMaxValue ;
        private double _defaultStepValue;
        #endregion

        #region CONSTRUCTORS
        public ParametersDataSet(DataSetModel dataSet)
        {
            _dataSetName = dataSet.DataSetName;
            _timeValue = Math.Round(dataSet.TimeValue,2);
            _commission = Math.Round(dataSet.Commission,2);
            _multiplier = dataSet.Multiplier;
            _contractSize = dataSet.ContractSize;
            _pointValue = Math.Round(dataSet.PointValue,5);
            _zim = Math.Round(dataSet.Zim,5);
            _tickSize = Math.Round(dataSet.TickSize, 2);

            _reversalLevel = 80;
            _defaultMinValue = 60;
            _defaultMaxValue = 100;
            _defaultStepValue = 10;
            CalcelateStepValues();
        }
        #endregion

        #region PARAMETERS CALCULATING
        public void CalcelateStepValues()
        {
            _stopLevel =  Math.Round(_zim / _pointValue, 2);
            _reversalLevel *= _tickSize;
            _defaultMinValue *= _tickSize;
            _defaultMaxValue *= _tickSize;
            _defaultStepValue *= _tickSize;
        }
        #endregion

        #region PROPERTIES
        public string DataSetName
        {
            get { return _dataSetName; }
        }
        public double TimeValue
        {
            get { return _timeValue; }
        }
        public double Commission
        {
            get { return _commission; }
        }
        public int Multiplier
        {
            get { return _multiplier; }
        }
        public int ContractSize
        {
            get { return _contractSize; }
        }
        public double PointValue
        {
            get { return _pointValue; }
        }
        public double Zim
        {
            get { return _zim; }
        }
        public double TickSize
        {
            get { return _tickSize; }
        }
        public double StopLevel
        {
            get { return _stopLevel; }
        }
        public double ReversalLevel
        {
            get { return _reversalLevel; }
        }
        public double MinValue
        {
            get { return _defaultMinValue; }
        }
        public double MaxValue
        {
            get { return _defaultMaxValue; } 
        }
        public double Step
        {
            get { return _defaultStepValue; }
        }

        #endregion
    }
}

using System;

namespace Reversals.DataContainer
{
    public class Tick
    {
        #region VARIABLES
        private DateTime _time;
        private readonly double _close;
        private readonly int _intradayIndex;
        private readonly int _addIntradayIndex;
        private readonly int _dayOWeekIndex;
        #endregion

        #region CONSTRUCTOR
        public Tick(DateTime date, double close, int dayIndex, int addIntradayIndex, int dayOWeekIndex)
        {
            _time = date;
            _close = close;
            _intradayIndex = dayIndex;
            _addIntradayIndex = addIntradayIndex;
            _dayOWeekIndex = dayOWeekIndex;
        }
        #endregion

        #region PROPERTIES
        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (value != null)
                {
                    _time = value;
                }
            }
        }

        public double Close
        {
            get { return _close; }
        }

        public int IntradayIndex
        {
            get { return _intradayIndex; }
        }

        public int AddIntradayIndex
        {
            get { return _addIntradayIndex; }
        }

        public int DayOWeekIndex
        {
            get { return _dayOWeekIndex; }
        }
        #endregion
    }
}

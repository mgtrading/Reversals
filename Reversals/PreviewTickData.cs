using System;
using System.Collections.Generic;
using System.Linq;
using Reversals.DbDataManager;
using Reversals.DbDataManager.Structs;

namespace Reversals
{
    public static class PreviewTickData
    {
        private static List<TickDataModel> _cashTickDataList = new List<TickDataModel>();

        private static bool _tickDataExists;
        private static string _symbol;
        private static DateTime _startDate;
        private static DateTime _endDate;

        public static void SetCurrentSymbol(string symbol)
        {
            _symbol = symbol;
            _tickDataExists = DataManager.GetTickStartEndDates(_symbol, 
                                                                      out _startDate,                      
                                                                      out _endDate);
            _cashTickDataList.Clear();

            if (_tickDataExists)
            {                
                _cashTickDataList = DataManager.GetTickData(_symbol, _startDate);
            }            
        }

        public static List<TickDataModel> GetTickData(DateTime date)
        {
            if (!_tickDataExists) return null;

            if (!_cashTickDataList.Exists(a => a.Date.Date == date.Date))
            {
                _cashTickDataList.AddRange(DataManager.GetTickData(_symbol, date.Date));
            }
            return _cashTickDataList.Where(a => a.Date.Date == date.Date).ToList();
        }

        public static DateTime TickStartDate
        {
            get {
                return _tickDataExists ? _startDate : DateTime.MinValue;
            }
        }
        public static DateTime TickEndDate
        {
            get
            {
                return _tickDataExists ? _endDate : DateTime.MinValue;
            }
        }

        public static bool TickDataExists
        {
            get { return _tickDataExists; }

        }

    }

  
}

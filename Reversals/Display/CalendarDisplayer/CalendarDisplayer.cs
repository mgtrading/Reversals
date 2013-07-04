using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DevComponents.Schedule.Model;
using DevComponents.DotNetBar.Schedule;
using Reversals.DateFormats;
using Reversals.DbDataManager;
using Reversals.DbDataManager.Structs;

namespace Reversals.Display.CalendarDisplayer
{
    class CalendarDisplayer
    {
        #region VARIABLES
        private List<Appointment> _appointments;
        private eCalendarView _calendarViewMode;
        private List<ResultModel> _calendarResult;
        private List<ResultModel> _weeklist;
        private string _previousWeekValue;
        private string[] _weeksInMonth;
        private bool _isSixWeekInMonth;
        private string _monthlyTotal;
        private DateTime _startCalendarTime;
        private DateTime _endCalendarTime;
        private DateTime _monthViewStartDate;
        private DateTime _monthViewEndDate;
        private WeeklyDataDisplayer _weekData;
        private double[] MonthProfit;
        private string _currentMonthName;
        private bool _isLoadedFromFile;


          
        #endregion
      
        #region CONSTRUCTOR
        public CalendarDisplayer(WeeklyDataDisplayer weekData)
        {
            _weekData = weekData;
            MonthProfit = new double[12];
           
            _calendarViewMode = eCalendarView.Month;
            _weeksInMonth = new string[6];
            _calendarResult = new List<ResultModel>();
            _weeklist = new List<ResultModel>();
            _appointments = new List<Appointment>();
             CreateCalendarItems(); 
        }
        public CalendarDisplayer(List<ResultModel> list )
        {
            
            MonthProfit = new double[12];
            
            _calendarViewMode = eCalendarView.Month;
            _weeksInMonth = new string[6];
            _calendarResult = new List<ResultModel>();
            _weeklist = new List<ResultModel>();
            _appointments = new List<Appointment>();
            LoadCalendarItems(list);
        }
        #endregion

        #region Create Calendar Items From Test Results
        private void CreateCalendarItems()
        {
            _isLoadedFromFile = false;
            _appointments.Clear();
            _calendarResult.Clear();
         
             var nfi = new CultureInfo("en-US", false).NumberFormat;

            for (int i = 0; i < MonthProfit.Length; i++) MonthProfit[i] = 0;

            var result1 = from item in _weekData.DayDataTable.AsEnumerable() select item;
            var tableDaysRows = result1 as WeeklyData.tableDaysRow[] ?? result1.ToArray();
            int count = tableDaysRows.ToArray().Length;

           var  datetimetemp = DateTime.ParseExact(tableDaysRows.ToArray().ElementAt(0).Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            _startCalendarTime = new DateTime(datetimetemp.Year,datetimetemp.Month,datetimetemp.Hour);
            datetimetemp = DateTime.ParseExact(tableDaysRows.ToArray().ElementAt(count - 1).Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            _endCalendarTime = new DateTime(datetimetemp.Year, datetimetemp.Month, datetimetemp.Hour);

            _monthViewStartDate = new DateTime(_startCalendarTime.Year, _startCalendarTime.Month, 1);
            _monthViewEndDate = new DateTime(_endCalendarTime.Year, _startCalendarTime.Month + 1, 1);


            foreach (WeeklyData.tableDaysRow dataRow in tableDaysRows.ToArray())
            {

                var dateTimeStart = DateTime.ParseExact(dataRow.Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
                
                if (dateTimeStart.DayOfWeek != DayOfWeek.Sunday && dateTimeStart.DayOfWeek != DayOfWeek.Saturday)
                {
                    var appointment = new Appointment();
                    var tt = new DateTime(dateTimeStart.Year, dateTimeStart.Month, dateTimeStart.Day);
                    appointment.StartTime = tt;
                    
                    CalcMonthProfit(dateTimeStart, dataRow.PNL);
                    appointment.EndTime = appointment.StartTime.AddDays(1);


                    appointment.CategoryColor = dataRow.PNL > 0 ? Appointment.CategoryGreen : Appointment.CategoryRed;

                    appointment.Locked = true;
                    appointment.Subject = Math.Round(dataRow.PNL, 2).ToString(nfi);
                    appointment.TimeMarkedAs = Appointment.TimerMarkerDefault;

                    var item = new ResultModel {Date = appointment.StartTime, Pnl = Math.Round(dataRow.PNL, 2)};
                    _calendarResult.Add(item);
                    
                    _appointments.Add(appointment);
                
                    int month = _monthViewStartDate.Month;

                   
                        GetCalendarWeeksValue(month);
                 

                }
            }
        }
        private void GetCalendarWeeksValue(int monthIndex)
        {

            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
          
          
         
            var result1 = from item in _weekData.WeekDataTable.AsEnumerable()
                          where
                              DateTime.ParseExact(item.Start_Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture).Month == monthIndex
                          select item;

            var mfi = new DateTimeFormatInfo();

            _currentMonthName = mfi.GetMonthName(monthIndex);

            double monthly = MonthProfit[monthIndex - 1];
            _monthlyTotal  = monthly.ToString("n", nfi);
            const string emptyString = "";
            for (int i = 0; i < 6;i++ )
                _weeksInMonth.SetValue(emptyString, i);


            bool isSixWeek;
            var tempWeek = DateTime.ParseExact(result1.ToArray()[0].Start_Date,
                                               DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss",
                                               CultureInfo.InvariantCulture);
            var sixWeek = new DateTime(tempWeek.Year, tempWeek.Month, 1);

            if (sixWeek.DayOfWeek == DayOfWeek.Saturday)
                isSixWeek = true;
            else if (sixWeek.DayOfWeek == DayOfWeek.Friday && DateTime.DaysInMonth(sixWeek.Year, sixWeek.Month) == 31)
                isSixWeek = true;
            else isSixWeek = false;

            _isSixWeekInMonth = isSixWeek;

            if (isSixWeek)
            {
                var sixthW = from item in _weekData.WeekDataTable.AsEnumerable()
                             where
                                 DateTime.ParseExact(item.Start_Date,
                                                     DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss",
                                                     CultureInfo.InvariantCulture).Month == monthIndex + 1
                             select item;
                if (sixthW.ToArray().Length != 0)
               _weeksInMonth[5] = sixthW.ToArray()[0].PNL.ToString("n", nfi);

            }
            else _weeksInMonth[5] = "";


            for (int i = 0; i < result1.ToArray().Count(); i++)
            {
                WeeklyData.tableWeekRow wRow = result1.ToArray()[i];
                var dtt = DateTime.ParseExact(wRow.Start_Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture);
                int weeknumber = GetWeekOfMonth(new DateTime(dtt.Year, dtt.Month, dtt.Day));
                switch (weeknumber)
                {
                    case 1:
                        _weeksInMonth[0] =  wRow.PNL.ToString("n", nfi);
                        break;
                    case 2:
                        _weeksInMonth[1] = wRow.PNL.ToString("n", nfi);
                        break;
                    case 3:
                        _weeksInMonth[2] = wRow.PNL.ToString("n", nfi);
                        break;
                    case 4:
                        _weeksInMonth[3] = wRow.PNL.ToString("n", nfi);
                        break;
                    case 5:
                        _weeksInMonth[4] = wRow.PNL.ToString("n", nfi);
                        break;
                    

                }
                
            }
        }

        private static int GetWeekOfMonth(DateTime date)
        {
            var beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int)Math.Truncate(date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }
        private void CalcMonthProfit(DateTime date, double pnl)
        {
            switch (date.Month)
            {
                case 1:
                    MonthProfit[0] += pnl;
                    break;
                case 2:
                    MonthProfit[1] += pnl;
                    break;
                case 3:
                    MonthProfit[2] += pnl;
                    break;
                case 4:
                    MonthProfit[3] += pnl;
                    break;
                case 5:
                    MonthProfit[4] += pnl;
                    break;
                case 6:
                    MonthProfit[5] += pnl;
                    break;
                case 7:
                    MonthProfit[6] += pnl;
                    break;
                case 8:
                    MonthProfit[7] += pnl;
                    break;
                case 9:
                    MonthProfit[8] += pnl;
                    break;
                case 10:
                    MonthProfit[9] += pnl;
                    break;
                case 11:
                    MonthProfit[10] += pnl;
                    break;
                case 12:
                    MonthProfit[11] += pnl;
                    break;

            }
        }
      #endregion 

        #region Calendar Itmes From/To DB
        private void LoadCalendarWeeksValue(int monthIndex)
        {

            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;


            var mfi = new DateTimeFormatInfo();

            _currentMonthName = mfi.GetMonthName(monthIndex);

            double monthly = MonthProfit[monthIndex - 1];

           _monthlyTotal = monthly.ToString("n", nfi);

            const string emptyStr = "";
            for (int i = 0; i < _weeksInMonth.Length;i++ )
                _weeksInMonth.SetValue(emptyStr, i);
         
            int dayCounter = 0;
            int weekCounter = 0;

            _weeklist.Clear();


            var weekTimes = new DateTime[_calendarResult.Count];
            var pnlarray = new double[_calendarResult.Count];




            foreach (var calitem in _calendarResult)
            {
                if (dayCounter == 0)
                {
                    weekTimes[weekCounter] = calitem.Date;
                    dayCounter++;
                }
                if (calitem.Date.DayOfWeek == DayOfWeek.Monday)
                {
                    weekCounter++;
                    weekTimes[weekCounter] = calitem.Date;

                }

                pnlarray[weekCounter] += calitem.Pnl;
            }

            int cc = 0;

            while (cc <= weekCounter)
            {
                var rm = new ResultModel {Date = weekTimes[cc], Pnl = pnlarray[cc]};
                _weeklist.Add(rm);
                cc++;

            }

            var result1 = from item in _weeklist.AsEnumerable()
                          where
                              item.Date.Month == monthIndex
                          select item;


            bool isSixWeek;
            var tempdate = result1.ToArray()[0].Date;
            var sixWeek = new DateTime(tempdate.Year, tempdate.Month, 1);
            if (sixWeek.DayOfWeek == DayOfWeek.Saturday)
                isSixWeek = true;
            else if (sixWeek.DayOfWeek == DayOfWeek.Friday && DateTime.DaysInMonth(sixWeek.Year, sixWeek.Month) == 31)
                isSixWeek = true;
            else isSixWeek = false;
            _isSixWeekInMonth = isSixWeek;
            if (isSixWeek)
            {
                var sixthW = from item in _weeklist.AsEnumerable()
                             where item.Date.Month == monthIndex + 1
                             select item;
                if (sixthW.ToArray().Length != 0)
                    _weeksInMonth[5] = sixthW.ToArray()[0].Pnl.ToString("n", nfi);
            }
            else _weeksInMonth[5] = "";

            for (int i = 0; i < result1.ToArray().Length; i++)
            {
                var item = result1.ElementAt(i);
                var dtt = item.Date;
                int weeknumber = GetWeekOfMonth(dtt);
                switch (weeknumber)
                {
                    case 1:
                        _weeksInMonth[0] = item.Pnl.ToString("n", nfi);
                        break;
                    case 2:
                        _weeksInMonth[1] = item.Pnl.ToString("n", nfi);
                        break;
                    case 3:
                        _weeksInMonth[2] = item.Pnl.ToString("n", nfi);
                        break;
                    case 4:
                        _weeksInMonth[3] = item.Pnl.ToString("n", nfi);
                        break;
                    case 5:
                        _weeksInMonth[4] = item.Pnl.ToString("n", nfi);
                        break;

                }

            }



        }

        public void SaveCalendarItem(int symbolID, int dataSetId)
        {
           if (_calendarResult != null)
            {
                DataManager.AddResult(symbolID, dataSetId, _calendarResult);
            }
        }

        private void LoadCalendarItems(List<ResultModel> list )
        {
            
            _isLoadedFromFile = true;
         

            if (list.Count != 0)
            {
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

                for (int i = 0; i < MonthProfit.Length; i++) MonthProfit[i] = 0;
                _appointments.Clear();
                _calendarResult.Clear();
             


                int count = list.Count;
               
                _startCalendarTime = list.ElementAt(0).Date;
                _endCalendarTime = list.ElementAt(count - 1).Date;
                DateTime dateTimeStart = _startCalendarTime;
                _monthViewStartDate = new DateTime(dateTimeStart.Year,dateTimeStart.Month, 1);
                _monthViewEndDate = new DateTime(dateTimeStart.Year,dateTimeStart.Month + 1, 1);

                foreach (var calendaritem in list)
                {

                    dateTimeStart = new DateTime(calendaritem.Date.Year, calendaritem.Date.Month, calendaritem.Date.Day);
                    if (dateTimeStart.DayOfWeek != DayOfWeek.Sunday && dateTimeStart.DayOfWeek != DayOfWeek.Saturday)
                    {
                        var appointment = new Appointment {StartTime = dateTimeStart};

                        CalcMonthProfit(dateTimeStart, calendaritem.Pnl);
                        appointment.EndTime = appointment.StartTime.AddDays(1);


                        appointment.CategoryColor = calendaritem.Pnl > 0 ? Appointment.CategoryGreen : Appointment.CategoryRed;

                        appointment.Locked = true;
                        appointment.Subject = Math.Round(calendaritem.Pnl, 2).ToString(nfi);
                        appointment.TimeMarkedAs = Appointment.TimerMarkerDefault;
                        var item = new ResultModel {Date = appointment.StartTime, Pnl = Math.Round(calendaritem.Pnl, 2)};

                        _calendarResult.Add(item);

                        _appointments.Add(appointment);
                        }
                }
                int month = _monthViewStartDate.Month;
             
                    LoadCalendarWeeksValue(month);
             
            }


        }               
        #endregion

        #region Display Functions
        public  void MonthRangeIncrement()
        {
            if (_appointments.Count != 0)
            {

              var  cnt = _appointments.Count - 1;

                if ((_monthViewEndDate.Month - _appointments.ElementAt(cnt).StartTime.Month) <= 0)
                {
                    string str = _weeksInMonth[4];

                    _previousWeekValue = _weeksInMonth[4];
                    _monthViewStartDate = _monthViewStartDate.AddMonths(1);
                    _monthViewEndDate = _monthViewEndDate.AddMonths(1);
                    if(_isLoadedFromFile)
                        LoadCalendarWeeksValue(_monthViewStartDate.Month);
                    else
                    GetCalendarWeeksValue(_monthViewStartDate.Month);

                    if ( _weeksInMonth[0] == "")
                    {
                        _weeksInMonth[0] = str;
                    }
                }
            }
        }
        public void MonthRangeDecrement()
        {
            if (_appointments.Count != 0)
            {
                if (!(_startCalendarTime.Month == _monthViewStartDate.Month))
                {
                    _monthViewEndDate = _monthViewEndDate.AddMonths(-1);
                    _monthViewStartDate = _monthViewStartDate.AddMonths(-1);
                    const string str = "";
                    for (int i = 0; i < _weeksInMonth.Length; i++)
                        _weeksInMonth.SetValue(str, i);

                    if (_isLoadedFromFile)
                        LoadCalendarWeeksValue(_monthViewStartDate.Month);
                    else
                    GetCalendarWeeksValue(_monthViewStartDate.Month);

                    if (_startCalendarTime.Month != _monthViewStartDate.Month)
                    {
                        if (_weeksInMonth[0] == "") _weeksInMonth[0] = _previousWeekValue;
                    }
                }

            }
        }

        #endregion

        #region PROPERTIES
         public List<Appointment> Appointments
        {

            get {return _appointments; }
        }
         public  string WeekOne
        {
            get { return _weeksInMonth[0]; }
        }
         public  string WeekTwo
        {
            get { return _weeksInMonth[1]; }
        }
         public  string WeekThree
        {
            get { return _weeksInMonth[2]; }
        }
         public  string WeekFour
        {
            get { return _weeksInMonth[3]; }
        }
         public  string WeekFive
        {
            get { return _weeksInMonth[4]; }
        }
         public string WeekSix
         {
             get { return _weeksInMonth[5]; }
         }
         public DateTime MonthViewStartDay
        {
            get { return _monthViewStartDate; }
        }
         public DateTime MonthViewEndDay
        {
            get { return _monthViewEndDate; }
        }
         public string MonthlyTotal
        {
            get { return _monthlyTotal; }
        }
        public bool IsSixWeekInMonth
        {
            get { return _isSixWeekInMonth; }
        }
        public eCalendarView CalendarViewMode
        {
            get { return _calendarViewMode; }
        }
        public string CurrentMonthName
        {
            get { return _currentMonthName; }
        }
      
        #endregion
    }
}

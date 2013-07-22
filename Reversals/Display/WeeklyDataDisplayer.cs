using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using System.Linq;
using Reversals.Backtests;
using Reversals.Backtests.Enums;
using Reversals.DateFormats;
using Reversals.Display.BaseDisplayer;
using Reversals.Strategies;

namespace Reversals.Display
{
    class WeeklyDataDisplayer : Displayer
    {


        private int _tradeindexer, _dayindexer, _weekindexer;
        private int _tempweek;
        private int _currentweek;
        private int _tempDayOfYear;
        private int _currDayOfYear;
        private bool _isFullWeek;
        private bool _isNewDay;
        private List<Position> _tempTrades;
        public WeeklyData _weeklyData;
        private DateTime _startWeek;
        private DateTime _endWeek;
        private DateTime _currWeek;
        private int _daycounter;
        private double _dayOpenPrice;
        private double _dayClosePrice;
        private double _nextDayOpenPrice;
        private double _nextDayClosePrice;
        private DateTime _dayDate;
        private ArrayList templist;
        private List<Order> _orderList;

        private string _dateTimeFormat;
        enum TableID
        {
            TradeTable,
            DayTable,
            WeekTable
        }

        public WeeklyDataDisplayer(Strategy vtrades, bool visIntraDay, DateTime startTime, DateTime endTime)
            : base(vtrades,  startTime, endTime)
        {
            DateTimeFormatInfo dtfInfo = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentCulture);
            _orderList = vtrades.OrderOperationsList;
            _dateTimeFormat = "dd" + dtfInfo.DateSeparator.ToString() + "MM" + dtfInfo.DateSeparator.ToString() + "yyyy" +
                              " HH:mm:ss";
            _dayindexer = 0;
            _weekindexer = 0;
            _tradeindexer = 0;
            _isFullWeek = false;
            _tempDayOfYear = Trades[0].TimeOpen.DayOfYear;
            _currDayOfYear = _tempDayOfYear;
            _currentweek = Trades[0].TimeOpen.DayOfYear;
            _tempweek = Trades[0].TimeOpen.DayOfYear;

            _currWeek = Trades[0].TimeOpen;
            _startWeek = Trades[0].TimeOpen;
            _endWeek = Trades[0].TimeOpen;
            _tempTrades = Trades;

            _weeklyData = new WeeklyData();

            _weeklyData.Relations.Add("1", _weeklyData.Tables["tableWeek"].Columns["id"],
                                   _weeklyData.Tables["tableDays"].Columns["id_Week"], false);
            _weeklyData.Relations.Add("2", _weeklyData.Tables["tableDays"].Columns["id"],
                                   _weeklyData.Tables["orderTable"].Columns["id_Day"], false);




            CreateDataTablesFromTrades();
        }




        public void CreateDataTablesFromTrades()
        {
            int i = 0;

            _tempTrades = Trades.OrderBy(pos => pos.TimeOpen).ToList();

            foreach (Position position in _tempTrades)
            {
              
                _currDayOfYear = position.TimeOpen.DayOfYear;
                _currWeek = position.TimeOpen;





                if (position.Operation == Operation.Premium)
                {
                    if ((position.TimeOpen.DayOfWeek != DayOfWeek.Sunday &&
                         position.TimeOpen.DayOfWeek != DayOfWeek.Saturday))
                    {

                   
                        _dayDate = new DateTime(position.TimeOpen.Year, position.TimeOpen.Month, position.TimeOpen.Day,
                                                17, 00, 00);
                        CalculateTradeParams(position);
                        CalculateDayParams();
                        CalculateWeekParams();
                        _isNewDay = true;
                        _dayindexer++;
                        _daycounter++;
                    }
                    else
                    {
                        CalculateTradeParams(position);
                        CalculateDayParams();
                        CalculateWeekParams();
                    }
                }
                else
                {
                    CalculateTradeParams(position);
                    CalculateDayParams();
                    CalculateWeekParams();
                }


                if (position.TimeOpen.DayOfWeek == DayOfWeek.Friday && position.Operation == Operation.Premium)
                {

                    _isFullWeek = true;
                    _weekindexer++;
                    _startWeek = _currWeek;
                    _daycounter = 0;

                }
                _tempDayOfYear = _currDayOfYear;


            }

            DeleteLastPNL();
            CalculateOrderParams();
            //todo at there place need to call CreateOrderTable()
        }

private void CalculateOrderParams()
{
    bool _firstOrderDay;
    foreach(var row in _weeklyData.tableDays)
    {

        DateTime asd = DateTime.ParseExact(row.Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture);
        var ad = asd.Hour + asd.Minute/100 + asd.Second/100;
        int _orderDayIndexer = row.id;
        int rowDay =DateTime.ParseExact(row.Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss",CultureInfo.InvariantCulture).DayOfYear;
        //var orderResults = from item in _orderList.AsEnumerable() where
        //                                                              (item.Time.DayOfYear == rowDay 
        //                                                                        &&
        //                                                              (item.Time.Hour + item.Time.Minute/100 + item.Time.Second/100) <=17)
        //                                                                        ||
        //                                                            (  rowDay-1 == item.Time.DayOfYear 
        //                                                                        &&
        //                                                              (item.Time.Hour + item.Time.Minute / 100 + item.Time.Second / 100 )> 17)
        //                                                                        ||
        //                                                             ( item.Time.DayOfWeek == DayOfWeek.Friday 
        //                                                                        &&
        //                                                              (item.Time.Hour + item.Time.Minute / 100 + item.Time.Second / 100) > 17)


        //                                                          select item;
       
        foreach(var  position in _orderList.ToArray())
        {
            bool canAddIt = false;
            int indexer = _orderDayIndexer;
            if(position.Time.DayOfYear == rowDay)
            {
                double hourindex = position.Time.Hour;
                double minuteindex = position.Time.Minute;
                double secondindex = position.Time.Second;
                double dayindex = hourindex + (minuteindex + secondindex)/100;
                if (dayindex <= 17.00)
                    canAddIt = true;
                else
                   
                {
                    indexer++;
                    canAddIt = true;
                }

            }
            if (canAddIt)
            {
                var trValues = new ArrayList
                                   {
                                       null,
                                       indexer,
                                       position.Time.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss"),
                                       position.Operation,
                                       position.Price,
                                   };


                DataRow drow = _weeklyData.orderTable.NewRow();
                drow.ItemArray = trValues.ToArray();
                _weeklyData.orderTable.Rows.Add(drow);
                _weeklyData.AcceptChanges();
                _orderList.Remove(position);
            }
        }
    }
}
        private void CalculateWeekParams()
        {
            int count = _weeklyData.tableWeek.Rows.Count;

            if ((!_isFullWeek) && (count != 0))
                _weeklyData.tableWeek.Rows.Remove(_weeklyData.tableWeek.Rows[count - 1]);
            //if (_dayindexer != 1)
            //{
                DataRow row = _weeklyData.tableWeek.NewRow();
                row.ItemArray = CalculateData(TableID.WeekTable).ToArray();
                _weeklyData.tableWeek.Rows.Add(row);
                _weeklyData.AcceptChanges();
                _isFullWeek = false;
            //}
            

        }


        private void CalculateDayParams()
        {
            int count = _weeklyData.tableDays.Rows.Count;
           // if (_dayindexer != 0)
                if ((!_isNewDay) && (count != 0))
                    _weeklyData.tableDays.Rows.Remove(_weeklyData.tableDays.Rows[count - 1]);

            //if (_dayindexer != 1)
            //{
                DataRow row = _weeklyData.tableDays.NewRow();
                row.ItemArray = CalculateData(TableID.DayTable).ToArray();
                _weeklyData.tableDays.Rows.Add(row);
                _weeklyData.AcceptChanges();
                _isNewDay = false;
            //}
            //next day

        }

        private void ReplaceRows()
        {

            var result = from item in _weeklyData.tableDays.AsEnumerable() select item;

            int cnt = 0;
            double tempsumPremiumSumma,
                   tempsumtradeprofit,
                   tempsumcommission,
                   tempsumpospnl,
                   tempsumclosepnl,
                   tempsumpnl,
                   tempsummaryReversal;
            tempsumPremiumSumma =
                tempsumtradeprofit =
                tempsumcommission =
                tempsumpospnl =
                tempsumclosepnl =
                tempsumpnl =
                tempsummaryReversal = 0;

            foreach (WeeklyData.tableDaysRow dataRow in result.ToArray())
            {
                if (cnt > 0)
                {
                    dataRow.Commission = tempsumcommission;
                    dataRow.ClosePNL = tempsumclosepnl;
                    dataRow.PNL = tempsumpnl;
                    dataRow.Trades = tempsumtradeprofit;
                }
                tempsumtradeprofit = dataRow.Trades;
                tempsumcommission = dataRow.Commission;
                tempsumclosepnl = dataRow.ClosePNL;
                tempsumpnl = dataRow.PNL;

            }
            _weeklyData.AcceptChanges();
        }

        private void DeleteLastPNL()
        {
            var result = from item in _weeklyData.tableTrades.AsEnumerable() where (item.id_Day == _dayindexer-1) select item;

            foreach (var dataRow in result.ToArray())
            {
                if (dataRow.Operation == "PNL")
                {
                    _weeklyData.tableTrades.Rows.Remove(dataRow);
                    _weeklyData.AcceptChanges();
                }
            }
        }

        private ArrayList CalculateData(TableID id)
        {
            EnumerableRowCollection<WeeklyData.tableTradesRow> result1 = null;
            EnumerableRowCollection<WeeklyData.tableDaysRow> result2 = null;
            EnumerableRowCollection<WeeklyData.tableTradesRow> result3 = null;

            double sumpospnl = 0;
            double sumclosepnl = 0;
            double sumtradeprofit = 0;
            double sumcommission = 0;
            double sumPremiumSumma = 0;
            double sumpnl = 0;
            int summaryReversal;
            int index = 0;
            string time;
            ArrayList valuelist = null;

            int newDayIndexer = 0;

            if (id == TableID.DayTable)
            {

                result1 = from item in _weeklyData.tableTrades.AsEnumerable() where (item.id_Day == _dayindexer) select item;
                summaryReversal = 0;


                foreach (WeeklyData.tableTradesRow dataRow in result1.ToArray())
                {

                    double commission;
                    double closepnl;
                    double pospnl;
                    double tradeprofit;
                    double premiumSumma;
                    double pnl;

                    string openprice = null;
                    string closeprice = null;


                    if (dataRow.Operation.ToString() == "Premium")
                    {
                        double.TryParse(dataRow.Trades.ToString(), out premiumSumma);
                        sumPremiumSumma += premiumSumma;

                    }

                    if (dataRow.Operation.ToString() == "PNL")
                    {
                        double.TryParse(dataRow.PosPNL.ToString(), out pospnl);
                        sumpospnl += Math.Round(pospnl, 2);
                        double.TryParse(dataRow.ClosePNL.ToString(), out closepnl);
                        sumclosepnl += Math.Round(closepnl, 2);
                        double.TryParse(dataRow.Commission.ToString(), out commission);
                        sumcommission += Math.Round(commission, 2);


                    }

                    if (dataRow.Operation != "PNL" && dataRow.Operation != "Premium")
                    {
                        double.TryParse(dataRow.Trades.ToString(), out tradeprofit);
                        sumtradeprofit += Math.Round(tradeprofit, 2);
                    }

                    if (dataRow.Operation != "PNL" && dataRow.Operation != "Premium" && dataRow.Comment != "Closed for Exit")
                    {
                        summaryReversal += 1;
                    }




                    sumpnl = sumpospnl + sumclosepnl + Math.Round(sumtradeprofit + sumcommission + 0.5) + sumPremiumSumma;

                    string sumPremium = FormatNumber(sumPremiumSumma);
                    string sumTrProf = FormatNumber(sumtradeprofit);
                    string sumComiss = FormatNumber(sumcommission);
                    string sumPosPNL = FormatNumber(sumpospnl);
                    string sumClPNL = FormatNumber(sumclosepnl);
                    string sumPNL = FormatNumber(sumpnl);
                    //decimal summm = Decimal.Parse(sumTrProf, Nfi);
                    if (_dayDate.DayOfWeek == DayOfWeek.Friday && _isFullWeek)
                     _dayDate =    _dayDate.AddDays(3);

                    valuelist = new ArrayList
                          { 
                               _dayindexer,
                               _weekindexer,
                               _dayDate.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss"),//result1.ToArray().ElementAt(result1.ToArray().Length - 1).TimeClose,
                               sumPremiumSumma, 
                               Math.Round(sumtradeprofit + sumcommission + 0.5), 
                               sumcommission, 
                               sumpospnl, 
                               sumclosepnl, 
                               sumpnl, 
                               summaryReversal, 
                               _dayOpenPrice,
                               _dayClosePrice
                          };

                }

                result3 = from item in _weeklyData.tableTrades.AsEnumerable() where (item.id_Day == _dayindexer - 1) select item;


                foreach (WeeklyData.tableTradesRow dataRow in result3.ToArray())
                {
                    if (dataRow.Operation == "PNL")
                    {
                        _weeklyData.tableTrades.Rows.Remove(dataRow);
                        _weeklyData.AcceptChanges();
                    }
                }
            }
            else
                if (id == TableID.WeekTable)
                {
                    
                    result2 = from item in _weeklyData.tableDays.AsEnumerable() where (item.id_Week == _weekindexer) select item;
                    summaryReversal = 0;

                    foreach (WeeklyData.tableDaysRow dataRow in result2.ToArray())
                    {
                        double commission;
                        double closepnl;
                        double pospnl;
                        double tradeprofit;
                        double premiumSumma;
                        double pnl;
                        int reversal = 0;
                        int firstday = 0;
                        DateTime wstarttime;

                      
                        if (firstday == 0)
                        {
                            _startWeek = DateTime.ParseExact(dataRow.Date, DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture);
                            firstday++;
                        }


                        double.TryParse(dataRow.Time_Value.ToString(), out premiumSumma);
                        sumPremiumSumma += premiumSumma;

                        double.TryParse(dataRow.PosPNL.ToString(), out pospnl);
                        sumpospnl += pospnl;

                        double.TryParse(dataRow.ClosePNL.ToString(), out closepnl);
                        sumclosepnl += closepnl;
                        double.TryParse(dataRow.Commission.ToString(), out commission);
                        sumcommission += commission;
                        double.TryParse(dataRow.PNL.ToString(), out  pnl);
                        sumpnl += pnl;

                        int.TryParse(dataRow.Reversal.ToString(), out reversal);
                        summaryReversal += reversal;

                        double.TryParse(dataRow.Trades.ToString(), out tradeprofit);
                        sumtradeprofit += tradeprofit;

                        string sumPremium = FormatNumber(sumPremiumSumma);
                        string sumTrProf = FormatNumber(sumtradeprofit);
                        string sumComiss = FormatNumber(sumcommission);
                        string sumPosPNL = FormatNumber(sumpospnl);
                        string sumClPNL = FormatNumber(sumclosepnl);
                        string sumPNL = FormatNumber(sumpnl);
                   
                        valuelist = new ArrayList
                          { 
                             _weekindexer,
                             result2.ToArray().ElementAt(0).Date,
                             _startWeek.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss"),
                             sumPremiumSumma, 
                             sumtradeprofit, 
                             sumcommission, 
                             sumpospnl, 
                             sumclosepnl, 
                             sumpnl, 
                             summaryReversal                         
                          };
                    }

                }



            return valuelist;
        }

        private void CalculateTradeParams(Position position)
        {
            _tradeindexer++;
            //string positionstr = Math.Round(position.FPL, 2).ToString();

            string price = FormatNumber(position.Price);
            string last = FormatNumber(position.Last);
            string fpl = FormatNumber(Math.Round(position.Trades, 2));
            string clPnl = FormatNumber(Math.Round(position.ClosePNL, 2));
            string comission = FormatNumber(Math.Round(position.Commission, 2));
            string posPNL = FormatNumber(Math.Round(position.PosPNL, 2));
            string margin = FormatNumber(Math.Round(position.Margin, 2));

            if (position.Operation == Operation.PNL)
            {
                _dayOpenPrice = position.Price;
                _dayClosePrice = position.Last;
                //_nextDayOpenPrice = position.Price;
                //_nextDayClosePrice = position.Last;
                last = " ";
                price = "";
            }

            var trValues = new ArrayList
                           {   
                              null,
                               _dayindexer,
                               position.TimeOpen.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss"),
                               position.TimeClose.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss"),
                               position.Operation,
                               position.Price, 
                               position.Last, 
                               Math.Round(position.Trades, 2), 
                               Math.Round(position.Commission, 2),
                               Math.Round(position.PosPNL, 2), 
                               Math.Round(position.ClosePNL, 2), 
                               position.Comment,                                                         
                           };


            DataRow row = _weeklyData.tableTrades.NewRow();
            row.ItemArray = trValues.ToArray();
            _weeklyData.tableTrades.Rows.Add(row);
            _weeklyData.AcceptChanges();
     

        }



        public BindingSource GetDaysOfWeek(int i)
        {
            var tableDays = new WeeklyData.tableDaysDataTable();
            BindingSource dayBindingSource = null;
            var result = from item in _weeklyData.tableDays.AsEnumerable() where (item.id_Week == i) select item;
            foreach (WeeklyData.tableDaysRow dataRow in result.ToArray())
            {
                tableDays.AddtableDaysRow(dataRow.id_Week, dataRow.Date, dataRow.Time_Value,
                    dataRow.Trades, dataRow.Commission, dataRow.PosPNL, dataRow.ClosePNL, dataRow.PNL, 0, 0, 0);


            }
            tableDays.AcceptChanges();
            dayBindingSource = new BindingSource(tableDays, null);
            return dayBindingSource;
        }

        public BindingSource GetTradesOfDay(int i)
        {
            var tableTrades = new WeeklyData.tableTradesDataTable();
            BindingSource tradeBindingSource = null;
            var result = from item in _weeklyData.tableTrades.AsEnumerable() where (item.id_Day == i) select item;
            foreach (WeeklyData.tableTradesRow dataRow in result.ToArray())
            {

                tableTrades.Rows.Add(dataRow);
            }
            tradeBindingSource = new BindingSource(tableTrades, null);
            return tradeBindingSource;
        }

        public BindingSource TradeData
        {
            get
            {

                var bsource = new BindingSource(_weeklyData.tableTrades, null);

                return bsource;
            }
        }

        public BindingSource DayData
        {
            get
            {
                var dayTable = new WeeklyData.tableDaysDataTable();
                dayTable = _weeklyData.tableDays;
                dayTable.Columns.Remove("id_Week");
                var bsource = new BindingSource(dayTable, null);
                return bsource;
            }
        }

        public BindingSource WeekData
        {
            get
            {

                var bsource = new BindingSource(_weeklyData.tableWeek, null);
                return bsource;
            }
        }

        public WeeklyData.tableWeekDataTable WeekDataTable
        {
            get { return _weeklyData.tableWeek; }
        }


        public WeeklyData.tableDaysDataTable DayDataTable
        {
            get { return _weeklyData.tableDays; }
        }

        public static int WeekNumber(System.DateTime value)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        }
    }
}

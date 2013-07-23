using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reversals.DbDataManager.Structs
{
    public struct WeeklyDataTradeModel
    {
        public int id_Day;
        public DateTime TimeOpen;
        public DateTime TimeClose;
        public string Operation;
        public double OpenPr;
        public double ClosePr;
        public double Trades;
        public double Commission;
        public double PosPNL;
        public double ClosePNL;
        public string Comment;
    }

    public struct WeeklyDataOrderModel
    {
        public DateTime Time;
        public string Operation;
        public double Price;
    }
}

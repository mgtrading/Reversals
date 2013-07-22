using System;

namespace Reversals.DbDataManager.Structs
{
    struct SummaryResultModel
    {
        public string SymbolName;
        public DateTime StartDate;
        public DateTime EndDate;
        public double Pnl;
        public int Reversals;
        public double StopLevel;
        public double ReversalLevel;
        public double Zim;
        public double PointValue;
    }
}

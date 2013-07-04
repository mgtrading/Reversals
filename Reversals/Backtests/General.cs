using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptionPR.Backtests
{
    #region Enum
    public enum Operation
    {
        Buy = 1,
        Sell = -1,
        Undefined = 0
    }

    public enum OrderType
    {
        Market = 1,
        Stop = 2,
        Limit = 3,
        StopLimit = 4,
        OCO = 5,
        Undefined = 0
    }

    public enum OrderState
    {
        Initialized = 1,
        Working = 2,
        Filled = 3,
        Completed = 4,
        Undefined = 0
    }

    public enum StrategyMode
    {
        Daily = 1,
        Intraday = 2,
        Undefined = 0
    }

    public enum IntradayModeEnum
    {
        EndOfDay = 1,
        Custom = 2,
        Undefined = 0
    }

    public enum Timeframe
    {
        M1 = 1,
        M2 = 2,
        M5 = 5,
        M10 = 10,
        M15 = 15,
        M30 = 30,
        M50 = 50,
        H1 = 60,
        H2 = 120,
        H4 = 240,
        D = 1440,
        W = 10080,
        M = 44640,
        Y = 525600,
        Undefined = 0
    }
    #endregion

    #region Struct

    public struct IntradayMode
    {
        public readonly IntradayModeEnum Mode;
        public readonly TimeSpan CustomSessionStart;
        public readonly TimeSpan CustomSessionEnd;

        public IntradayMode(IntradayModeEnum mode, TimeSpan start, TimeSpan end)
        {
            Mode = mode;
            CustomSessionStart = start;
            CustomSessionEnd = end;
        }
    }

    public struct Range
    {
        public bool IsUseRange;
        public int RangeStartTime;
        public int RangeEndTime;

        public Range(bool isUse, int start, int end)
        {
            IsUseRange = isUse;
            RangeStartTime = start;
            RangeEndTime = end;
        }
    }

    #endregion
}

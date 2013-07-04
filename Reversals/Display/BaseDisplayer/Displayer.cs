using System;
using System.Collections.Generic;
using System.Globalization;
using Reversals.Backtests;
using Reversals.Statistics;
using Reversals.Strategies;

namespace Reversals.Display.BaseDisplayer
{
    public class Displayer
    {
       protected readonly List<Position> Trades;
       protected DateTime       MinDt;
       protected  DateTime      MaxDt;
       protected readonly string         Symbol;
       protected readonly StrategyPerfomance Strategyperformance;
       protected readonly Strategy Strategy;
       //protected string _dateTimeFormat;
       protected NumberFormatInfo Nfi = new CultureInfo("en-US", false).NumberFormat;


       protected Displayer(Strategy vtrades, DateTime startDate, DateTime endTime)
       {
           Strategy = vtrades;
           Symbol = vtrades.Parameters.Symbol;
           Trades = vtrades.Trades;
           MinDt = startDate;
           MaxDt = endTime;
           Strategyperformance = new StrategyPerfomance(Trades);
       }

       public string FormatNumber(double number)
       {
           return number.ToString("n", Nfi);
       }
       public void Clear()
       {
           Trades.Clear();
       }
    }

  

}

 

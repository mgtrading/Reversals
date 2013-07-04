using System;
using System.Windows.Forms;
using System.Data;
using System.Collections;
using Reversals.Backtests;
using Reversals.Display.BaseDisplayer;
using Reversals.Strategies;

namespace Reversals.Display.TradeDisplayer
{
    public class TradeDisplayer : Displayer
    {
        public TradeDisplayer(Strategy vtrades, DateTime startTime, DateTime endTime) :

            base(vtrades, startTime, endTime) { }



        public void TradeTableDisplayer(DataGridView tradegrid)
        {
            var provider = tradegrid.DataSource as DataTable;

            var trValues = new ArrayList();
            foreach (Position position in Trades)
            {

                trValues.Add(position.Account);
                trValues.Add(position.Symbol);
                trValues.Add(position.OpenOrderId);
                trValues.Add(position.CloseOrderId);
                trValues.Add(position.TimeOpen);
                trValues.Add(position.TimeClose);
                trValues.Add(position.Operation);
                trValues.Add(position.Size);
                trValues.Add(position.Price);
                trValues.Add(position.Last);
                trValues.Add(Math.Round(position.Trades, 2));
                trValues.Add(position.Margin);
                trValues.Add(position.Comment);
                trValues.Add(position.AddInfo);
                trValues.Add(MinDt.ToString("dd/MM/yyyy"));
                trValues.Add(MaxDt.ToString("dd/MM/yyyy"));
                if (provider != null) provider.Rows.Add(trValues.ToArray());
                trValues.Clear();
            }


            tradegrid.DataSource = provider;
        }
    }
}

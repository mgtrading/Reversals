using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;
using System.Collections;
using Reversals.DateFormats;
using Reversals.Display.BaseDisplayer;
using Reversals.Strategies;

namespace Reversals.Display.SummaryDisplayer
{
    public class SummaryDisplayer : Displayer
    {

        public SummaryDisplayer(Strategy vtrades, DateTime startDate, DateTime endDate) :

            base(vtrades, startDate, endDate) { }

        public void DisplayLabelItems(DevComponents.DotNetBar.Controls.GroupPanel panel)
        {
          
            var itemlist = new ArrayList();
          
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                if (panel.Controls[i] is Label)
                {
                    itemlist.Add(panel.Controls[i]);
                }
            }

            var values = new ArrayList
                             {
                                  FormatNumber(Math.Round(Strategyperformance.EntireTradesSum, 2)),
                                  FormatNumber(Math.Round(Strategyperformance.ProfitTradesSum, 2)),
                                 FormatNumber(Math.Round(Strategyperformance.LossTradesSum, 2)),
                                  FormatNumber(Math.Round(Strategyperformance.Profitability, 2)),
                                 FormatNumber(Math.Round(Strategyperformance.ProfitFactor, 2)),
                                  FormatNumber(Math.Round(Strategyperformance.AverageTrade, 4)),
                                 Strategyperformance.EntireTradesCount,
                                 Strategyperformance.ProfitTradesCount,
                                 Strategyperformance.LossTradesCount,
                                 MinDt.ToString("dd/MM/yyyy"),
                                 MaxDt.ToString("dd/MM/yyyy")
                             };

            for (int i = values.Count - 1; i >= 0; i--)
            {
                MethodInvoker action = delegate { ((Label)itemlist[(values.Count - 1) - i]).Text = values[i].ToString(); };
                ((Label)itemlist[(values.Count - 1) - i]).Invoke(action);
            }


        }

        public void DisplayTable(DataGridView dgrid)
        {
            var provider = dgrid.DataSource as DataTable;
            if (provider != null && provider.Columns["StopLVL"] == null)
            {
                 provider.Columns.Add(new DataColumn("StopLVL"));
                 provider.Columns.Add(new DataColumn("RevLVL"));
                 provider.Columns.Add(new DataColumn("ZIM"));
                provider.Columns[5].ReadOnly = true;
                provider.Columns[6].ReadOnly = true;
                provider.Columns[7].ReadOnly = true;
            }

            string entrieTrSum = FormatNumber(Math.Round(Strategyperformance.EntireTradesSum, 2));

            var values = new ArrayList
                             {
                                 Symbol,
                                 MinDt.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss"),
                                 MaxDt.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss"),
                                 entrieTrSum, 
                                 Strategyperformance.Reversals,
                                 Strategy.AdditionalParameters[7].Value,
                                 Strategy.AdditionalParameters[8].Value,
                                 Strategy.AdditionalParameters[5].Value
                             };

            if (provider != null) provider.Rows.Add(values.ToArray());
            values.Clear();
            try
            {
                MethodInvoker action = delegate
                {
                    dgrid.DataSource = null;
                    dgrid.DataSource = provider;
                    dgrid.Columns[1].FillWeight = dgrid.Columns[2].FillWeight = 180;
                    dgrid.Refresh();
                };
                dgrid.Invoke(action);
            }
            catch (Exception)
            {
            }

        }

        public void DisplayChart(Chart dchart)
        {
            if (dchart.Series.Count == 0)
            {
                var series = new Series("Chart")
                                 {
                                     ChartType = SeriesChartType.Line,
                                     Color = System.Drawing.Color.MediumPurple,
                                     BorderWidth = 2
                                 };
                MethodInvoker action = delegate { dchart.Series.Add(series); };
                dchart.Invoke(action);


            }
            else
            {

                MethodInvoker action = delegate
                                           {
                                               dchart.Series[0].Points.AddXY(MaxDt.ToShortDateString(), Strategyperformance.EntireTradesSum);
                                               dchart.Legends[0].Enabled = false;
                                           };
                dchart.Invoke(action);
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using DevComponents.DotNetBar.Schedule;
using DevComponents.DotNetBar.SuperGrid;
using DevComponents.Editors;
using DevComponents.DotNetBar.Metro;
using DevComponents.DotNetBar;
using System.Text.RegularExpressions;
using Reversals.CollectingPriceData;
using Reversals.DataContainer;
using Reversals.DateFormats;
using Reversals.DbDataManager;
using Reversals.DbDataManager.Structs;
using Reversals.Display;
using Reversals.Display.CalendarDisplayer;
using Reversals.Display.SummaryDisplayer;
using Reversals.Optimization;
using Reversals.ParametersNotifier;
using Reversals.Properties;
using Reversals.Strategies;
using Reversals.TestWorking;
using ToolTip = System.Windows.Forms.ToolTip;

namespace Reversals.Forms
{
    public partial class MainFormMetroApp : MetroAppForm
    {
        private List<ContractModel> _contracts = new List<ContractModel>();
        private string _filepath = "";
        private Data _data;
        //private IndicatorPortfolio _portfolio;
        private Strategy _strategy;
        private DateTime _inSampleStartTime;
        private DateTime _inSampleEndTime;
        private Thread _thread;
        //private double[] _monthProfit;
        private GridPanel _weeksPanel;
        private GridPanel _daysPanel;
        private GridPanel _tradesPanel;
        private WeeklyDataDisplayer _weekData;
        private readonly CultureInfo _nfi = new CultureInfo("en-US", false);
        private double _tickSize;
        private List<DataSetModel> _dataSets;
        private ParametersChangingNotifier _optParameterssChanging;
        private bool _addingNewDataSet;
        private readonly Image _expandimage;
        private readonly Image _collapseimage;
        private string _currentSymbolName;

        //calendar variables
        private readonly List<ResultModel> _calendarResult;
        private List<ContractModel> _symbolList;
        private List<DataSetModel> _datasetList;
        private readonly List<ResultModel> _weeklist;
        private CalendarDisplayer _calendarDisplayer;
        private readonly BlackScholesCalculator _blackScholesCalculator;

        public MainFormMetroApp()
        {
            InitializeComponent();

            InitializeWeeklyDataTable();
            _expandimage = Resources.treeexpand;
            _collapseimage = Resources.treecollapse;
            _blackScholesCalculator = new BlackScholesCalculator();

            uiStrategy_comboBoxExOISStepKind.SelectedIndex = 0;
            uiStrategy_comboBoxExOOOSStepKind.SelectedIndex = 0;
            uiStrategy_comboBoxExOOOSStepKind.KeyPress += KeyPressValidator;
            uiStrategy_comboBoxExOISStepKind.KeyPress += KeyPressValidator;

            uiCalendar_calendarViewResultView.SelectedView = eCalendarView.Month;
            uiBlackSValueDate.Value = DateTime.Now;

            uiBlackSInterestRate.KeyPress += KeyPressValidator;
            uiBlackSUnderlyingPrice.KeyPress += KeyPressValidator;
            uiBlackSVolality.KeyPress += KeyPressValidator;
            uiBlackSQuantity.KeyPress += QuantityKeyPressValidator;
            uiBlackSPriceStrike.KeyPress += KeyPressValidator;
            uiBlackSCallGamma.KeyPress += KeyPressValidator;
            uiBlackSCallTheta.KeyPress += KeyPressValidator;
            uiBlackSCallVega.KeyPress += KeyPressValidator;
            uiBlackSPutGamma.KeyPress += KeyPressValidator;
            uiBlackSPutTheta.KeyPress += KeyPressValidator;
            uiBlackSPutVega.KeyPress += KeyPressValidator;

            uiBlackSInterestRate.KeyDown += SubmitTextBox;
            uiBlackSUnderlyingPrice.KeyDown += SubmitTextBox;
            uiBlackSVolality.KeyDown += SubmitTextBox;
            uiBlackSQuantity.KeyDown += SubmitTextBox;
            uiBlackSPriceStrike.KeyDown += SubmitTextBox;

            uiStrategy_dataGridViewNoOptomizationParameters.KeyPress += DGridKeyPressValidator;
            uiStrategy_dataGridViewOptomizationParameters.KeyPress += DGridKeyPressValidator;

            uiBlackSQuantity.Text = @"45";

            _calendarResult = new List<ResultModel>();
            _weeklist = new List<ResultModel>();

            labelX1.ForeColor = Color.SteelBlue;
            labelX2.ForeColor = Color.SteelBlue;
            labelX3.ForeColor = Color.SteelBlue;
            labelX4.ForeColor = Color.SteelBlue;
            labelX5.ForeColor = Color.SteelBlue;
            labelX7.ForeColor = Color.SteelBlue;
            labelX8.ForeColor = Color.SteelBlue;
            labelX9.ForeColor = Color.SteelBlue;
            uiDataArchive_labelXSymbolsCommands.ForeColor = Color.SteelBlue;
            uiDataArchive_labelXCollectingControl.ForeColor = Color.SteelBlue;
            uiDataArchive_labelXSearch.ForeColor = Color.SteelBlue;

            uiDataArchive_dateTimeInputFindTime.CustomFormat = @"HH:mm:ss";
            uiDataArchive_dateTimeInputFindTime.Format = eDateTimePickerFormat.ShortTime;

            uiDataArchive_dateTimeInputStart.Value = DateTime.Today.AddDays(-1);
            uiDataArchive_dateTimeInputEnd.Value = DateTime.Today;

            uiStrategy_dateTimeAdvOISStart.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiStrategy_dateTimeAdvOISStart.Format = eDateTimePickerFormat.Custom;

            uiStrategy_dateTimeAdvOISEnd.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiStrategy_dateTimeAdvOISEnd.Format = eDateTimePickerFormat.Custom;

            uiStrategy_dateTimeAdvOOOSStart.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiStrategy_dateTimeAdvOOOSStart.Format = eDateTimePickerFormat.Custom;

            uiStrategy_dateTimeAdvOOOSEnd.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiStrategy_dateTimeAdvOOOSEnd.Format = eDateTimePickerFormat.Custom;

            uiBlackSValueDate.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiBlackSValueDate.Format = eDateTimePickerFormat.Custom;

            uiBlackSExpiryDate.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiBlackSExpiryDate.Format = eDateTimePickerFormat.Custom;

            uiDataArchive_dateTimeInputStart.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiDataArchive_dateTimeInputStart.Format = eDateTimePickerFormat.Custom;

            uiDataArchive_dateTimeInputEnd.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiDataArchive_dateTimeInputEnd.Format = eDateTimePickerFormat.Custom;

            uiDataArchive_dateTimeInputFindDate.CustomFormat = DateFormatsManager.CurrentShortDateFormat;
            uiDataArchive_dateTimeInputFindDate.Format = eDateTimePickerFormat.Custom;

            ToastNotification.ToastBackColor = Color.SteelBlue;
            ToastNotification.ToastForeColor = Color.White;
            ToastNotification.ToastFont = new Font("Segoe UI", 10F);
            ToastNotification.CustomGlowColor = Color.Blue;
            ToastNotification.DefaultToastPosition = eToastPosition.TopCenter;
            ToastNotification.DefaultTimeoutInterval = 2000;
        }

        private void metroShell1_SettingsButtonClick(object sender, EventArgs e)
        {
            var fs = new FormSettings();
            fs.ShowDialog();
        }

        private void UiBTSummaryScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (!(uiSummary_dataGridViewBT.RowCount < 2))
            {


                uiBTSummaryScrollBar.Maximum = uiSummary_dataGridViewBT.RowCount;

                if ((e.OldValue - e.NewValue) > 0)
                {
                    uiSummary_dataGridViewBT.FirstDisplayedScrollingRowIndex = e.NewValue;

                }
                else if ((e.OldValue - e.NewValue) < 0)
                {
                    uiSummary_dataGridViewBT.FirstDisplayedScrollingRowIndex = e.NewValue;
                }
            }
        }

        private void InitializeWeeklyDataTable()
        {
            _weeksPanel = uiWeeklyData_superGridControlTable.PrimaryGrid;
            _weeksPanel.Name = "Week";
            _weeksPanel.ShowCheckBox = false;
            _weeksPanel.ShowTreeButtons = true;
            _weeksPanel.ShowTreeLines = true;
            _weeksPanel.AutoGenerateColumns = true;
            _weeksPanel.DataMember = "tableWeek";
            _weeksPanel.IsParentOf(_daysPanel);

            _daysPanel = new GridPanel
            {
                DataMember = "tableDays",
                ShowTreeButtons = true,
                ShowTreeLines = true,
                AutoGenerateColumns = true
            };
            _daysPanel.IsParentOf(_tradesPanel);
            _daysPanel.RowHeaderWidth = 0;


            _tradesPanel = new GridPanel
            {
                DataMember = "tableTrades",
                ShowCheckBox = false,
                ShowTreeButtons = true,
                ShowTreeLines = true,
                AutoGenerateColumns = true
            };
            uiWeeklyData_superGridControlTable.GetCellStyle += WeeklyDataTableCellStyle;
        }

        private void uiFilePicker_Click(object sender, EventArgs e)
        {
            uiBlackSDivYield.Enabled = true;
            uiBlackSInterestRate.Enabled = true;
            uiBlackSPriceStrike.Enabled = true;
            uiBlackSVolality.Enabled = true;
            uiBlackSUnderlyingPrice.Enabled = true;

            try
            {
                if (uiStrategy_radioButtonDataFromFile.Checked)
                {

                    var fileDialog = new OpenFileDialog
                    {
                        Multiselect = true,
                        InitialDirectory = Directory.GetCurrentDirectory(),
                        Filter = @"TXT (*.txt)|*.txt",
                        FilterIndex = 2,
                        RestoreDirectory = true
                    };

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _filepath = fileDialog.FileName;
                        _data = new Data(_filepath, Data.DataFileType.TextFromTs);
                        _data.CreateData();
                    }
                    else
                        _filepath = string.Empty;
                }
                //progressBarX1.Visible = true;
                //timer1.Enabled = true;
                //timer1.Start();
                if (_filepath == string.Empty)
                    return;


                Report(string.Format("{0} bars have been loaded from file .", _data.Count),
                       InformerMessageType.Success);

                uiStrategy_textBoxXFilepath.Text = _filepath;

                uiStrategy_dateTimeAdvOISStart.MaxDate = uiStrategy_dateTimeAdvOOOSStart.MaxDate = _data[_data.Count - 1].Time;
                uiStrategy_dateTimeAdvOISStart.MinDate = uiStrategy_dateTimeAdvOOOSStart.MinDate = _data[0].Time;

                uiStrategy_dateTimeAdvOISEnd.MaxDate = uiStrategy_dateTimeAdvOOOSEnd.MaxDate = _data[_data.Count - 1].Time;
                uiStrategy_dateTimeAdvOISEnd.MinDate = uiStrategy_dateTimeAdvOOOSEnd.MinDate = _data[0].Time;

                uiStrategy_dateTimeAdvOISStart.Value = _data[0].Time;
                uiStrategy_dateTimeAdvOISEnd.Value = uiStrategy_dateTimeAdvOOOSStart.Value = uiStrategy_dateTimeAdvOOOSEnd.Value = _data[_data.Count - 1].Time;

                uiStrategy_panelExOptions.Enabled = true;
                uiOptimizationInSample.Enabled = true;
                uiStrategy_panelExOptimizationOutOfSample.Enabled = true;


                uiStrategy_buttonXStop.Enabled = false;

                uiStrategy_panelExBlackScholes.Enabled = true;

                uiStrategy_comboBoxExDataSet.Items.Clear();
                var tempDataset = new TemporaryDatasets();

                _dataSets = DataManager.GetDatasets();

                uiStrategy_dataGridViewNoOptomizationParameters.DataSource = tempDataset.AliDataSet151519;
                uiStrategy_dataGridViewOptomizationParameters.DataSource = tempDataset.OptimParams;
                uiStrategy_comboBoxExDataSet.Items.Add("Default DataSet for file");
                uiStrategy_comboBoxExDataSet.SelectedIndex = uiStrategy_comboBoxExDataSet.Items.Count - 1;

                foreach (DataSetModel dataSet in _dataSets)
                {
                    uiStrategy_comboBoxExDataSet.Items.Add(dataSet.DataSetName);
                }
            }
            catch (Exception)
            {
                Report("Error when reading the file! Please, open another file.", InformerMessageType.Error);
            }

        }


        private void uiStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            const int account = 1;
            var name = uiStrategy.Items[uiStrategy.SelectedIndex].ToString();
            const string symbol = "";

            var parameters = new Strategy.StrategyParameters(account, symbol, name);

            Initialize(uiStrategy.SelectedIndex, parameters);
            if (_strategy != null)
            {
                //TODO there are we send only one parameter
                //NotOptimizationParametersContainer.Instance.Parameters = DefaultNOptParameters(_strategy.Parameters, _strategy .AdditionalParameters);
                NotOptimizationParametersContainer.Instance.Parameters = DefaultNOptParameters( _strategy.AdditionalParameters);
            }

            uiStrategy_dataGridViewNoOptomizationParameters.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            uiStrategy_dataGridViewNoOptomizationParameters.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

            uiStrategy_panelExRunContinuesPanel.Enabled = true;
        }


        private void uiStart_Click(object sender, EventArgs e)
        {
            uiStrategy_SelectedIndexChanged(this, e);
            DefaultBtSummaryTable();
            DefaultFtSummaryTable();

            uiCalendar_comboBoxXSymbol.SelectedItem = uiStrategy_comboBoxExSymbol.SelectedItem;
            uiCalendar_comboBoxXDSet.SelectedItem = uiStrategy_comboBoxExDataSet.SelectedItem;

            uiBTSummaryChart.Series.Clear();

            File.Delete(@"LogStrategy.txt");

            uiStrategy_buttonXStart.Enabled = false;
            uiStrategy_buttonXStop.Enabled = true;
            InitializeDates();
            uiCalendar_calendarViewResultView.CalendarModel.Appointments.Clear();
            uiStatus_toolStripProgressBar.Value = 0;

            _currentSymbolName = uiStrategy_comboBoxExSymbol.Text;
            if (uiStrategy_switvhButtonStartOptimizeCheck.Value)
            {
                Invoke((Action)(() => Report("Optimization is working.", InformerMessageType.Normal)));
                _thread = new Thread(StartOptimize) { Name = "Optimize", Priority = ThreadPriority.AboveNormal };
                _thread.Start();
            }
            else
            {
                Invoke((Action)(() => Report("In Sample/Out Of Sample procedure is working.", InformerMessageType.Normal)));
                _thread = new Thread(StartBackTest) { Name = "BackTest", Priority = ThreadPriority.AboveNormal };
                _thread.Start();
            }
        }

        private void StartBackTest()
        {
            Invoke((Action)delegate
            {
                TestManager.ProgressEvent += ProgressIncrement;
            });
            var parameters = new Strategy.StrategyParameters(1, _currentSymbolName, "Step Change");

            List<Strategy.StrategyAdditionalParameter> additional = UpdateAdditionalParams();
            TestManager.ResultEvent += DisplayBackTestResult;
            TestManager.StartBackTest(parameters, additional, _data.BarsRange(_inSampleStartTime, _inSampleEndTime, 0).Bars);
            TestManager.ResultEvent -= DisplayBackTestResult;

            Invoke((Action)delegate
            {
                Report("In Sample/Out Of Sample procedure finished.", InformerMessageType.Success);
                uiStrategy_buttonXStart.Enabled = true;
                uiStrategy_buttonXStop.Enabled = false;
            });
        }

        private void ProgressIncrement()
        {
            Invoke((Action)delegate
            {
                if (uiStatus_toolStripProgressBar.Value <= uiStatus_toolStripProgressBar.Maximum)
                {
                    uiStatus_toolStripProgressBar.Value += 1;
                }

            });
        }


        private void StartOptimize()
        {
            uiWeeklyData_superGridControlTable.PrimaryGrid.DataSource = null;
            var optimizer = new Optimizer(_strategy, OptimizationParameters.Instance.Parameters, uiStrategy_dateTimeAdvOISStart.Value, uiStrategy_dateTimeAdvOISEnd.Value);
            optimizer.ProgressEvent += OptimizeProgressIncrement;
            optimizer.StartOptimize(uiSummary_dataGridViewBT);

            Invoke((Action)delegate
            {
                Report("In Sample/Out Of Sample procedure completed.", InformerMessageType.Success);
                uiStrategy_buttonXStart.Enabled = true;
                uiStrategy_buttonXStop.Enabled = false;
            });

        }

        private void OptimizeProgressIncrement(double percent)
        {
            Invoke((Action)delegate
            {
                if (uiStatus_toolStripProgressBar.Value <= uiStatus_toolStripProgressBar.Maximum)
                    uiStatus_toolStripProgressBar.Value += (int)percent;
            });
        }

        private void InitializeDates()
        {
            _inSampleStartTime = uiStrategy_dateTimeAdvOISStart.Value;
            _inSampleEndTime = uiStrategy_dateTimeAdvOISEnd.Value;
        }

        private void DisplayBackTestResult(Strategy strategy)
        {

            var summaryDisplayer = new SummaryDisplayer(strategy, _inSampleStartTime, _inSampleEndTime);

            Invoke((Action)(() => summaryDisplayer.DisplayTable(uiSummary_dataGridViewBT)));

            if (uiStrategy_checkBoxXRunContinuesCheck.Checked == false)
            {
                if (strategy.Trades != null)
                {
                    _weekData = new WeeklyDataDisplayer(strategy, true, _inSampleStartTime, _inSampleEndTime);
                    uiWeeklyData_superGridControlTable.PrimaryGrid.DataSource = _weekData._weeklyData;

                    _calendarDisplayer = new CalendarDisplayer(_weekData);
                    Invoke((Action)InitCalendar);
                    Invoke((Action)(() => uiCalendar_buttonXSave.Enabled = true));
                }
            }
        }

        private DataTable Provider(params object[] parameters)
        {
            var provider = new DataTable();
            foreach (object parameter in parameters)
            {
                if (parameter.ToString() == "FPL") provider.Columns.Add(parameter.ToString(), typeof(double));
                else provider.Columns.Add(parameter.ToString());
            }
            return provider;
        }

        #region region.strategy

        private void Initialize(int index, Strategy.StrategyParameters parameters)
        {
            switch (index)
            {
                case 0:
                    {
                        var additional8 = new Strategy.StrategyAdditionalParameter("", "Stop Level", 80 * _tickSize, typeof(double), false);
                        var additional9 = new Strategy.StrategyAdditionalParameter("", "Reversal Level", 80 * _tickSize, typeof(double), false);

                        List<Strategy.StrategyAdditionalParameter> additional = UpdateAdditionalParams();

                        additional.Add(additional8);
                        additional.Add(additional9);

                        _strategy = new StepChange(parameters, additional, ref _data);

                        break;
                    }
                default: // Other Unregistered Strategies
                    {
                        Report("The selected strategy is not registered. Please, select a registered strategy.", InformerMessageType.Error);
                        break;
                    }
            }
        }

        private List<Strategy.StrategyAdditionalParameter> UpdateAdditionalParams()
        {  //user input controlling in the function -> uiStrategyNoOptomizationParameters_CellValidating()
            var additional = new List<Strategy.StrategyAdditionalParameter>();
            var provider = uiStrategy_dataGridViewNoOptomizationParameters.DataSource as DataTable;

            if (provider == null) return null;

            for (var i = 0; i < provider.Rows.Count; i++)
            {
                string paramname = provider.Rows[i]["Variables"].ToString();
                object paramvalue = provider.Rows[i]["Default Value"];
                if (paramname != "DataSet Name")
                {
                    if (uiStrategy_checkBoxXReqTVCheck.Checked && paramname == "ZIM")
                    {
                        paramvalue = double.Parse(uiBlackSZim.Text, _nfi);

                    }
                    var additionaltemp = new Strategy.StrategyAdditionalParameter("", paramname, paramvalue, paramvalue.GetType(), false);
                    additional.Add(additionaltemp);
                }
            }

            var provider2 = uiStrategy_dataGridViewOptomizationParameters.DataSource as DataTable;
            if (provider2 != null && provider2.Columns.Count > 0)
            {

                for (int i = 0; i < provider2.Rows.Count; i++)
                {
                    string paramname = provider2.Rows[i]["Triggers"].ToString();
                    object paramvalue = provider2.Rows[i]["Default Value"];

                    var additionaltemp = new Strategy.StrategyAdditionalParameter("", paramname, paramvalue,
                                                                                  paramvalue.GetType(), false);
                    additional.Add(additionaltemp);
                }
            }

            return additional;
        }
        #endregion

        #region region.ui.informer
        enum InformerMessageType
        {
            Success,
            Error,
            Attention,
            Normal
        }

        private void Report(string message, InformerMessageType type)
        {
            Invoke((Action)delegate
            {
                uiStatus_labelItemProgramStatus.Text = message;

                switch (type)
                {
                    case InformerMessageType.Success:
                        uiStatus_metroStatusBarStripInformer.BackColor = Color.Green;
                        uiStatus_metroStatusBarStripInformer.ForeColor = Color.White;
                        break;
                    case InformerMessageType.Error:
                        uiStatus_metroStatusBarStripInformer.BackColor = Color.Red;
                        uiStatus_metroStatusBarStripInformer.ForeColor = Color.White;
                        break;
                    case InformerMessageType.Attention:
                        uiStatus_metroStatusBarStripInformer.BackColor = Color.RoyalBlue;
                        uiStatus_metroStatusBarStripInformer.ForeColor = Color.White;
                        break;
                    case InformerMessageType.Normal:
                        uiStatus_metroStatusBarStripInformer.BackColor = Color.Transparent;
                        uiStatus_metroStatusBarStripInformer.ForeColor = Color.White;
                        break;
                }
            });

        }
        #endregion

        #region Strategy.Helper

        // todo check this function. i remove first parameter, bcz not use
        private List<NotOptimizationParameter> DefaultNOptParameters(IEnumerable<Strategy.StrategyAdditionalParameter> additionals)
        {

            var _default = new List<NotOptimizationParameter>();

            foreach (Strategy.StrategyAdditionalParameter additional in additionals)
            {
                if (!additional.Optimizable)
                {
                    if (additional.Name == "TV")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "Commission")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "Multiplier")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "Contract Size")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "Point Value")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "ZIM")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "Tick Size")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "Step1")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                    else if (additional.Name == "Step2")
                    {
                        _default.Add(new NotOptimizationParameter("", additional.Name, additional.Type, 0, 0, additional.Value, 0));
                    }
                }
            }

            return _default;
        }

        #endregion

        private void DefaultBtSummaryTable()
        {
            uiSummary_dataGridViewBT.Columns.Clear();
            uiSummary_dataGridViewBT.DefaultCellStyle.Font = new Font("Segoe UI", 8);
            uiSummary_dataGridViewBT.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8);
            uiSummary_dataGridViewBT.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8);

            uiSummary_dataGridViewBT.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            DataTable provider = Provider("BSm", "BFr", "BTo", "PNL", "Reversals");
            provider.Columns[0].ReadOnly = true;
            provider.Columns[1].ReadOnly = true;
            provider.Columns[2].ReadOnly = true;
            provider.Columns[3].ReadOnly = true;
            provider.Columns[4].ReadOnly = true;

            uiSummary_dataGridViewBT.DataSource = provider;
        }

        private void DefaultFtSummaryTable()
        {
            uiFTSummaryTable.Columns.Clear();
            uiFTSummaryTable.DefaultCellStyle.Font = new Font("Segoe UI", 8);
            uiFTSummaryTable.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8);
            uiFTSummaryTable.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8);

            uiFTSummaryTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            DataTable provider1 = Provider("FSm", "FFr", "FTo", "FNet", "FNetPr", "FNetLs", "FPrf%", "FPrF", "FAvgTr", "FNTr", "FNPr", "FNLs");
            uiFTSummaryTable.DataSource = provider1;
        }

        private void uiStop_Click(object sender, EventArgs e)
        {
            if (_thread != null)
                _thread.Abort();
            uiStrategy_buttonXStart.Enabled = true;
            uiStrategy_buttonXStop.Enabled = false;
            Report("Procedure is stopped.", InformerMessageType.Normal);
        }

        private void uiWeeklyDataTable_DataBindingComplete(object sender, GridDataBindingCompleteEventArgs e)
        {
            uiWeeklyData_superGridControlTable.PrimaryGrid.Columns["id"].CellStyles.Default.TextColor = Color.White;
            uiWeeklyData_superGridControlTable.PrimaryGrid.Columns["id"].FillWeight = 20;
            uiWeeklyData_superGridControlTable.PrimaryGrid.Columns["id"].HeaderText = "";
            e.GridPanel.ExpandImage = _expandimage;
            e.GridPanel.CollapseImage = _collapseimage;
            e.GridPanel.DefaultVisualStyles.ColumnHeaderStyles.Default.Background.Color1 = Color.DodgerBlue;
            e.GridPanel.DefaultVisualStyles.ColumnHeaderStyles.Default.Background.Color2 = Color.DodgerBlue;
        }

        private void uiWeeklyDataTable_AfterExpand(object sender, GridAfterExpandEventArgs e)
        {
            e.GridContainer.Rows[0].GridPanel.RowHeaderWidth = 0;
            e.GridContainer.Rows[0].GridPanel.Columns["id"].Width = 19;
            e.GridContainer.Rows[0].GridPanel.Columns["id"].HeaderText = "";
            if (e.GridContainer.Rows[0].GridPanel.Columns["id_Week"] != null)
            {
                e.GridContainer.Rows[0].GridPanel.Columns["id_Week"].Visible = false;
                e.GridContainer.Rows[0].GridPanel.Columns["Date"].Width = 120;
                e.GridContainer.Rows[0].GridPanel.AllowEdit = false;
            }
            if (e.GridContainer.Rows[0].GridPanel.Columns["id_Day"] != null)
            {
                e.GridContainer.Rows[0].GridPanel.Columns["id"].Visible = false;
                e.GridContainer.Rows[0].GridPanel.Columns["id_Day"].Visible = false;
                e.GridContainer.Rows[0].GridPanel.Columns["TimeOpen"].Width = 120;
                e.GridContainer.Rows[0].GridPanel.Columns["TimeClose"].Width = 120;
                e.GridContainer.Rows[0].GridPanel.AllowEdit = false;
            }

        }

        private void uiDataSet_SelectedIndexChanged(object sender, EventArgs e)
        {

            uiStrategy_panelExBlackScholesPanel.Enabled = true;

            uiStrategy_buttonXStart.Enabled = true;
            uiStrategy_buttonXStop.Enabled = true;
            uiStrategy_WorkingButtonsPanel.Enabled = true;

            uiBlackSPriceStrike.Text = @"1875.00";
            uiBlackSInterestRate.Text = @"5";
            uiBlackSUnderlyingPrice.Text = @"1870.00";
            uiBlackSVolality.Text = @"19";
            uiBlackSValueDate.Value = DateTime.Today;
            uiBlackSExpiryDate.Value = DateTime.Today.AddDays(30);

            if (uiStrategy_radioButtonDataFromFile.Checked)
            {
                if (uiStrategy_comboBoxExDataSet.Text != @"Default DataSet for file")
                {
                    var selectedDataSet = DataManager.GetDatasetData(_dataSets.First(a => a.DataSetName == uiStrategy_comboBoxExDataSet.Text).Id);
                    DisplayDatasetParams(selectedDataSet);
                    UpdateOptParamsInstance(selectedDataSet);
                    _addingNewDataSet = false;
                }
                uiStrategy.SelectedIndex = 0;
            }

            if (uiStrategy_radioButtonDataFromDB.Checked)
            {
                var selectedDataSet = DataManager.GetDatasetData(_dataSets.First(a => a.DataSetName == uiStrategy_comboBoxExDataSet.Text).Id);
                DisplayDatasetParams(selectedDataSet);
                UpdateOptParamsInstance(selectedDataSet);
                if (_data != null)
                {
                    uiStrategy.SelectedIndex = 0;
                }
                else
                {
                    uiStrategy_panelExBlackScholesPanel.Enabled = false;

                    uiStrategy_buttonXStart.Enabled = false;
                    uiStrategy_buttonXStop.Enabled = false;
                    uiStrategy_WorkingButtonsPanel.Enabled = false;
                }
                _addingNewDataSet = false;
            }

            if (uiStrategy_comboBoxExDataSet.Text == @"Default DataSet for file")
            {
                var tempDataset = new TemporaryDatasets();

                uiStrategy_dataGridViewNoOptomizationParameters.DataSource = tempDataset.AliDataSet151519;
                uiStrategy_dataGridViewOptomizationParameters.DataSource = tempDataset.OptimParams;

                OptimizationParameters.Instance.Parameters.Clear();
                OptimizationParameters.Instance.Parameters.Add(new OptimizationParameter("", "Stop Level", typeof(double),
                                                     tempDataset.OptimParams.Rows[0][2], tempDataset.OptimParams.Rows[0][3],
                                                     tempDataset.OptimParams.Rows[0][1],
                                                     tempDataset.OptimParams.Rows[0][4]));
                OptimizationParameters.Instance.Parameters.Add(new OptimizationParameter("", "Reversal Level", typeof(double),
                                                     tempDataset.OptimParams.Rows[1][2], tempDataset.OptimParams.Rows[1][3],
                                                     tempDataset.OptimParams.Rows[1][1],
                                                     tempDataset.OptimParams.Rows[1][4]));
                uiStrategy.SelectedIndex = 0;
            }


        }

        private void DisplayDatasetParams(DataSetModel dataSet)
        {
            var parameterProvider = new DataTable();
            var optimalizableparamsProvider = new DataTable();
            var column = new DataColumn("Variables");
            var column2 = new DataColumn("Default Value");

            parameterProvider.Columns.Add(column);
            parameterProvider.Columns.Add(column2);

            column = new DataColumn("Triggers");
            optimalizableparamsProvider.Columns.Add(column);
            column = new DataColumn("Default Value");
            optimalizableparamsProvider.Columns.Add(column);
            column = new DataColumn("MinValue");
            optimalizableparamsProvider.Columns.Add(column);
            column = new DataColumn("MaxValue");
            optimalizableparamsProvider.Columns.Add(column);
            column = new DataColumn("Step");
            optimalizableparamsProvider.Columns.Add(column);


            DataRow row = parameterProvider.NewRow();
            row[0] = "DataSet Name";
            row[1] = dataSet.DataSetName;
            parameterProvider.Rows.Add(row);

            row = parameterProvider.NewRow();
            row[0] = "Time Value";
            row[1] = dataSet.TimeValue;
            parameterProvider.Rows.Add(row);

            row = parameterProvider.NewRow();
            row[0] = "Commission";
            row[1] = dataSet.Commission;
            parameterProvider.Rows.Add(row);

            row = parameterProvider.NewRow();
            row[0] = "Multiplier";
            row[1] = dataSet.Multiplier;
            parameterProvider.Rows.Add(row);

            row = parameterProvider.NewRow();
            row[0] = "Contract Size";
            row[1] = dataSet.ContractSize;
            parameterProvider.Rows.Add(row);

            row = parameterProvider.NewRow();
            row[0] = "Point Value";
            row[1] = dataSet.PointValue;
            parameterProvider.Rows.Add(row);

            row = parameterProvider.NewRow();
            row[0] = "ZIM";
            row[1] = dataSet.Zim;
            parameterProvider.Rows.Add(row);

            row = parameterProvider.NewRow();
            row[0] = "Tick Size";
            row[1] = dataSet.TickSize;
            _tickSize = dataSet.TickSize;
            parameterProvider.Rows.Add(row);

            row = optimalizableparamsProvider.NewRow();
            row[0] = "Stop Level";
            row[1] = dataSet.StopLevelDef;
            row[2] = dataSet.StopLevelMin;
            row[3] = dataSet.StopLevelMax;
            row[4] = dataSet.StopLevelStep;
            optimalizableparamsProvider.Rows.Add(row);

            row = optimalizableparamsProvider.NewRow();
            row[0] = "Reversal Level";
            row[1] = dataSet.ReversalLevelDef;
            row[2] = dataSet.ReversalLevelMin;
            row[3] = dataSet.ReversalLevelMax;
            row[4] = dataSet.ReversalLevelStep;
            optimalizableparamsProvider.Rows.Add(row);

            parameterProvider.Columns["Variables"].ReadOnly = true;
            optimalizableparamsProvider.Columns["Triggers"].ReadOnly = true;
            uiStrategy_dataGridViewOptomizationParameters.DataSource = optimalizableparamsProvider;
            uiStrategy_dataGridViewNoOptomizationParameters.DataSource = parameterProvider;

            uiStrategy_buttonXStart.Enabled = true;
            uiStrategy_buttonXStop.Enabled = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_thread != null)
                _thread.Abort();
        }

        private void UpdateOptionsCalc()
        {
            if (uiStrategy_panelExBlackScholesPanel.Enabled)
            {
                var nfi = new CultureInfo("en-US", false).NumberFormat;
                double time1 = uiBlackSValueDate.Value.DayOfYear;
                double time2 = uiBlackSExpiryDate.Value.DayOfYear;
                var time = (time2 - time1)/365;
                textBoxX1.Text = (time*365).ToString(CultureInfo.InvariantCulture);
                var underlineprice = double.Parse(uiBlackSUnderlyingPrice.Text, nfi);
                var strikeprice = double.Parse(uiBlackSPriceStrike.Text, nfi);
                var volatily = double.Parse(uiBlackSVolality.Text)/100;
                var interestrate = double.Parse(uiBlackSInterestRate.Text)/100;
                const double divider = 0.05;

                uiBlackSCallDelta.Text = _blackScholesCalculator.CallDelta(underlineprice, strikeprice, time,interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);
                uiBlackSPutDelta.Text = _blackScholesCalculator.PutDelta(underlineprice, strikeprice, time, interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);
                uiBlackSCallGamma.Text = _blackScholesCalculator.Gamma(underlineprice, strikeprice, time, interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);
                uiBlackSPutGamma.Text = uiBlackSCallGamma.Text;

                uiBlackSCallVega.Text = _blackScholesCalculator.Vega(underlineprice, strikeprice, time, interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);
                uiBlackSPutVega.Text = uiBlackSCallVega.Text;

                uiBlackSCallRho.Text = _blackScholesCalculator.CallRho(underlineprice, strikeprice, time, interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);

                uiBlackSPutRho.Text = _blackScholesCalculator.PutRho(underlineprice, strikeprice, time, interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);

                uiBlackSCallTheta.Text = _blackScholesCalculator.CallTheta(underlineprice, strikeprice, time,interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);

                uiBlackSPutTheta.Text = _blackScholesCalculator.PutTheta(underlineprice, strikeprice, time, interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);
                uiBlackSCallPremium.Text = _blackScholesCalculator.CallOption(underlineprice, strikeprice, time,interestrate,volatily, divider).ToString(CultureInfo.InvariantCulture);
                uiBlackSPutPremium.Text = _blackScholesCalculator.PutOption(underlineprice, strikeprice, time,interestrate, volatily, divider).ToString(CultureInfo.InvariantCulture);

                if (uiStrategy_checkBoxXReqTVCheck.Checked)
                {
                    double contractSize =
                        double.Parse(uiStrategy_dataGridViewNoOptomizationParameters[1, 3].Value.ToString(), nfi);
                    double movement = double.Parse(
                        uiStrategy_dataGridViewOptomizationParameters[1, 0].Value.ToString(), nfi);
                    double quant2 = double.Parse(uiBlackSQuantity.Text, nfi);

                    var totalVega = (quant2*contractSize)*
                                    (_blackScholesCalculator.CallTheta(underlineprice, strikeprice, time, interestrate,volatily, divider)
                                     / _blackScholesCalculator.Vega(underlineprice, strikeprice, time, interestrate,
                                                                  volatily, divider));

                    var zimD = (quant2*_blackScholesCalculator.Gamma(underlineprice, strikeprice, time, interestrate,volatily, divider))*movement;

                    var reqTv = quant2*contractSize*_blackScholesCalculator.CallTheta(underlineprice, strikeprice, time, interestrate,volatily, divider);

                    uiBlackSRTV.Text = Math.Round(Math.Abs(reqTv), 4).ToString(nfi);
                    uiBlackSTVega.Text = Math.Round(Math.Abs(totalVega), 4).ToString(nfi);
                    uiBlackSZim.Text = Math.Round(zimD, 4).ToString(nfi);
                }
            }
        }

        private void uiBlackSExpiryDate_ValueChanged(object sender, EventArgs e)
        {
            UpdateOptionsCalc();
        }

        private void uiBTSummaryTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                for (int i = 0; i < uiSummary_dataGridViewBT.Rows.Count; i++)
                {
                    uiSummary_dataGridViewBT.Rows[i].Cells[3].Style.ForeColor = uiSummary_dataGridViewBT.Rows[i].Cells[3].Value.ToString().IndexOf("-", StringComparison.Ordinal) > -1 ? Color.Red : Color.Green;
                }
            }
        }

        private void WeeklyDataTableCellStyle(object sender, GridGetCellStyleEventArgs e)
        {
            double num;
            var nfi = new CultureInfo("en-US", false).NumberFormat;

            if (e.GridCell.Value.ToString().IndexOf("-", StringComparison.Ordinal) != -1)
                e.Style.TextColor = Color.Red;
            else
                if ((e.GridCell.Value.ToString().IndexOf(",", StringComparison.Ordinal) != -1))
                    e.Style.TextColor = Color.Green;
                else
                    if (double.TryParse(e.GridCell.Value.ToString(), NumberStyles.Float, nfi, out num))
                        e.Style.TextColor = Color.Green;
        }

        private void uiWeeklyDataTable_CompareElements(object sender, GridCompareElementsEventArgs e)
        {
            var a = e.ElementA as GridCell;
            var b = e.ElementB as GridCell;

            if (a != null && b != null && (a.Value is string && (a.ColumnIndex == 1 || a.ColumnIndex == 2 || a.ColumnIndex == 3)))
            {
                e.Cancel = true;
                var aValue = DateTime.ParseExact(a.Value.ToString(), DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture);
                var bValue = DateTime.ParseExact(b.Value.ToString(), DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss", CultureInfo.InvariantCulture);
                    if (aValue > bValue)
                        e.Result = 1;
                    else
                        if (aValue < bValue)
                        {
                            e.Result = -1;
                        }
                        else
                            e.Result = 0;
            }
        }

        private void uiReqTVCheck_CheckedChanged(object sender, EventArgs e)
        {
            var error = false;
            const int visibleTime = 3 * 1000; //in milliseconds
            if (uiStrategy_checkBoxXReqTVCheck.Checked)
            {
                if (uiBlackSQuantity.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSQuantity, 0, 0, visibleTime);
                    error = true;
                    uiStrategy_checkBoxXReqTVCheck.Checked = false;
                }
                if (uiBlackSUnderlyingPrice.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSUnderlyingPrice, 20, -10, visibleTime);
                    error = true;
                    uiStrategy_checkBoxXReqTVCheck.Checked = false;
                }
                if (uiBlackSVolality.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSVolality, 20, -10, visibleTime);
                    error = true;
                    uiStrategy_checkBoxXReqTVCheck.Checked = false;
                }
                if (uiBlackSPriceStrike.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSPriceStrike, 20, -10, visibleTime);
                    error = true;
                    uiStrategy_checkBoxXReqTVCheck.Checked = false;
                }
                if (uiBlackSInterestRate.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSInterestRate, 20, -10, visibleTime);
                    error = true;
                    uiStrategy_checkBoxXReqTVCheck.Checked = false;
                }
                if (!error)
                {
                    uiStrategy_checkBoxXReqTVCheck.Checked = true;
                    uiBlackSRTV.Enabled = true;
                    uiBlackSTVega.Enabled = true;
                    uiBlackSZim.Enabled = true;
                    uiBlackSQuantity.Enabled = true;

                    UpdateOptionsCalc();
                }
            }
            else
            {
                uiBlackSRTV.Text = "";
                uiBlackSTVega.Text = "";
                uiBlackSZim.Text = "";
                uiBlackSRTV.Enabled = false;
                uiBlackSTVega.Enabled = false;
                uiBlackSZim.Enabled = false;
                uiBlackSQuantity.Enabled = true;
                uiStrategy_checkBoxXReqTVCheck.Checked = false;
            }
        }

        private void KeyPressValidator(object sender, KeyPressEventArgs e)
        {
            var box = sender as TextBox;
            if (box != null && (e.KeyChar == '0' && box.Text == @"0"))
            {
                e.Handled = true;
                var textBox = sender as TextBox;
                textBox.Text = @"0";
            }

            if (e.KeyChar == '\b') return;
            var textBox1 = sender as TextBox;
            if (textBox1 != null)
            {
                string text = textBox1.Text.Insert(textBox1.SelectionStart, e.KeyChar.ToString(CultureInfo.InvariantCulture));
                var reg = new Regex(@"^((((0{1}|[1-9]\d*)\.?\d*)|([1-9]\d*)))$");
                if (!reg.IsMatch(text))
                {
                    e.Handled = true;
                }
            }
        }

        private void QuantityKeyPressValidator(object sender, KeyPressEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && (e.KeyChar == '0' && textBox.Text == @"0"))
            {
                e.Handled = true;
                textBox.Text = @"0";
            }
            if (e.KeyChar == '\b') return;
            var box = sender as TextBox;
            if (box != null)
            {
                var text = box.Text.Insert(box.SelectionStart, e.KeyChar.ToString(CultureInfo.InvariantCulture));
                if (text == "-")
                {
                    e.Handled = false;
                }
                else
                {
                    var reg = new Regex(@"^(-?(((0{1}|[1-9]\d*)\.?\d*)|([1-9]\d*)))$");
                    if (!reg.IsMatch(text))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void DGridKeyPressValidator(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && uiStrategy_dataGridViewNoOptomizationParameters.SelectedCells[0] != uiStrategy_dataGridViewNoOptomizationParameters[1, 0])
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    string text = textBox.Text.Insert(textBox.SelectionStart, e.KeyChar.ToString(CultureInfo.InvariantCulture));
                    if (e.KeyChar == '-' && text == "-")
                        e.Handled = false;
                    else
                    {
                        var reg = new Regex(@"^(-?(((0{1}|[1-9]\d*)\.?\d*)|([1-9]\d*)))$");
                        if (!reg.IsMatch(text))
                        {
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        private void SubmitTextBox(object sender, KeyEventArgs e)
        {
            var error = false;
            const int visibleTime = 3 * 1000; //in milliseconds
            if (e.KeyCode == Keys.Enter)
            {
                if (uiBlackSQuantity.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSQuantity, 0, 0, visibleTime);
                    error = true;
                }
                if (uiBlackSUnderlyingPrice.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSUnderlyingPrice, 20, -10, visibleTime);
                    error = true;
                }
                if (uiBlackSVolality.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSVolality, 20, -10, visibleTime);
                    error = true;
                }
                if (uiBlackSPriceStrike.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSPriceStrike, 20, -10, visibleTime);
                    error = true;

                }
                if (uiBlackSInterestRate.Text == "")
                {
                    var tt = new ToolTip();
                    tt.Show("Please, enter a valid number.", uiBlackSInterestRate, 20, -10, visibleTime);
                    error = true;

                }

                double outD;
                if (!(double.TryParse(uiBlackSQuantity.Text, out outD)) &&
                    !(double.TryParse(uiBlackSUnderlyingPrice.Text, out outD)) &&
                    !(double.TryParse(uiBlackSPriceStrike.Text, out outD)) &&
                        !(double.TryParse(uiBlackSVolality.Text, out outD)) &&
                            !(double.TryParse(uiBlackSInterestRate.Text, out outD)))
                    error = true;
                if (!error)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    UpdateOptionsCalc();
                }
                else
                    e.Handled = false;
            }
        }

        private void uiStrategyNoOptomizationParameters_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var tb = (DataGridViewTextBoxEditingControl)e.Control;
            tb.KeyPress += DGridKeyPressValidator;

            e.Control.KeyPress += DGridKeyPressValidator;
        }

        private void uiStrategyOptomizationParameters_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var tb = (DataGridViewTextBoxEditingControl)e.Control;
            tb.KeyPress += DGridKeyPressValidator;

            e.Control.KeyPress += DGridKeyPressValidator;
        }

        private void MainFormMetroApp_Shown(object sender, EventArgs e)
        {
            Text = @"Reversals v" + Application.ProductVersion;
            if (DataManager.Initialize(Settings.Default.sHost, Settings.Default.sDB, Settings.Default.sUser, Settings.Default.sPassword))
            {
                UpdateLists();
            }
            else
            {
                MessageBox.Show(@"Check settings for connection to db.");
                var fs = new FormSettings();
                var res = fs.ShowDialog();
                if (res == DialogResult.OK)
                {
                    UpdateLists();
                }
            }
            Refresh();
        }

        private void uiDataFromDB_CheckedChanged(object sender, EventArgs e)
        {
            if (uiStrategy_radioButtonDataFromDB.Checked)
            {
                _data = null;
                uiStrategy_buttonXStart.Enabled = false;

                uiCalendar_buttonXSave.Enabled = false;
                uiStrategy_panelExBlackScholesPanel.Enabled = false;
                uiStrategy_checkBoxXReqTVCheck.Checked = false;
                uiStrategy_comboBoxExSymbol.Items.Clear();
                uiCalendar_comboBoxXSymbol.Items.Clear();

                var symbolList = DataManager.GetContracts();

                foreach (var item in symbolList)
                {
                    uiStrategy_comboBoxExSymbol.Items.Add(item.ContractName);
                    uiCalendar_comboBoxXSymbol.Items.Add(item.ContractName);
                }

                uiStrategy_panelExOptions.Enabled = true;
                uiOptimizationInSample.Enabled = true;
                uiStrategy_panelExOptimizationOutOfSample.Enabled = true;
                uiStrategy_buttonXStop.Enabled = false;

                uiFilePathLabel.Visible = false;
                uiStrategy_buttonXFilePicker.Visible = false;
                uiStrategy_textBoxXFilepath.Visible = false;

                if (uiStrategy_comboBoxExSymbol.Items.Count > 0)
                {
                    uiStrategy_comboBoxExSymbol.SelectedIndex = 0;
                }
                uiCalendar_panelExSaving.Enabled = true;
            }
        }

        private void uiSymbol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiStrategy_radioButtonDataFromDB.Checked)
            {
                uiStrategy_comboBoxExDataSet.Items.Clear();
                uiStrategy_comboBoxExDataSet.Text = "";
                uiStrategy_dataGridViewNoOptomizationParameters.DataSource = null;
                uiStrategy_dataGridViewOptomizationParameters.DataSource = null;

                GetTicksFromDb(uiStrategy_comboBoxExSymbol.Text);

                _dataSets = DataManager.GetDatasets();
                int symbolId = _contracts.Find(a => a.ContractName == uiStrategy_comboBoxExSymbol.Text).CountractId;

                foreach (DataSetModel dataSet in _dataSets.Where(a => a.SymbolId == symbolId))
                {
                    uiStrategy_comboBoxExDataSet.Items.Add(dataSet.DataSetName);
                }
                uiStrategy_buttonXStart.Enabled = false;
                uiStrategy_buttonXStop.Enabled = false;

                uiAddDatasetButton.Enabled = true;
                uiDeleteDatasetButton.Enabled = true;
                uiStrategy_buttonXSaveDataset.Enabled = true;
                uiStrategy_buttonXCancelDataSetButton.Enabled = true;

                if (uiStrategy_comboBoxExDataSet.Items.Count > 0)
                {
                    uiStrategy_comboBoxExDataSet.SelectedIndex = 0;
                }
            }
        }

        private void GetTicksFromDb(string symbol)
        {

            var tickList = DataManager.GetContractData(symbol);
            if (tickList.Count > 0)
            {
                _data = new Data(tickList, Data.DataFileType.TickFromDb);

                _data.CreateData();

                //________//
                uiStrategy_dateTimeAdvOISStart.MaxDate = uiStrategy_dateTimeAdvOOOSStart.MaxDate = _data[_data.Count - 1].Time;
                uiStrategy_dateTimeAdvOISStart.MinDate = uiStrategy_dateTimeAdvOOOSStart.MinDate = _data[0].Time;

                uiStrategy_dateTimeAdvOISEnd.MaxDate = uiStrategy_dateTimeAdvOOOSEnd.MaxDate = _data[_data.Count - 1].Time;
                uiStrategy_dateTimeAdvOISEnd.MinDate = uiStrategy_dateTimeAdvOOOSEnd.MinDate = _data[0].Time;

                uiStrategy_dateTimeAdvOISStart.Value = _data[0].Time;
                uiStrategy_dateTimeAdvOISEnd.Value = uiStrategy_dateTimeAdvOOOSStart.Value = uiStrategy_dateTimeAdvOOOSEnd.Value = _data[_data.Count - 1].Time;

                uiStrategy_panelExOptions.Enabled = true;
                uiOptimizationInSample.Enabled = true;
                uiStrategy_panelExOptimizationOutOfSample.Enabled = true;
                uiStrategy_buttonXStop.Enabled = false;
            }
        }

        private void UpdateLists()
        {
            Invoke((Action)delegate
            {
                uiStatus_labelItemConnectionStatus.Text = DataManager.IsConnected() ? "Connected" : "NotConnected";
                _contracts = DataManager.GetContracts();
                // TODO: Load in comboboxes list of symbols when we connect or change symbols list

                uiDataArchive_checkedListBoxSymbols.Items.Clear();
                uiDataArchive_dataGridViewXContracts.RowCount = _contracts.Count;                
                uiCalendar_comboBoxXSymbol.Items.Clear();
                uiStrategy_comboBoxExDataSet.Items.Clear();
                for (int i = 0; i < _contracts.Count; i++)
                {
                    uiStrategy_comboBoxExDataSet.Items.Add(_contracts[i].ContractName);
                    uiCalendar_comboBoxXSymbol.Items.Add(_contracts[i].ContractName);
                    uiDataArchive_checkedListBoxSymbols.Items.Add(_contracts[i].ContractName);
                    //* add list
                    uiDataArchive_dataGridViewXContracts.Rows[i].Cells[0].Value =
                        i.ToString(CultureInfo.InvariantCulture);
                    uiDataArchive_dataGridViewXContracts.Rows[i].Cells[1].Value = _contracts[i].ContractName;
                    uiDataArchive_dataGridViewXContracts.Rows[i].Height = 30;
                    DateTime sDate = DataManager.GetStartDate(_contracts[i].ContractName);
                    DateTime eDate = DataManager.GetEndDate(_contracts[i].ContractName);
                    string sDateStr = sDate.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss");
                    string eDateStr = eDate.ToString(DateFormatsManager.CurrentShortDateFormat + " HH:mm:ss");

                    if (sDate == eDate && sDate == DateTime.Today)
                    {
                        sDateStr = "No data";
                        eDateStr = "No data";
                    }

                    uiDataArchive_dataGridViewXContracts.Rows[i].Cells[2].Value = sDateStr;
                    uiDataArchive_dataGridViewXContracts.Rows[i].Cells[3].Value = eDateStr;
                    //* add list
                }                
            });
        }

        #region DATA ARCHIVE

        private void buttonX_data_arch_add_Click(object sender, EventArgs e)
        {
            if (!DataManager.IsConnected())
            {
                ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, @"Cant's add contract. Not connected to database.");
                return;
            }
            var fAdd = new FormContractAdd
            {
                Location = uiDataArchive_buttonXAddSymbol.PointToScreen(new Point(0,40))
            };

            DialogResult dr = fAdd.ShowDialog();
            switch (dr)
            {
                case DialogResult.OK:
                    {

                        string contract = fAdd.textBoxX_contract_name.Text;
                        if (!_contracts.Exists(a => a.ContractName == contract))
                        {
                            DataManager.AddContract(contract);
                            UpdateLists();
                        }
                        else
                        {
                            ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, @"Can't add contract. This contract already exists.");
                        }

                        break;
                    }
            }
        }

        private void uiDataArchive_buttonXEditSymbol_Click(object sender, EventArgs e)
        {
            if (uiDataArchive_checkedListBoxSymbols.CheckedItems.Count <= 0)
            {
                ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, @"Please, select contract.");
                return;
            }
            var oldName = uiDataArchive_checkedListBoxSymbols.CheckedItems[0].ToString();

            var fEdit = new FormContractEdit
            {
                Location = uiDataArchive_buttonXAddSymbol.PointToScreen(new Point(0,40)),
                textBoxX_contract_name = { Text = oldName }
            };

            var dr = fEdit.ShowDialog();
            switch (dr)
            {
                case DialogResult.OK:
                    {

                        string contract = fEdit.textBoxX_contract_name.Text;
                        if (!_contracts.Exists(a => a.ContractName == contract))
                        {
                            DataManager.EditContract(oldName, contract);
                            UpdateLists();
                        }
                        else
                        {
                            ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, "Can't edit contract. This contract already exists.");
                        }

                        break;
                    }
            }
        }

        private void buttonX_data_archive_delete_Click(object sender, EventArgs e)
        {
            if (uiDataArchive_checkedListBoxSymbols.CheckedItems.Count > 0)
            {
                if (MessageBox.Show(@"Do you realy want to dalete contract with all data?", @"Deleting contract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (object t in uiDataArchive_checkedListBoxSymbols.CheckedItems)
                    {
                        DataManager.DelContract(t.ToString());
                    }

                    UpdateLists();
                }
            }
            else
            {
                ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, "Please, select contract.");
            }
        }

        private void buttonX_data_archive_start_Click(object sender, EventArgs e)
        {
            var contracts = (from object t in uiDataArchive_checkedListBoxSymbols.CheckedItems select t.ToString()).ToList();

            if (contracts.Count > 0)
            {
                IntradayTick.ProgressEvent += UpdateProgressBar;
                IntradayTick.CollectContracts(contracts, uiDataArchive_dateTimeInputStart.Value, uiDataArchive_dateTimeInputEnd.Value);
                ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, @"Collecting start.");
            }
            else
            {

                ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, @"Please, select contract.");
            }
        }

        void UpdateProgressBar(int value)
        {
            Invoke((Action)delegate
            {
                if (value == uiStatus_toolStripProgressBar.Minimum)
                {
                    uiStatus_labelItemProgramStatus.Text = @"Collecting start";
                }
                if (value == uiStatus_toolStripProgressBar.Maximum)
                {
                    uiStatus_labelItemProgramStatus.Text = @"Collecting finished";
                    UpdateLists();
                }
                uiStatus_toolStripProgressBar.Value = value;
            });
        }

        private void buttonX_data_archive_stop_Click(object sender, EventArgs e)
        {
            IntradayTick.Stop();
            ToastNotification.Show(uiDataArchive_checkedListBoxSymbols, @"Collecting stoped.");
        }

        #endregion

        #region DATASETS ADD, DEL, etc.

        private void uiDataFromFile_CheckedChanged(object sender, EventArgs e)
        {
            if (uiStrategy_radioButtonDataFromFile.Checked)
            {

                uiStrategy_panelExBlackScholesPanel.Enabled = false;
                uiStrategy_checkBoxXReqTVCheck.Checked = false;

                uiStrategy_comboBoxExDataSet.Items.Clear();
                uiStrategy_comboBoxExDataSet.Text = "";
                uiFilePathLabel.Visible = true;
                uiStrategy_buttonXFilePicker.Visible = true;
                uiStrategy_textBoxXFilepath.Visible = true;

                uiStrategy_dataGridViewNoOptomizationParameters.DataSource = null;
                uiStrategy_dataGridViewOptomizationParameters.DataSource = null;

                uiStrategy_buttonXStart.Enabled = false;
                uiStrategy_buttonXStop.Enabled = false;

                uiStrategy_comboBoxExSymbol.Text = "";

                uiAddDatasetButton.Enabled = false;
                uiDeleteDatasetButton.Enabled = false;
                uiStrategy_buttonXSaveDataset.Enabled = false;
                uiStrategy_buttonXCancelDataSetButton.Enabled = false;
                uiStrategy_comboBoxExSymbol.Items.Clear();
                uiCalendar_panelExSaving.Enabled = false;
            }
        }

        private void uiAddDatasetButton_Click(object sender, EventArgs e)
        {
            var provider = uiStrategy_dataGridViewNoOptomizationParameters.DataSource as DataTable;
            var optProvider = uiStrategy_dataGridViewOptomizationParameters.DataSource as DataTable;
            if (provider != null && optProvider != null)
            {
                foreach (DataRow row in provider.Rows)
                {
                    row["Default Value"] = 0;
                }
                provider.Rows[0]["Default Value"] = "Untitled DataSet";
                _addingNewDataSet = true;
                uiStrategy_comboBoxExDataSet.Text = "";

                foreach (DataRow row in optProvider.Rows)
                {
                    row[1] = 0;
                    row[2] = 0;
                    row[3] = 0;
                    row[4] = 0;
                }

                uiStrategy_dataGridViewNoOptomizationParameters.CurrentCell = uiStrategy_dataGridViewNoOptomizationParameters[1, 0];
            }
            else
            {
                var parameterProvider = new DataTable();
                var column = new DataColumn("Variables");
                var column2 = new DataColumn("Default Value");
                parameterProvider.Columns.Add(column);
                parameterProvider.Columns.Add(column2);

                var row = parameterProvider.NewRow();
                row[0] = "DataSet Name";
                row[1] = "Untitled DataSet";
                parameterProvider.Rows.Add(row);

                row = parameterProvider.NewRow();
                row[0] = "Time Value";
                row[1] = 0;
                parameterProvider.Rows.Add(row);

                row = parameterProvider.NewRow();
                row[0] = "Commission";
                row[1] = 0;
                parameterProvider.Rows.Add(row);

                row = parameterProvider.NewRow();
                row[0] = "Multiplier";
                row[1] = 0;
                parameterProvider.Rows.Add(row);

                row = parameterProvider.NewRow();
                row[0] = "Contract Size";
                row[1] = 0;
                parameterProvider.Rows.Add(row);

                row = parameterProvider.NewRow();
                row[0] = "Point Value";
                row[1] = 0;
                parameterProvider.Rows.Add(row);

                row = parameterProvider.NewRow();
                row[0] = "ZIM";
                row[1] = 0;
                parameterProvider.Rows.Add(row);

                row = parameterProvider.NewRow();
                row[0] = "Tick Size";
                row[1] = 0;
                parameterProvider.Rows.Add(row);

                var optimalizableparamsProvider = new DataTable();

                column = new DataColumn("Triggers");
                optimalizableparamsProvider.Columns.Add(column);
                column = new DataColumn("Default Value");
                optimalizableparamsProvider.Columns.Add(column);
                column = new DataColumn("MinValue");
                optimalizableparamsProvider.Columns.Add(column);
                column = new DataColumn("MaxValue");
                optimalizableparamsProvider.Columns.Add(column);
                column = new DataColumn("Step");
                optimalizableparamsProvider.Columns.Add(column);

                row = optimalizableparamsProvider.NewRow();
                row[0] = "Stop Level";
                row[1] = 0;
                row[2] = 0;
                row[3] = 0;
                row[4] = 0;
                optimalizableparamsProvider.Rows.Add(row);

                row = optimalizableparamsProvider.NewRow();
                row[0] = "Reversal Level";
                row[1] = 0;
                row[2] = 0;
                row[3] = 0;
                row[4] = 0;
                optimalizableparamsProvider.Rows.Add(row);

                parameterProvider.Columns["Variables"].ReadOnly = true;
                uiStrategy_dataGridViewNoOptomizationParameters.DataSource = parameterProvider;
                uiStrategy_dataGridViewOptomizationParameters.DataSource = optimalizableparamsProvider;
                _addingNewDataSet = true;
                uiStrategy_comboBoxExDataSet.Text = "";
                uiStrategy_dataGridViewNoOptomizationParameters.CurrentCell = uiStrategy_dataGridViewNoOptomizationParameters[1, 0];
            }
            uiStrategy_WorkingButtonsPanel.Enabled = false;
        }

        private void uiDeleteDatasetButton_Click(object sender, EventArgs e)
        {
            const string message = "Are you sure that you would like to delete current DataSet?";
            const string caption = "DataSet delete";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                int dataSetId = _dataSets.Find(a => a.DataSetName == uiStrategy_comboBoxExDataSet.Text).Id;
                DataManager.DelDataset(dataSetId);
                _dataSets = DataManager.GetDatasets();
                uiStrategy_comboBoxExDataSet.Items.Clear();

                int symbolId = _contracts.Find(a => a.ContractName == uiStrategy_comboBoxExSymbol.Text).CountractId;

                foreach (DataSetModel dataSet in _dataSets.Where(a => a.SymbolId == symbolId))
                {
                    uiStrategy_comboBoxExDataSet.Items.Add(dataSet.DataSetName);
                }
                uiStrategy_comboBoxExDataSet.Text = "";

                uiStrategy_dataGridViewNoOptomizationParameters.DataSource = null;
                uiStrategy_dataGridViewOptomizationParameters.DataSource = null;

                uiStrategy_WorkingButtonsPanel.Enabled = false;
            }
        }

        private void uiSaveDatasetButton_Click(object sender, EventArgs e)
        {
            var provider = uiStrategy_dataGridViewNoOptomizationParameters.DataSource as DataTable;
            var optProvider = uiStrategy_dataGridViewOptomizationParameters.DataSource as DataTable;
            var dataSetModel = new DataSetModel();
            try
            {
                if (provider != null && optProvider != null)
                {
                    dataSetModel.SymbolId = _contracts.Find(a => a.ContractName == uiStrategy_comboBoxExSymbol.Text).CountractId;
                    dataSetModel.DataSetName = provider.Rows[0]["Default Value"].ToString();
                    dataSetModel.TimeValue = double.Parse(provider.Rows[1]["Default Value"].ToString(), _nfi);
                    dataSetModel.Commission = double.Parse(provider.Rows[2]["Default Value"].ToString(), _nfi);
                    dataSetModel.Multiplier = int.Parse(provider.Rows[3]["Default Value"].ToString());
                    dataSetModel.ContractSize = int.Parse(provider.Rows[4]["Default Value"].ToString(), _nfi);
                    dataSetModel.PointValue = double.Parse(provider.Rows[5]["Default Value"].ToString(), _nfi);
                    dataSetModel.Zim = double.Parse(provider.Rows[6]["Default Value"].ToString(), _nfi);
                    dataSetModel.TickSize = double.Parse(provider.Rows[7]["Default Value"].ToString(), _nfi);

                    dataSetModel.StopLevelDef = double.Parse(optProvider.Rows[0][1].ToString());
                    dataSetModel.StopLevelMin = double.Parse(optProvider.Rows[0][2].ToString());
                    dataSetModel.StopLevelMax = double.Parse(optProvider.Rows[0][3].ToString());
                    dataSetModel.StopLevelStep = double.Parse(optProvider.Rows[0][4].ToString());

                    dataSetModel.ReversalLevelDef = double.Parse(optProvider.Rows[1][1].ToString());
                    dataSetModel.ReversalLevelMin = double.Parse(optProvider.Rows[1][2].ToString());
                    dataSetModel.ReversalLevelMax = double.Parse(optProvider.Rows[1][3].ToString());
                    dataSetModel.ReversalLevelStep = double.Parse(optProvider.Rows[1][4].ToString());
                    _tickSize = dataSetModel.TickSize;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"Please check entered values");
                return;
            }

            if (_dataSets.Exists(a => a.DataSetName == uiStrategy_comboBoxExDataSet.Text) || _dataSets.Exists(a => a.DataSetName == dataSetModel.DataSetName))
            {
                var dataSetId = _dataSets.Find(a => a.DataSetName == uiStrategy_comboBoxExDataSet.Text).Id;
                if (DataManager.EditDataset(dataSetId, dataSetModel))
                {
                    _dataSets = DataManager.GetDatasets();
                    uiStrategy_comboBoxExDataSet.Items.Clear();
                    uiCalendar_comboBoxXDSet.Items.Clear();
                    var symbolId = _contracts.Find(a => a.ContractName == uiStrategy_comboBoxExSymbol.Text).CountractId;

                    foreach (var dataSet in _dataSets.Where(a => a.SymbolId == symbolId))
                    {
                        uiCalendar_comboBoxXDSet.Items.Add(dataSet.DataSetName);
                        uiStrategy_comboBoxExDataSet.Items.Add(dataSet.DataSetName);
                    }
                    uiStrategy_comboBoxExDataSet.Text = dataSetModel.DataSetName;
                    uiCalendar_comboBoxXDSet.Text = dataSetModel.DataSetName;
                    UpdateOptParamsInstance(dataSetModel);
                }
            }
            else
            {
                if (DataManager.AddDataset(dataSetModel))
                {
                    uiStrategy_comboBoxExDataSet.Items.Add(dataSetModel.DataSetName);
                    uiCalendar_comboBoxXDSet.Items.Add(dataSetModel.DataSetName);
                    _dataSets = DataManager.GetDatasets();

                    UpdateOptParamsInstance(dataSetModel);
                    uiStrategy_comboBoxExDataSet.SelectedIndex = uiStrategy_comboBoxExDataSet.Items.Count - 1;
                    uiCalendar_comboBoxXDSet.SelectedIndex = uiCalendar_comboBoxXDSet.Items.Count - 1;
                }
            }
            uiStrategy_WorkingButtonsPanel.Enabled = true;
            _addingNewDataSet = false;
        }

        private void uiCancelDataSetButton_Click(object sender, EventArgs e)
        {
            if (_addingNewDataSet == false)
            {
                
                var currentDataSet = DataManager.GetDatasetData(_dataSets.First(a => a.DataSetName == uiStrategy_comboBoxExDataSet.Text).Id);
                DisplayDatasetParams(currentDataSet);
                UpdateOptParamsInstance(currentDataSet);
                if (_data != null)
                {
                    uiStrategy.SelectedIndex = 0;
                }

                uiAddDatasetButton.Enabled = true;
                uiDeleteDatasetButton.Enabled = true;
                uiStrategy_buttonXSaveDataset.Enabled = true;
                uiStrategy_buttonXCancelDataSetButton.Enabled = true;
            }
            else if (_addingNewDataSet)
            {
                var provider = uiStrategy_dataGridViewNoOptomizationParameters.DataSource as DataTable;
                var optProvider = uiStrategy_dataGridViewOptomizationParameters.DataSource as DataTable;
                if (provider != null)
                {
                    foreach (DataRow row in provider.Rows)
                    {
                        row["Default Value"] = 0;
                    }
                    provider.Rows[0][1] = "Untitled DataSet";
                }

                if (optProvider != null)
                    foreach (DataRow row in optProvider.Rows)
                    {
                        row[1] = 0;
                        row[2] = 0;
                        row[3] = 0;
                        row[4] = 0;
                    }
                }
        }


        private void UpdateOptParamsInstance(DataSetModel currDataSet)
        {
            OptimizationParameters.Instance.Parameters.Clear();
            OptimizationParameters.Instance.Parameters.Add(new OptimizationParameter("", "Stop Level", typeof(double),
                                                 currDataSet.StopLevelMin, currDataSet.StopLevelMax,
                                                 currDataSet.StopLevelDef,
                                                 currDataSet.StopLevelStep));
            OptimizationParameters.Instance.Parameters.Add(new OptimizationParameter("", "Reversal Level", typeof(double),
                                                 currDataSet.ReversalLevelMin, currDataSet.ReversalLevelMax,
                                                 currDataSet.ReversalLevelDef,
                                                 currDataSet.ReversalLevelStep));
        }

        #endregion

        #region CALENDAR FROM AND TO DATABASE

//
        private void InitCalendar()
        {
            uiCalendar_calendarViewResultView.CalendarModel.Appointments.Clear();

            foreach (var item in _calendarDisplayer.Appointments)
                uiCalendar_calendarViewResultView.CalendarModel.Appointments.Add(item);
            Invoke((Action)SetCalendarValues);


        }

        private void SetCalendarValues()
        {
            uiCalendar_calendarViewResultView.SelectedView = _calendarDisplayer.CalendarViewMode;
            uiCalendar_calendarViewResultView.MonthViewEndDate = _calendarDisplayer.MonthViewEndDay;
            uiCalendar_calendarViewResultView.MonthViewStartDate = _calendarDisplayer.MonthViewStartDay;
            uiCalendar_labelXWeek1.Text = _calendarDisplayer.WeekOne;
            uiCalendar_labelXWeek2.Text = _calendarDisplayer.WeekTwo;
            uiCalendar_labelXWeek3.Text = _calendarDisplayer.WeekThree;
            uiCalendar_labelXWeek4.Text = _calendarDisplayer.WeekFour;
            uiCalendar_labelXWeek5.Text = _calendarDisplayer.WeekFive;
            uiCalendar_labelXWeek6.Text = _calendarDisplayer.WeekSix;
            SetLabelColor();
            uiCalendar_labelXMonthTotal.Text = _calendarDisplayer.MonthlyTotal;
            uiMonthlyCaption.Text = @"Monthly total for " + _calendarDisplayer.CurrentMonthName + @":";
            SetCalendarWeeksCount(!_calendarDisplayer.IsSixWeekInMonth);

        }
        private void uiCalendarSymbol_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dataSets = DataManager.GetDatasets();
            var symbolId = _contracts.Find(a => a.ContractName == uiCalendar_comboBoxXSymbol.Text).CountractId;
            uiCalendar_comboBoxXDSet.Items.Clear();
            uiCalendar_comboBoxXDSet.Text = "";

            foreach (DataSetModel dataSet in _dataSets.Where(a => a.SymbolId == symbolId))
            {
                uiCalendar_comboBoxXDSet.Items.Add(dataSet.DataSetName);
            }
        }

        private void uiCalendarDSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiCalendar_comboBoxXDSet.SelectedIndex != -1)
            {
                uiCalendar_buttonXLoad.Enabled = true;
                uiCalendar_buttonXDelete.Enabled = true;
            }
        }
        private void uiCalendarDelete_Click(object sender, EventArgs e)
        {
            int symbolId = 0;
            int dsetId = 0;
            if (_calendarDisplayer.Appointments.Count != 0)
            {
                uiCalendar_calendarViewResultView.CalendarModel.Appointments.Clear();
                uiCalendar_calendarViewResultView.Refresh();
                uiCalendar_labelXWeek1.Text = "";
                uiCalendar_labelXWeek2.Text = "";
                uiCalendar_labelXWeek3.Text = "";
                uiCalendar_labelXWeek4.Text = "";
                uiCalendar_labelXWeek5.Text = "";
                uiCalendar_labelXMonthTotal.Text = "";
                uiCalendar_labelXMonth.Text = "";
                uiCalendar_labelXWeek6.Text = "";



              
               _symbolList = DataManager.GetContracts();

                _calendarResult.Clear();
                _weeklist.Clear();

                foreach (var item in _symbolList)
                {
                    if (uiCalendar_comboBoxXSymbol.Text == item.ContractName)
                        symbolId = item.CountractId;
                }

            _datasetList = DataManager.GetDatasets();

                foreach (var item in _datasetList)
                {
                    if (uiCalendar_comboBoxXDSet.Text == item.DataSetName)
                        dsetId = item.Id;
                }

                DataManager.DelResult(symbolId, dsetId);
                ToastNotification.Show(this,"CALENDAR ITEMS ARE DELETED");
                uiCalendar_calendarViewResultView.CalendarModel.Appointments.Clear();
            }
        }

        private void SetCalendarWeeksCount(bool isFive)
        {
            if (isFive)
            {
                tableLayoutPanelCalendar.Hide();
                tableLayoutPanelCalendar.RowStyles[7].Height = 0;
                uiCalendar_panelExWhiteCell6.Visible = false;
                tableLayoutPanelCalendar.Show();
            }
            else
            {
                tableLayoutPanelCalendar.Hide();
                tableLayoutPanelCalendar.RowStyles[7].Height = (float)16.67;
                uiCalendar_panelExWhiteCell6.Visible = true;
                tableLayoutPanelCalendar.Show();
            }
        }
        private void uiCalendarLoad_Click(object sender, EventArgs e)
        {

            int symbolId = 0;
            int dsetId = 0;

            
            if (_symbolList == null)
                _symbolList = DataManager.GetContracts();

            _calendarResult.Clear();
            _weeklist.Clear();

            foreach (var item in _symbolList)
            {
                if (uiCalendar_comboBoxXSymbol.Text == item.ContractName)
                    symbolId = item.CountractId;
            }
            
            _datasetList = DataManager.GetDatasets();

            foreach (var item in _datasetList)
            {
                if (uiCalendar_comboBoxXDSet.Text == item.DataSetName)
                    dsetId = item.Id;
            }

            uiCalendar_calendarViewResultView.CalendarModel.Appointments.Clear();
            var list = DataManager.GetResult(symbolId, dsetId);
            _calendarDisplayer = new CalendarDisplayer(list);
           
            if (_calendarDisplayer.Appointments.Count != 0) 
            {ToastNotification.Show(this, "CALENDAR ITEMS ARE LOADED.");
              InitCalendar();
            
            }
            else
            {
                uiCalendar_calendarViewResultView.CalendarModel.Appointments.Clear();
                uiCalendar_calendarViewResultView.Refresh();
                uiCalendar_labelXWeek1.Text = "";
                uiCalendar_labelXWeek2.Text = "";
                uiCalendar_labelXWeek3.Text = "";
                uiCalendar_labelXWeek4.Text = "";
                uiCalendar_labelXWeek5.Text = "";
                uiCalendar_labelXMonthTotal.Text = "";
                uiCalendar_labelXMonth.Text = "";
                uiCalendar_labelXWeek6.Text = "";
              
            }
            
            

        }

        private void uiCalendarSave_Click(object sender, EventArgs e)
        {
            int symbolId = 0;
            int dataSetId = 0;
            _symbolList = DataManager.GetContracts();
            foreach (var item in _symbolList)
            {
                if (item.ContractName == uiCalendar_comboBoxXSymbol.Text)
                    symbolId = item.CountractId;
            }
            _datasetList = DataManager.GetDatasets();

            foreach (var item in _datasetList)
            {
                if (item.DataSetName == uiCalendar_comboBoxXDSet.Text)
                    dataSetId = item.Id;
            }
            if (_calendarResult != null)
            {
                _calendarDisplayer.SaveCalendarItem(symbolId, dataSetId);
                ToastNotification.Show(this, "CALENDAR ITEMS ARE SAVED.");
            }
        }
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (_calendarDisplayer != null)
            {
                _calendarDisplayer.MonthRangeDecrement();
                SetCalendarValues();
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (_calendarDisplayer != null)
            {
                _calendarDisplayer.MonthRangeIncrement();
                SetCalendarValues();
            }
        }

        private void SetLabelColor()
        {
            uiCalendar_labelXWeek1.ForeColor = (uiCalendar_labelXWeek1.Text.IndexOf("-", StringComparison.Ordinal) > -1)
                                                   ? Color.Crimson
                                                   : Color.Green;
            uiCalendar_labelXWeek2.ForeColor = (uiCalendar_labelXWeek2.Text.IndexOf("-", StringComparison.Ordinal) > -1)
                                                 ? Color.Crimson
                                                 : Color.Green;
            uiCalendar_labelXWeek3.ForeColor = (uiCalendar_labelXWeek3.Text.IndexOf("-", StringComparison.Ordinal) > -1)
                                                 ? Color.Crimson
                                                 : Color.Green;
            uiCalendar_labelXWeek4.ForeColor = (uiCalendar_labelXWeek4.Text.IndexOf("-", StringComparison.Ordinal) > -1)
                                                 ? Color.Crimson
                                                 : Color.Green;
            uiCalendar_labelXWeek5.ForeColor = (uiCalendar_labelXWeek5.Text.IndexOf("-", StringComparison.Ordinal) > -1)
                                                 ? Color.Crimson
                                                 : Color.Green;
            uiCalendar_labelXWeek6.ForeColor = (uiCalendar_labelXWeek6.Text.IndexOf("-", StringComparison.Ordinal) > -1)
                                                 ? Color.Crimson
                                                 : Color.Green;
            uiCalendar_labelXMonthTotal.ForeColor = (uiCalendar_labelXMonthTotal.Text.IndexOf("-", StringComparison.Ordinal) > -1)
                                                 ? Color.Crimson
                                                 : Color.Green;

        }

//
        


        #endregion
   
        #region VALIDATING DATE SELECT

        private void uiOISStart_MonthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            var endDate = e.End;

            if (endDate.Day >= uiStrategy_dateTimeAdvOISEnd.Value.Day)
            {
                uiStrategy_dateTimeAdvOISStart.Value = endDate.AddDays(-1);
            }
        }

        private void uiOISEnd_MonthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            var endDate = e.End;

            if (endDate.Day <= uiStrategy_dateTimeAdvOISStart.Value.Day)
            {
                uiStrategy_dateTimeAdvOISEnd.Value = endDate.AddDays(1);
            }
        }

        private void uiOOOSStart_MonthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            var endDate = e.End;

            if (endDate.Day >= uiStrategy_dateTimeAdvOISEnd.Value.Day)
            {
                uiStrategy_dateTimeAdvOOOSStart.Value = endDate.AddDays(-1);
            }
        }

        private void uiOOOSEnd_MonthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            var endDate = e.End;

            if (endDate.Day <= uiStrategy_dateTimeAdvOISStart.Value.Day)
            {
                uiStrategy_dateTimeAdvOOOSEnd.Value = endDate.AddDays(1);
            }
        }

        #endregion

        private void uiStrategyNoOptomizationParameters_DataSourceChanged(object sender, EventArgs e)
        {
            if (uiStrategy_dataGridViewNoOptomizationParameters.Columns["Default Value"] != null)
            {
                uiStrategy_dataGridViewNoOptomizationParameters.Columns["Default Value"].Width = 153;
                uiStrategy_dataGridViewNoOptomizationParameters.ColumnHeadersHeight = 4;
            }
        }

        private void uiStrategyOptomizationParameters_DataSourceChanged(object sender, EventArgs e)
        {
            if (uiStrategy_dataGridViewOptomizationParameters.Columns.Count > 0)
            {
                uiStrategy_dataGridViewOptomizationParameters.ColumnHeadersHeight = 4;

                foreach (DataGridViewColumn col in uiStrategy_dataGridViewOptomizationParameters.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
        }

        private void uiStrategyNoOptomizationParameters_CellEndEdit_1(object sender, DataGridViewCellEventArgs e)
        {
            for (var i = 1; i < uiStrategy_dataGridViewNoOptomizationParameters.Rows.Count; i++)
            {
                if (uiStrategy_dataGridViewNoOptomizationParameters[1, i].Value.ToString() == "" || uiStrategy_dataGridViewNoOptomizationParameters[1, i].Value.ToString() == "-")
                {
                    uiStrategy_dataGridViewNoOptomizationParameters[1, i].Value = 0.0;
                }
            }

            var rowindex = e.RowIndex;
            var columnindex = e.ColumnIndex;


            if ((rowindex == 6) && (columnindex == 1)&&(uiStrategy_dataGridViewOptomizationParameters.DataSource != null))
            {
                var zim = double.Parse(uiStrategy_dataGridViewNoOptomizationParameters[columnindex, rowindex].Value.ToString(), _nfi);
                //ticksize = double.Parse(uiStrategy_dataGridViewNoOptomizationParameters[1, 7].Value.ToString(), _nfi);
                var step = double.Parse(uiStrategy_dataGridViewOptomizationParameters[1, 0].Value.ToString(), _nfi);
                var pvalue = zim / step;
                uiStrategy_dataGridViewNoOptomizationParameters[1, 5].Value = Math.Round(pvalue, 5);
                //UpdateOptionsCalc();
            }
        }

        private void uiStrategyOptomizationParameters_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int rowindex = e.RowIndex;
            int columnindex = e.ColumnIndex;
            double zim;
            double pvalue;
            double step;

            for (int i = 1; i < uiStrategy_dataGridViewOptomizationParameters.ColumnCount; i++)
            {
                if (uiStrategy_dataGridViewOptomizationParameters[i, 0].Value.ToString() == "")
                {
                    uiStrategy_dataGridViewOptomizationParameters[i, 0].Value = 0.0;
                }
                if (uiStrategy_dataGridViewOptomizationParameters[i, 1].Value.ToString() == "")
                {
                    uiStrategy_dataGridViewOptomizationParameters[i, 1].Value = 0.0;
                }
            }

            if ((rowindex == 0) && (columnindex == 2))
            {
                zim = double.Parse(uiStrategy_dataGridViewNoOptomizationParameters[1, 6].Value.ToString(), _nfi);
                //ticksize = double.Parse(uiStrategy_dataGridViewNoOptomizationParameters[1, 7].Value.ToString(), _nfi);
                step = double.Parse(uiStrategy_dataGridViewOptomizationParameters[columnindex, rowindex].Value.ToString(), _nfi);
                pvalue = zim / step;
                uiStrategy_dataGridViewNoOptomizationParameters[1, 5].Value = Math.Round(pvalue, 5);
            }
            if ((rowindex == 0) && (columnindex == 1))
            {
                zim = double.Parse(uiStrategy_dataGridViewNoOptomizationParameters[1, 6].Value.ToString(), _nfi);
                //ticksize = double.Parse(uiStrategy_dataGridViewNoOptomizationParameters[1, 7].Value.ToString(), _nfi);
                step = double.Parse(uiStrategy_dataGridViewOptomizationParameters[columnindex, rowindex].Value.ToString(), _nfi);
                pvalue = zim / step;
                uiStrategy_dataGridViewNoOptomizationParameters[1, 5].Value = Math.Round(pvalue, 5);
                UpdateOptionsCalc();
            }

            _optParameterssChanging = new ParametersChangingNotifier();

            _optParameterssChanging.PropertyChanged += OptimizationParameters.Instance.UpdateParameter;   
           
            _optParameterssChanging.ParameterName = uiStrategy_dataGridViewOptomizationParameters.Rows[e.RowIndex].Cells[0].Value.ToString();
            _optParameterssChanging.DefaultValue = uiStrategy_dataGridViewOptomizationParameters.Rows[e.RowIndex].Cells[1].Value;
            _optParameterssChanging.MinValue = uiStrategy_dataGridViewOptomizationParameters.Rows[e.RowIndex].Cells[2].Value;
            _optParameterssChanging.MaxValue = uiStrategy_dataGridViewOptomizationParameters.Rows[e.RowIndex].Cells[3].Value;
            _optParameterssChanging.Step = uiStrategy_dataGridViewOptomizationParameters.Rows[e.RowIndex].Cells[4].Value;
                
            _optParameterssChanging.PropertyChanged -= OptimizationParameters.Instance.UpdateParameter;
        }

        #region PREVIEW TICK DATA

        private void PreviewChangeSymbol()
        {
            if (uiDataArchive_dataGridViewXContracts.SelectedRows.Count <= 0) return;

            uiDataArchive_dateTimeInputFindTime.Value = DateTime.Today;
            var symbol = uiDataArchive_dataGridViewXContracts.SelectedRows[0].Cells[1].Value.ToString();

            PreviewTickData.SetCurrentSymbol(symbol);

            if (PreviewTickData.TickDataExists)
            {
                uiDataArchive_panelExSearchPanel.Enabled = true;

                uiDataArchive_dateTimeInputFindDate.MinDate = PreviewTickData.TickStartDate;
                uiDataArchive_dateTimeInputFindDate.MaxDate = PreviewTickData.TickEndDate;

                uiDataArchive_dateTimeInputFindDate.Value = PreviewTickData.TickStartDate;
                PreviewShowTickData();
            }
            else
            {
                uiDataArchive_panelExSearchPanel.Enabled = false;
                uiDataArchive_dataGridViewXPreview.RowCount = 0;
            }
        }

        private void PreviewShowTickData()
        {
            var list1 = PreviewTickData.GetTickData(uiDataArchive_dateTimeInputFindDate.Value.Date);

            var n = list1.Count();
            uiDataArchive_dataGridViewXPreview.RowCount = n;

            for (var i = 0; i < n; i++)
            {
                if (list1[i].Date.Date != uiDataArchive_dateTimeInputFindDate.Value.Date) continue;

                uiDataArchive_dataGridViewXPreview.Rows[i].Cells[0].Value =
                    list1[i].Date.ToString(DateFormatsManager.CurrentShortDateFormat) +
                    " " + list1[i].Date.ToString("HH:mm:ss");
                uiDataArchive_dataGridViewXPreview.Rows[i].Cells[1].Value = list1[i].Price;
            }
            if (n > 0)
            {
                uiDataArchive_dataGridViewXPreview.ClearSelection();
                uiDataArchive_dataGridViewXPreview.Rows[0].Selected = true;
                uiDataArchive_dataGridViewXPreview.FirstDisplayedScrollingRowIndex = 0;
            }
        }
        
        private void uiDataArchive_buttonXFind_Click(object sender, EventArgs e)
        {
            var n = uiDataArchive_dataGridViewXPreview.Rows.Count;
            if (n == 0) return;
            var searchString =
                uiDataArchive_dateTimeInputFindDate.Value.ToString(DateFormatsManager.CurrentShortDateFormat) + " " + uiDataArchive_dateTimeInputFindTime.Value.ToString("HH:mm");

            
            uiDataArchive_dataGridViewXPreview.ClearSelection();
            for (var i = 0; i < n; i++)
            {
                var str = uiDataArchive_dataGridViewXPreview.Rows[i].Cells[0].Value.ToString();
                var currDt = str.Substring(0,str.Length - 3);                

                if (currDt == searchString)
                {
                    uiDataArchive_dataGridViewXPreview.Rows[i].Selected = true;
                    uiDataArchive_dataGridViewXPreview.FirstDisplayedScrollingRowIndex = i;
                    break;
                }
            }
            if (uiDataArchive_dataGridViewXPreview.SelectedRows.Count <= 0)
            {
                ToastNotification.Show(uiDataArchive_dataGridViewXPreview, "Nothing found.");
            }
        }
        
        private void uiDataArchive_dateTimeInputFindDate_ValueChanged(object sender, EventArgs e)
        {
            PreviewShowTickData();
            uiDataArchive_dataGridViewXPreview.Focus();
        }

        
        private void uiDataArchive_dataGridViewXContracts_CurrentCellChanged(object sender, EventArgs e)
        {
            var defTimeOut= ToastNotification.DefaultTimeoutInterval;
            ToastNotification.DefaultTimeoutInterval = 900;
            ToastNotification.Show(uiDataArchive_dataGridViewXPreview, "Loading...");
            PreviewChangeSymbol();
            ToastNotification.DefaultTimeoutInterval = defTimeOut;

        }

        #endregion


        private void metroShellMain_SelectedTabChanged(object sender, EventArgs e)
        {
            if (metroShellMain.SelectedTab == ui_tabItem_data_archive && uiDataArchive_dataGridViewXPreview.Rows.Count==0)
                PreviewChangeSymbol();
        }

    }
}
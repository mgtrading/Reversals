using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Reversals.Display.SummaryDisplayer;
using Reversals.Strategies;

namespace Reversals.Optimization
{
    class Optimizer
    {
        private readonly Strategy _startStrategy;
        private Strategy _strategy;
        private OptimizationParameter _parameterStep1;
        private OptimizationParameter _parameterStep2;
        private double _parameterStep1CurrentValue;
        private double _parameterStep2CurrentValue;
        private readonly double _step1Incrementor;
        private readonly double _step2Incrementor;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;

        public delegate void ProgressEventHandler(double precent);

        public ProgressEventHandler ProgressEvent;


        public Optimizer(Strategy strategy, IEnumerable<OptimizationParameter> optParameters, DateTime startTime, DateTime endTime)
        {
            _startStrategy = strategy;
            _strategy = strategy;
            _startTime = startTime;
            _endTime = endTime;

            foreach (OptimizationParameter optParameter in optParameters)
            {
                if (optParameter.Name == "Stop Level")
                {
                    _parameterStep1 = optParameter;
                    _step1Incrementor = (double)optParameter.Step;
                    _parameterStep1CurrentValue = (double)optParameter.MinValue;
                }
                else if (optParameter.Name == "Reversal Level")
                {
                    _parameterStep2 = optParameter;
                    _step2Incrementor = (double)optParameter.Step;
                    _parameterStep2CurrentValue = (double)optParameter.MinValue;
                }
            }
            System.IO.File.Delete(@"Log1.txt");
        }

        public void StartOptimize(DataGridView summaryTable)
        {
            double stepPercecnt = 100 / (((double)_parameterStep1.MaxValue - (double)_parameterStep1.MinValue + _step1Incrementor) / _step1Incrementor * ((double)_parameterStep2.MaxValue - (double)_parameterStep2.MinValue + _step2Incrementor) / _step2Incrementor);
            _strategy.IsOptimisation = true;
            if (_strategy is StepChange)
            {
                while (_parameterStep1CurrentValue <= (double)_parameterStep1.MaxValue)
                {
                    _strategy = null;
                    _strategy = _startStrategy;
                    var parameterStep1 = new Strategy.StrategyAdditionalParameter("", "Stop Level", Math.Round(_parameterStep1CurrentValue), typeof(double),true);
                    while (_parameterStep2CurrentValue <= (double)_parameterStep2.MaxValue)
                    {
                        var parameterStep2 = new Strategy.StrategyAdditionalParameter("","Reversal Level",Math.Round(_parameterStep2CurrentValue),typeof(double),true);
                        _strategy.AdditionalParameters[7] = parameterStep1;
                        _strategy.AdditionalParameters[8] = parameterStep2;
                        _strategy.Start(0);
                        DisplayBackTestResult(summaryTable);
                        _strategy.Clear();
                        ProgressNotify(stepPercecnt);
                        _parameterStep2CurrentValue += _step2Incrementor;
                    }
                    _parameterStep1CurrentValue += _step1Incrementor;
                    _parameterStep2CurrentValue = (double)_parameterStep2.MinValue;
                }
            }

            _strategy.IsOptimisation = false;
        }

        private void DisplayBackTestResult(DataGridView summaryTable)
        {
            var summaryDisplayer = new SummaryDisplayer(_strategy, _startTime, _endTime);

            summaryDisplayer.DisplayTable(summaryTable);
        }

        private void ProgressNotify(double percent)
        {
            if (ProgressEvent != null)
            {
                ProgressEvent(percent);
            }
        }
    }
}

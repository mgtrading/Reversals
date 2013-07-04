using System.Collections.Generic;
using Reversals.DataContainer;
using Reversals.Strategies;

namespace Reversals.TestWorking
{
    static class TestManager
    {
        #region EVENT HANDLERS

        public delegate void ResultEventHandler(Strategy strategy);
        public static event ResultEventHandler ResultEvent;

        public delegate void ProgressEventHandler();
        public static event ProgressEventHandler ProgressEvent;

        #endregion

        #region VARIABLES

        private static Strategy _strategy;
        private static Strategy.StrategyParameters _strategyParameterses; 
        private static List<Strategy.StrategyAdditionalParameter> _strategyAdditionalParameters;
        private static List<Tick> _ticks;
 
        #endregion

        #region START BACKTEST
        public static void StartBackTest(Strategy.StrategyParameters parameters, List<Strategy.StrategyAdditionalParameter> additionalParameters, List<Tick> ticks)
        {
            _strategyParameterses = parameters;
            _strategyAdditionalParameters = additionalParameters;
            _ticks = ticks;

            InitializeStrategy();

            _strategy.ProgressEvent += NotifyProgressEvent;

            _strategy.Start(0);

            NotifyResultEvent(_strategy);
        }

        private static void InitializeStrategy()
        {
            _strategy = new StepChange
                {
                    Ticks = _ticks,
                    AdditionalParameters = _strategyAdditionalParameters,
                    Parameters = new Strategy.StrategyParameters(_strategyParameterses.Account
                                                                 , _strategyParameterses.Symbol
                                                                 , _strategyParameterses.Name)
                };
        }
        #endregion

        #region EVENTS

        private static void NotifyResultEvent(Strategy strategy)
        {
            if (ResultEvent != null)
            {
                ResultEvent(strategy);
            }
        }

        private static void NotifyProgressEvent()
        {
            if (ProgressEvent != null)
            {
                ProgressEvent();
            }
        }

        #endregion
    }
}

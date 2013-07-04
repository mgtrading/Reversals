using System.Collections.Generic;

namespace Reversals.Optimization
{
    class NotOptimizationParametersContainer
    {
        public delegate void ChangedEventHandler();
        public event ChangedEventHandler ChangedEvent;
        private static NotOptimizationParametersContainer _instance;
        private List<NotOptimizationParameter> _parameters = new List<NotOptimizationParameter>();

        public static NotOptimizationParametersContainer Instance
        {
            get { return _instance ?? (_instance = new NotOptimizationParametersContainer()); }
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public List<NotOptimizationParameter> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; NotifyEvent(); }
        }

        private void NotifyEvent()
        {
            if (ChangedEvent != null)
            {
                ChangedEvent();
            }
        }
    }
}

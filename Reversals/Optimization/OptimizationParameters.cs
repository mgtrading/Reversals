using System;
using System.Collections.Generic;

namespace Reversals.Optimization
{
    public class OptimizationParameters
    {
        public delegate void ChangedEventHandler();
        public event ChangedEventHandler ChangedEvent; 
        private static OptimizationParameters _instance;
        private List<OptimizationParameter> _parameters = new List<OptimizationParameter>();

        public static OptimizationParameters Instance
        {
            get { return _instance ?? (_instance = new OptimizationParameters()); }
        }

        public void UpdateParameter(string name, object defValue, object minValue, object maxValue, object step)
        {
            
            for(int i=0; i<_parameters.Count; i++)
            {
                if (_parameters[i].Name == name)
                {
                    var updatedParameter = new OptimizationParameter("", name, _parameters[i].Type, Convert.ToDouble(minValue), Convert.ToDouble(maxValue), Convert.ToDouble(defValue), Convert.ToDouble(step));
                    _parameters[i] = updatedParameter;
                }
            }
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public List<OptimizationParameter> Parameters
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

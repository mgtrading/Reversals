namespace Reversals.ParametersNotifier
{
    public class ParametersChangingNotifier
    {
        #region VARIABLES
        public delegate void PropertyChangedEventHandler(string name, object defValue, object minValue, object maxValue, object step);
        public event PropertyChangedEventHandler PropertyChanged;
        private string _parameterName;
        private object _minValue;
        private object _maxValue;
        private object _defaultValue;
        private object _step;
        #endregion

        #region EVENTS
        private void NotifyPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(_parameterName,_defaultValue, _minValue, _maxValue, _step);
            }                
        }
        #endregion

        #region PROPERTIES
        public string ParameterName
        {
            set
            {
                if (value != null && value != _parameterName)
                {
                    _parameterName = value;
                }
            }
        }
        public object MinValue
        {
            set
            {
                if (value != null && value != _minValue)
                {
                    _minValue = value;
                }
            }
        }
        public object MaxValue
        {
            set
            {
                if (value != null && value != _maxValue)
                {
                    _maxValue = value;
                }
            }
        }
        public object DefaultValue
        {
            set
            {
                if (value != null && value != _defaultValue)
                {
                    _defaultValue = value;
                }
            }
        }
        public object Step
        {
            set
            {
                if (value != null && value != _step)
                {
                    _step = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion
    }
}

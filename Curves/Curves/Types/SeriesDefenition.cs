using System.ComponentModel;
using ExpressionEvaluatorNet;

namespace Curves.Types
{
    internal class SeriesDefenition : INotifyPropertyChanged
    {
        private string _expression;
        public string Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;
                OnPropertyChanged("Expression");
            }
        }

        private double _startValue;
        public double StartValue
        {
            get { return _startValue; }
            set
            {
                _startValue = value;
                OnPropertyChanged("StartValue");
            }
        }

        private double _endValue;
        public double EndValue
        {
            get { return _endValue; }
            set
            {
                _endValue = value;
                OnPropertyChanged("EndValue");
            }
        }

        private uint _steps;
        public uint Steps
        {
            get { return _steps; }
            set
            {
                _steps = value;
                OnPropertyChanged("Steps");
            }
        }

        public ExpressionEvaluator GetEvaluator()
        {
            return new ExpressionEvaluator(Expression);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

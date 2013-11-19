using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Curves.Helpers;
using Curves.Types;
using ExpressionEvaluatorNet;
using OxyPlot;
using OxyPlot.Series;

namespace Curves.ViewModels
{
    internal class MainViewModel
    {
        private PlotModel _plotModel;
        public PlotModel PlotModel { get { return _plotModel ?? (_plotModel = new PlotModel()); } }


        private ObservableCollection<SeriesDefenition> _series;
        public ObservableCollection<SeriesDefenition> Series
        {
            get { return _series ?? (_series = new ObservableCollection<SeriesDefenition>()); }
        }

        private ICommand _addNewSeriesCommand;
        public ICommand AddNewSeriesCommand
        {
            get { return _addNewSeriesCommand ?? (_addNewSeriesCommand = new DelegateCommand(AddNewSeries)); }
        }

        private ICommand _removeSeriesCommand;
        public ICommand RemoveSeriesCommand
        {
            get { return _removeSeriesCommand ?? (_removeSeriesCommand = new DelegateCommand<SeriesDefenition>(RemoveSeries)); }
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new DelegateCommand(Refresh)); }
        }

        public MainViewModel()
        {
            Series.CollectionChanged += SeriesCollectionChanged;

            AddNewSeries();
        }

        private void AddNewSeries()
        {
            var newSeries = new SeriesDefenition { Expression = "x^2", StartValue = -10, EndValue = 10, Steps = 100 };
            newSeries.PropertyChanged += (sender, args) =>
                                         {
                                             if (args.PropertyName == "Expression")
                                             {
                                                 PlotModel.Series.Single(c => c.Tag == sender).Title = ((SeriesDefenition)sender).Expression;
                                             }
                                         };
            Series.Add(newSeries);
        }

        private void RemoveSeries(SeriesDefenition seriesDefenition)
        {
            Series.Remove(seriesDefenition);
        }

        private void Refresh()
        {
            UpdateSources();

            PlotModel.RefreshPlot(true);
            PlotModel.Update();
        }

        private void UpdateSources()
        {
            PlotModel.Series.AsParallel().ForAll(series => UpdateSource((LineSeries)series));
        }

        private static void UpdateSource(LineSeries series)
        {
            var defenition = (SeriesDefenition)series.Tag;

            series.Points.Clear();

            ExpressionEvaluator evaluator;
            try
            {
                evaluator = defenition.GetEvaluator();
            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    String.Format("Bad expression \"{0}\". Error: {1}", defenition.Expression, e.Message))));
                return;
            }

            double from = defenition.StartValue;
            double to = defenition.EndValue;
            uint steps = defenition.Steps;
            double step = (to - @from) / steps;
            double currentValue = @from;


            try
            {
                for (int i = 0; i < steps + 1; i++)
                {
                    evaluator.SetVariableValue("x", currentValue);
                    series.Points.Add(new DataPoint(currentValue, evaluator.Execute()));

                    currentValue += step;
                }
            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    String.Format("Exception while evaluate \"{0}\". Error: {1}", defenition.Expression, e.Message))));
                series.Points.Clear();
            }
        }

        private void SeriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                var itemToAdd = notifyCollectionChangedEventArgs.NewItems.Cast<SeriesDefenition>().Single();
                PlotModel.Series.Add(new LineSeries { Title = itemToAdd.Expression, Tag = itemToAdd });
            }
            else if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                var itemToRemove = notifyCollectionChangedEventArgs.OldItems.Cast<SeriesDefenition>().Single();
                var seriesToTemove = PlotModel.Series.Single(c => c.Tag == itemToRemove);

                PlotModel.Series.Remove(seriesToTemove);
            }
        }
    }
}

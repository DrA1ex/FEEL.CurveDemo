﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Curves.Helpers;
using Curves.Types;
using Feel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Curves.ViewModels
{
    internal class MainViewModel
    {
        private ICommand _addNewSeriesCommand;
        private PlotModel _plotModel;

        private ICommand _refreshCommand;

        private ICommand _removeSeriesCommand;


        private ObservableCollection<SeriesDefenition> _series;

        public MainViewModel()
        {
            InitModel();
            Series.CollectionChanged += SeriesCollectionChanged;

            AddNewSeries();
        }

        public PlotModel PlotModel
        {
            get { return _plotModel ?? (_plotModel = new PlotModel()); }
        }

        public ObservableCollection<SeriesDefenition> Series
        {
            get { return _series ?? (_series = new ObservableCollection<SeriesDefenition>()); }
        }

        public ICommand AddNewSeriesCommand
        {
            get { return _addNewSeriesCommand ?? (_addNewSeriesCommand = new DelegateCommand(AddNewSeries)); }
        }

        public ICommand RemoveSeriesCommand
        {
            get { return _removeSeriesCommand ?? (_removeSeriesCommand = new DelegateCommand<SeriesDefenition>(RemoveSeries)); }
        }

        public ICommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new DelegateCommand(Refresh)); }
        }

        private void InitModel()
        {
            var yAxis = new LinearAxis {MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot};
            PlotModel.Axes.Add(yAxis);
            var xAxis = new LinearAxis {MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Position = AxisPosition.Bottom};
            PlotModel.Axes.Add(xAxis);
        }

        private void AddNewSeries()
        {
            var newSeries = new SeriesDefenition {Expression = "x^2", StartValue = -10, EndValue = 10, Steps = 100};
            newSeries.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "Expression")
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

            PlotModel.InvalidatePlot(true);
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
            catch(Exception e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    string.Format("Bad expression \"{0}\". Error: {1}", defenition.Expression, e.Message))));
                return;
            }

            var from = defenition.StartValue;
            var to = defenition.EndValue;
            var steps = defenition.Steps;
            var step = (to - from) / steps;
            var currentValue = from;


            try
            {
                for(var i = 0; i < steps + 1; i++)
                {
                    evaluator.SetVariableValue("x", currentValue);
                    series.Points.Add(new DataPoint(currentValue, evaluator.Execute()));

                    currentValue += step;
                }
            }
            catch(Exception e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    string.Format("Exception while evaluate \"{0}\". Error: {1}", defenition.Expression, e.Message))));
                series.Points.Clear();
            }
        }

        private void SeriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if(notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                var itemToAdd = notifyCollectionChangedEventArgs.NewItems.Cast<SeriesDefenition>().Single();
                PlotModel.Series.Add(new LineSeries {Title = itemToAdd.Expression, Tag = itemToAdd});
            }
            else if(notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                var itemToRemove = notifyCollectionChangedEventArgs.OldItems.Cast<SeriesDefenition>().Single();
                var seriesToTemove = PlotModel.Series.Single(c => c.Tag == itemToRemove);

                PlotModel.Series.Remove(seriesToTemove);
            }
        }
    }
}
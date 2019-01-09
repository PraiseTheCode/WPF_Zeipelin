namespace WPF_Zeipelin
{
    using System;

    using OxyPlot;
    using OxyPlot.Series;
    using System.Diagnostics;

    public class MainViewModel
    {
        public MainViewModel()
        {
            this.MyModel = new PlotModel { Title = "" };
            
            
            LineSeries ls1 = new LineSeries();
            ls1.Points.Add(new DataPoint(4510, 1));
            this.MyModel.Series.Add(ls1);
           
        }

        public PlotModel MyModel { get; private set; }
    }
}
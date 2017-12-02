using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;

using Sh8ps.Models;
using Sh8ps.Services;

namespace Sh8ps.ViewModels
{
    public class ChartViewModel : ViewModelBase
    {
        public ChartViewModel()
        {
        }

        public ObservableCollection<DataPoint> Source
        {
            get
            {
                // TODO WTS: Replace this with your actual data
                return SampleDataService.GetChartSampleData();
            }
        }
    }
}

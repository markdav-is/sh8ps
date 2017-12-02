using System;

using Sh8ps.ViewModels;

using Windows.UI.Xaml.Controls;

namespace Sh8ps.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        public MainPage()
        {
            InitializeComponent();
        }
    }
}

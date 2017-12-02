using System;

using Sh8ps.ViewModels;

using Windows.UI.Xaml.Controls;

namespace Sh8ps.Views
{
    public sealed partial class BlankPage : Page
    {
        private BlankViewModel ViewModel
        {
            get { return DataContext as BlankViewModel; }
        }

        public BlankPage()
        {
            InitializeComponent();
        }
    }
}

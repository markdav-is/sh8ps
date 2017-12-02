using System;

using Sh8ps.ViewModels;

using Windows.UI.Xaml.Controls;

namespace Sh8ps.Views
{
    public sealed partial class ShapePage : Page
    {
        private ShapeViewModel ViewModel
        {
            get { return DataContext as ShapeViewModel; }
        }

        public ShapePage()
        {
            InitializeComponent();
        }
    }
}

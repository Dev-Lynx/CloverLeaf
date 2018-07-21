using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using CloverLeaf.Desktop.ViewModels;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloverLeaf.Desktop.Views
{
    /// <summary>
    /// Interaction logic for StartView.xaml
    /// </summary>
    public partial class StartView : UserControl, IView
    {
        public StartView()
        {
            InitializeComponent();

            if (DataContext is IViewAccessable)
                ((IViewAccessable)DataContext).View = this;
        }
    }
}

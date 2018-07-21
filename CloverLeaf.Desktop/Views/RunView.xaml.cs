using CloverLeaf.Common.Infrastructure.Services.Interfaces;
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
    /// Interaction logic for RunView.xaml
    /// </summary>
    public partial class RunView : UserControl, IView
    {
        public RunView()
        {
            InitializeComponent();

            if (DataContext is IViewAccessable)
                ((IViewAccessable)DataContext).View = this;
        }
    }
}

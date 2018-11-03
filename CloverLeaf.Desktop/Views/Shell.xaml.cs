using CloverLeaf.Common.Infrastructure;
using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Regions;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell : Window, IShell
    {
        #region Properties
        IRegionManager RegionManager { get; }
        IUnityContainer Container { get; }
        #endregion

        public Shell(IRegionManager regionManager, IUnityContainer container)
        {
            InitializeComponent();
            RegionManager = regionManager;
            Container = container;
            
            RoutedEventHandler loaded = null;
            this.Loaded += loaded = (s, e) =>
            {
                Container.RegisterInstance<IShell>(this);
                DataContext = Container.Resolve<ViewModels.ShellViewModel>();

                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.StartView>(), Core.START_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.AboutView>(), Core.ABOUT_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.ContestDimensionsView>(), Core.CONTEST_DIMENSIONS_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.HomeView>(), Core.HOME_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.RunView>(), Core.RUN_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.DivisionsView>(), Core.DIVISIONS_VIEW);
            };
        }
    }
}

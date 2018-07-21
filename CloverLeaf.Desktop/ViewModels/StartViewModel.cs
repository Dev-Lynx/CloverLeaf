using CloverLeaf.Common.Infrastructure;
using CloverLeaf.Common.Infrastructure.Extensions;
using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CloverLeaf.Desktop.ViewModels
{
    public class StartViewModel : BindableBase, INavigationAware, IViewAccessable
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        #endregion

        #region Internals
        IView view;
        public IView View
        {
            get => view;
            set
            {
                view = value;
                if (view != null) Timer.Start();
            }
        }
        DispatcherTimer Timer { get; } = new DispatcherTimer();
        #endregion

        #endregion

        #region Constructors
        public StartViewModel(ILoggerFacade logger, IRegionManager regionManager)
        {
            Logger = logger;
            RegionManager = regionManager;
            Timer.Interval = TimeSpan.FromSeconds(3);

            Timer.Tick += async (s, e) =>
            {
                Timer.Stop();
                await ((UserControl)View).FadeOut();
                RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.CONTEST_DIMENSIONS_VIEW);
                ((UserControl)View).Opacity = 1;
            };
        }
        #endregion

        #region Methods

        #region INavigationAware Implementation

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        
        public void OnNavigatedTo(NavigationContext navigationContext) { Timer.Start(); }
        #endregion

        #endregion
    }
}

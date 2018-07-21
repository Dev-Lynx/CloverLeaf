using CloverLeaf.Common.Infrastructure;
using CloverLeaf.Common.Infrastructure.Extensions;
using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CloverLeaf.Desktop.ViewModels
{
    public class ContestDimensionsViewModel : BindableBase, IViewAccessable
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public ICoordinator Coordinator { get; }
        #endregion

        #region Commands
        public ICommand NextCommand { get; }
        #endregion

        #region Internals
        public IView View { get; set; }
        #endregion

        #endregion

        #region Construuctors
        public ContestDimensionsViewModel(ILoggerFacade logger, IRegionManager regionManager, ICoordinator coordinator)
        {
            Logger = logger;
            RegionManager = regionManager;
            Coordinator = coordinator;

            NextCommand = new DelegateCommand(OnNext);
        }
        #endregion

        #region Methods

        #region Command Handlers

        async void OnNext()
        {
            await ((FrameworkElement)View).FadeOut(4);
            RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.HOME_VIEW);
            ((FrameworkElement)View).Opacity = 1;
        }   

        #endregion

        #endregion
    }
}

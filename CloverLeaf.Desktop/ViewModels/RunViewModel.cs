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
using System.Windows.Controls;
using System.Windows.Input;

namespace CloverLeaf.Desktop.ViewModels
{
    public class RunViewModel : BindableBase, IViewAccessable
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public IDatabaseManager DatabaseManager { get; }
        public ICoordinator Coordinator { get; }
        #endregion

        #region Commands
        public ICommand GenerateDivisionsCommand { get; }
        public ICommand PreviousRoundCommand { get; }
        public ICommand NextRoundCommand { get; }
        public ICommand BackCommand { get; }
        #endregion

        #region Bindables
        public bool CanGenerateDivisions => false;
        #endregion

        #region Internals
        public IView View { get; set; }
        #endregion

        #endregion

        #region Constructors
        public RunViewModel(ILoggerFacade logger, IRegionManager regionManager, IDatabaseManager databaseManager, ICoordinator coordinator)
        {
            Logger = logger;
            RegionManager = regionManager;
            DatabaseManager = databaseManager;
            Coordinator = coordinator;

            BackCommand = new DelegateCommand(OnBack);
            PreviousRoundCommand = new DelegateCommand(OnNavigateToPreviousRound);
            NextRoundCommand = new DelegateCommand(OnNavigateToNextRound);
            GenerateDivisionsCommand = new DelegateCommand(OnGenerateDivisions);
        }
        #endregion

         #region Methods

        #region Command Handlers
        async void OnBack()
        {
            await ((FrameworkElement)View).FadeOut(3);
            RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.HOME_VIEW);
            ((FrameworkElement)View).Opacity = 1;
        }

        void OnNavigateToPreviousRound()
        {
            Coordinator.NavigateToPreviousRound();
        }

        void OnNavigateToNextRound()
        {
            Coordinator.NavigateToNextRound();
        }

        async void OnGenerateDivisions()
        {
            Coordinator.GenerateDivisions();
            var view = (FrameworkElement)RegionManager.Regions[Core.MAIN_REGION].GetView(Core.DIVISIONS_VIEW);
            view.Opacity = 0;
            RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.DIVISIONS_VIEW);
            await view.FadeIn(3);
        }
        #endregion

        #endregion
    }
}

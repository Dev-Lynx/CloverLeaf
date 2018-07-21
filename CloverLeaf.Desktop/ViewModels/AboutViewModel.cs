using CloverLeaf.Common.Infrastructure;
using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CloverLeaf.Desktop.ViewModels
{
    public class AboutViewModel : BindableBase
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        IDatabaseManager DatabaseManager { get; }
        #endregion

        #region Commands
        public ICommand ClearNamesCommand { get; }
        public ICommand VisitMeCommand { get; }
        public ICommand VisitLoveCommand { get; }
        #endregion

        #endregion

        #region Constructors
        public AboutViewModel(ILoggerFacade logger, IRegionManager regionManager, IDatabaseManager databaseManager)
        {
            Logger = logger;
            RegionManager = regionManager;
            DatabaseManager = databaseManager;

            ClearNamesCommand = new DelegateCommand(OnClearNames);
            VisitMeCommand = new DelegateCommand(OnVisitMe);
            VisitLoveCommand = new DelegateCommand(OnVisitLove);
        }
        #endregion

        #region Methods
        #region Command Handlers
        void OnClearNames()
        {
            var result = MessageBox.Show("Thread carefully, brave men, women and horses will be forgotten.", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Hand);

            if (result == MessageBoxResult.Yes)
                DatabaseManager.ClearDatabase();
        }

        void OnVisitMe() => Process.Start(new ProcessStartInfo(Core.COMPANY_PAGE));
        void OnVisitLove() => Process.Start(new ProcessStartInfo("https://soundcloud.com"));
        #endregion
        #endregion
    }
}

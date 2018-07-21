using CloverLeaf.Common.Infrastructure;
using CloverLeaf.Common.Infrastructure.Extensions;
using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CloverLeaf.Desktop.ViewModels
{
    public class DivisionsViewModel : BindableBase, IViewAccessable
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public ICoordinator Coordinator { get; }
        public IDatabaseManager DatabaseManager { get; }
        #endregion

        #region Bindables
        public ObservableCollection<string> DisplayOptions { get; } = new ObservableCollection<string>()
        {
            "All", "First"
        };

        public string _displayOption = "All";
        public string DisplayOption
        {
            get => _displayOption;
            set
            {
                _displayOption = value;

                if (_displayOption == "All")
                {
                    DisplayCount = Coordinator.Contest.DisplayCount;
                    Coordinator.Contest.DisplayCount = -1;
                }
                else Coordinator.Contest.DisplayCount = DisplayCount;

                RaisePropertyChanged("DisplayOption");
                RaisePropertyChanged("CanControlDisplayCount");
            }
        }

        public int DisplayCount { get; private set; } = 10;
        public bool CanControlDisplayCount => DisplayOption.ToLower() != "All".ToLower();
        #endregion

        #region Commands
        public ICommand BackCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand DoneCommand { get; }
        #endregion

        #region Internals
        public IView View { get; set; }
        #endregion

        #endregion

        #region Constructors
        public DivisionsViewModel(ILoggerFacade logger, IRegionManager regionManager, ICoordinator coordinator, IDatabaseManager databaseManager)
        {
            Logger = logger;
            RegionManager = regionManager;
            DatabaseManager = databaseManager;
            Coordinator = coordinator;

            BackCommand = new DelegateCommand(OnBack);
            ExportCommand = new DelegateCommand(OnExport);
            DoneCommand = new DelegateCommand(OnDone);
        }
        #endregion

        #region Methods

        #region Command Handlers
        async void OnBack()
        {
            if (View == null) return;
            await ((FrameworkElement)View).FadeOut(3);
            RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.RUN_VIEW);
            ((FrameworkElement)View).Opacity = 1;
        }

        void OnExport()
        {
            if (DialogHelper.SaveFile("Text File", ".txt", out string path))
                Coordinator.ExportDivisions(path);
        }

        void OnDone()
        {
            var result = MessageBox.Show("This will start this application from the beginning and all you progress will be lost", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (result == MessageBoxResult.Yes)
            {
                Coordinator.Reset();
                RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.START_VIEW);
            }
        }
        #endregion

        #endregion
    }
}

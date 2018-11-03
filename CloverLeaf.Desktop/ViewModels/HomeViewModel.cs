using CloverLeaf.Common.Infrastructure;
using CloverLeaf.Common.Infrastructure.Extensions;
using CloverLeaf.Common.Infrastructure.Models;
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
    public class HomeViewModel : BindableBase, IViewAccessable, INavigationAware
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public IDatabaseManager DatabaseManager { get; }
        public ICoordinator Coordinator { get; }
        #endregion
        
        #region Commands
        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand StartCommand { get; }
        #endregion

        #region Bindables
        public Rider Rider { get; set; } = new Rider();
        public Horse Horse { get; set; } = new Horse();

        string riderName = "";
        public string RiderName
        {
            get => riderName;
            set
            {
                riderName = value;
                RaisePropertyChanged("CanAdd");
                RaisePropertyChanged("RiderName");
            }
        }

        string horseName = "";
        public string HorseName
        {
            get => horseName;
            set
            {
                horseName = value;
                RaisePropertyChanged("CanAdd");
                RaisePropertyChanged("HorseName");
            }
        }

        public bool CanAdd => !string.IsNullOrWhiteSpace(RiderName) && !string.IsNullOrWhiteSpace(HorseName);
        public bool CanStart => Coordinator.Contest.Teams.Count >= 2;
        #endregion

        #region Internals
        public IView View { get; set; }
        #endregion

        #endregion

        #region Constructors
        public HomeViewModel(ILoggerFacade logger, IRegionManager regionManager, IDatabaseManager databaseManager, ICoordinator coordinator)
        {
            Logger = logger;
            RegionManager = regionManager;
            DatabaseManager = databaseManager;
            Coordinator = coordinator;

            AddCommand = new DelegateCommand(OnAdd);
            StartCommand = new DelegateCommand(OnStart);
            RemoveCommand = new DelegateCommand<object>(OnRemove);

            UIHelper.LoadComplete += (s, e) =>
            {
                RaisePropertyChanged("CanStart");
            };
        }
        #endregion

        #region Methods

        #region INavigationAware Implementation
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext) { RaisePropertyChanged("CanStart"); }
        #endregion

        #region Command Handlers

        void OnAdd()
        {
            Team team = new Team()
            {
                Rider = new Rider() { Name = RiderName },
                Horse = new Horse() { Name = HorseName },
            };

            DatabaseManager.RegisterRider(team.Rider);
            DatabaseManager.RegisterHorse(team.Horse);
            Coordinator.AddTeam(team);

            RiderName = HorseName = string.Empty;

            RaisePropertyChanged("Rider");
            RaisePropertyChanged("Horse");
            RaisePropertyChanged("CanStart");
        }

        async void OnStart()
        {
            Coordinator.GenerateTeams(true);
            await ((FrameworkElement)View).FadeOut(3);
            RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.RUN_VIEW);
            ((FrameworkElement)View).Opacity = 1;
        }

        async void OnRemove(object obj)
        {
            if (!(obj is Grid)) return;

            var container = (Grid)obj;
            var team = (Team) container.DataContext;

            await container.FadeOut(2);
            Coordinator.RemoveTeam(team);
            container.Opacity = 1;
            RaisePropertyChanged("CanStart");
        }
        #endregion

        #endregion
    }
}

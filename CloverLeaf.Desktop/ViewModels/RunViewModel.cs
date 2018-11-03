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
        public ICommand AddTeamCommand { get; }
        public ICommand RemoveTeamCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand RunTimeChangedCommand { get; }
        #endregion

        #region Bindables
        public bool CanGenerateDivisions => false;

        bool _regActive = false;
        public bool RegActive { get => _regActive; set => SetProperty(ref _regActive, value); }

        public bool CanAdd => !string.IsNullOrWhiteSpace(RiderName) && !string.IsNullOrWhiteSpace(HorseName);

        string _riderName = "";
        public string RiderName
        {
            get => _riderName;
            set
            {
                SetProperty(ref _riderName, value);
                RaisePropertyChanged("CanAdd");
            }
        }

        string _horseName = "";
        public string HorseName
        {
            get => _horseName;
            set
            {
                SetProperty(ref _horseName, value);
                RaisePropertyChanged("CanAdd");
            }
        }


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
            AddTeamCommand = new DelegateCommand(OnAddTeam);
            RemoveTeamCommand = new DelegateCommand<object>(OnRemoveTeam);
            AddCommand = new DelegateCommand(OnAdd);
            RunTimeChangedCommand = new DelegateCommand(OnRunTimeChanged);
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

        void OnAdd()
        {
            RegActive = !RegActive;
            RiderName = string.Empty;
            HorseName = string.Empty;
        }

        void OnAddTeam()
        {
            Team team = new Team()
            {
                Rider = new Rider() { Name = RiderName.Trim() },
                Horse = new Horse() { Name = HorseName.Trim() }
            };

            DatabaseManager.RegisterRider(team.Rider);
            DatabaseManager.RegisterHorse(team.Horse);
            Coordinator.AddTeam(team);

            OnAdd();
            Coordinator.GenerateTeams();
        }

        async void OnRemoveTeam(object obj)
        {
            if (!(obj is Grid)) return;

            var container = (Grid)obj;
            var team = (Team)container.DataContext;

            await container.FadeOut(2);
            Coordinator.RemoveTeam(team);
            Coordinator.GenerateTeams();
            container.Opacity = 1;
        }

        void OnRunTimeChanged() => Coordinator.CanSave = true;
        #endregion

        #endregion
    }
}

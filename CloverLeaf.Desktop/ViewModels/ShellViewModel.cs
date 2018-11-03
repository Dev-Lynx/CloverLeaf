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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml.Serialization;

namespace CloverLeaf.Desktop.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public ICoordinator Coordinator { get; }
        IShell Shell { get; }
        #endregion

        #region Commands
        public ICommand MoveCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ImageDoubleCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand LoadContestCommand { get; }
        public ICommand SaveContestCommand { get; }
        public ICommand SaveContestAsCommand { get; }
        public ICommand StartOverCommand { get; }
        #endregion

        #region Bindables
        public bool MenuActive => MenuViews.Contains(SplitCaps(CurrentView));
        //bool _menuOpen = false;
        //public bool MenuOpen { get => _menuOpen; private set => SetProperty(ref _menuOpen, value); }
        public bool MenuOpen { get; set; }
        #endregion

        #region Internals
        string CurrentView
        {
            get
            {
                try { return RegionManager.Regions[Core.MAIN_REGION].NavigationService.Journal.CurrentEntry.Uri.ToString(); }
                catch (Exception e) { Logger.Error(e); }
                return string.Empty;
            }
        }

        string[] MenuViews { get; } = new string[] { Core.HOME_VIEW, Core.RUN_VIEW, Core.DIVISIONS_VIEW };
        public string PreviousView { get; set; }
        public bool CanNavigateToAbout { get; set; } = true;
        #endregion

        #endregion

        #region Constructors
        public ShellViewModel(ILoggerFacade logger, ICoordinator coordinator, IRegionManager regionManager, IShell shell)
        {
            Logger = logger;
            RegionManager = regionManager;
            Coordinator = coordinator;
            Shell = shell;

            MoveCommand = new DelegateCommand(OnMoveWindow);
            MinimizeCommand = new DelegateCommand(OnMinimize);
            CloseCommand = new DelegateCommand(OnClose);
            ImageDoubleCommand = new DelegateCommand(OnNavigateToAbout);
            OpenMenuCommand = new DelegateCommand<object>(OnOpenMenu);
            LoadContestCommand = new DelegateCommand(OnLoadContest);
            SaveContestCommand = new DelegateCommand(OnSaveContest);
            SaveContestAsCommand = new DelegateCommand(OnSaveContestAs);
            StartOverCommand = new DelegateCommand(OnStartOver);

            UIHelper.ViewChanged += (s, e) => RaisePropertyChanged("MenuActive");

            Core.DisplayRequested += (s, e) =>
            {
                Shell.Activate();
                Shell.WindowState = WindowState.Normal;
            };

            if (Core.Load) Load(Core.LoadPath);
            Core.LoadRequested += (s, e) => Load(e);

        }
        #endregion

        #region Methods

        #region Command Handlers
        async void OnNavigateToAbout()
        {
            if (!CanNavigateToAbout) return;

            CanNavigateToAbout = false;

            var current = CurrentView;

            if (string.IsNullOrWhiteSpace(current))
            {
                CanNavigateToAbout = true;
                return;
            }

            string temp = SplitCaps(current);
            string original = current;
            current = temp;

            string destination = Core.ABOUT_VIEW;


            if (!string.IsNullOrWhiteSpace(PreviousView))
                destination = PreviousView;

            var view = (FrameworkElement)RegionManager.Regions[Core.MAIN_REGION].GetView(destination);

            if (destination == Core.ABOUT_VIEW)
            {
                view.Opacity = 0;
                RegionManager.RequestNavigateToView(Core.MAIN_REGION, destination);
                await view.FadeIn(1);
                view.Opacity = 1;
            }
            else
            {
                var about = (FrameworkElement)RegionManager.Regions[Core.MAIN_REGION].GetView(Core.ABOUT_VIEW);
                about.Opacity = 1;
                await about.FadeOut(3);
                about.Opacity = 0;
                RegionManager.RequestNavigateToView(Core.MAIN_REGION, destination);
            }


            PreviousView = current;
            CanNavigateToAbout = true;
        }

        void OnOpenMenu(object obj)
        {
            if (!(obj is RoutedEventArgs)) return;
            var e = (RoutedEventArgs)obj;

            if (!(e.OriginalSource is Button)) return;

            var button = (Button)e.OriginalSource;
            button.ContextMenu.VerticalOffset = -3;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
            button.ContextMenu.Placement = PlacementMode.Right;
            MenuOpen = true;
            RaisePropertyChanged("MenuOpen");
            /*
            if (!(obj is ContextMenu)) return;

            var menu = (ContextMenu)obj;
            menu.IsOpen = true;
            Logger.Debug("{0} ", menu.Parent);
            menu.PlacementTarget = UIHelper.FindParent<Button>(menu);
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
            */
            //menu.IsSubmenuOpen = true;
            /*
            //MenuOpen = !MenuOpen;
            if (!menu.IsSubmenuOpen)
            {
                menu.IsSubmenuOpen = true;
                //menu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            }
            */
        }

        void OnMoveWindow()
        {
            try { Shell.DragMove(); }
            catch { }
        }



        void OnMinimize() => Shell.WindowState = WindowState.Minimized;
        void OnClose() => Shell.Close();

        void OnLoadContest()
        {
            if (!MenuActive) return;
            if (DialogHelper.GetFile(Core.CONTEST_FILE_NAME, Core.CONTEST_EXTENSION_FILE, out string path))
                Load(path);
        }

        void OnSaveContest()
        {
            if (!MenuActive || !Coordinator.CanSave) return;
            Coordinator.SaveContest(Coordinator.SavePath);
        }

        void OnSaveContestAs()
        {
            if (!MenuActive) return;
            if (DialogHelper.SaveFile(out string savePath, $"{Core.CONTEST_FILE_NAME},{Core.CONTEST_EXTENSION_FILE}"))
                Coordinator.SaveContest(savePath);
        }

        void OnStartOver()
        {
            if (!MenuActive) return;
            var result = MessageBox.Show("This will start this application from the beginning and all you progress will be lost", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (result == MessageBoxResult.Yes)
            {
                Coordinator.Reset();
                RegionManager.RequestNavigateToView(Core.MAIN_REGION, Core.START_VIEW);
            }
        }
        #endregion

        void Load(string path)
        {
            bool success = false;
            success = Coordinator.LoadContest(path);

            if (!success) return;
            switch (SplitCaps(CurrentView))
            {
                case Core.HOME_VIEW:
                    UIHelper.LoadInitiated();
                    break;

                case Core.RUN_VIEW:
                    Coordinator.GenerateTeams();
                    break;

                case Core.DIVISIONS_VIEW:
                    Coordinator.GenerateTeams();
                    Coordinator.GenerateDivisions();
                    break;
            }
        }

        #region Utilities
        string SplitCaps(string value)
        {
            string temp = string.Empty;
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && i != 0) temp += " ";
                temp += value[i];
            }
            return temp;
        }
        #endregion

        #endregion
    }
}

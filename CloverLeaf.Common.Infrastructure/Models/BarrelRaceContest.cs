using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Models
{
    public class BarrelRaceContest : BindableBase
    {
        #region Properties
        public ObservableCollection<Team> Teams { get; } = new ObservableCollection<Team>();
        public ObservableCollection<ObservableCollection<Team>> Rounds { get; } = new ObservableCollection<ObservableCollection<Team>>();
        public ObservableCollection<Team> GeneratedTeams { get; private set; } = new ObservableCollection<Team>();
        public ObservableCollection<Division> Divisions { get; } = new ObservableCollection<Division>();
        public ObservableCollection<Division> DisplayDivisions { get; private set; } = new ObservableCollection<Division>();

        int _displayCount = -1;
        public int DisplayCount
        {
            get => _displayCount;
            set
            {
                _displayCount = value;
                OrganizeDisplay();
                RaisePropertyChanged("DisplayDivisions");
                RaisePropertyChanged("DisplayCount");
            }
        }

        public int TotalRounds { get; set; } = 1;

        int _currentRound = 1;
        public int CurrentRound
        {
            get => _currentRound;
            set
            {
                _currentRound = value;
                GeneratedTeams = Rounds[value - 1];
                RaisePropertyChanged("GeneratedTeams");
                RaisePropertyChanged("CurrentRound");
                RaisePropertyChanged("CanNavigateToPreviousRound");
                RaisePropertyChanged("CanNavigateToNextRound");
            }
        }

        public bool CanNavigateToPreviousRound => CurrentRound > 1;
        public bool CanNavigateToNextRound => CurrentRound < TotalRounds;

        #endregion

        #region Methods
        public void AddTeam(Team team)
        {
            Teams.Add(team);
            RaisePropertyChanged("Teams");
            RaisePropertyChanged("GeneratedTeams");
        }

        public void RemoveTeam(Team team)
        {
            try
            {
                Teams.Remove(team);
                foreach (var round in Rounds)
                    round.Remove(team);
            }
            catch { }
            RaisePropertyChanged("Teams");
            RaisePropertyChanged("GeneratedTeams");
        }

        public void OrganizeDisplay()
        {
            DisplayDivisions = new ObservableCollection<Division>();

            for (int i = 0; i < Divisions.Count; i++)
            {
                var division = new Division(i + 1);
                division.Teams = new ObservableCollection<Team>(Divisions[i].Teams.ToList());
                DisplayDivisions.Add(division);
            }

            if (DisplayCount != -1)
            {
                for (int i = 0; i < Divisions.Count; i++)
                {
                    DisplayDivisions[i].Teams.Clear();
                    for (int j = 0; j < DisplayCount && j < Divisions[i].Teams.Count; j++)
                        DisplayDivisions[i].Teams.Add(Divisions[i].Teams[j]);
                }
            }
            RaisePropertyChanged("DisplayDivisions");
        }
        #endregion
    }
}

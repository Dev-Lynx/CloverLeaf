using CloverLeaf.Common.Infrastructure.Extensions;
using CloverLeaf.Common.Infrastructure.Models;
using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using Prism.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CloverLeaf.Common.Infrastructure.Services
{
    public class Coordinator : BindableBase, ICoordinator
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IDatabaseManager DatabaseManager { get; }
        #endregion

        #region Bindables
        public BarrelRaceContest Contest { get; private set; } = new BarrelRaceContest();
        #endregion

        #region Internals
        Random Randomizer { get; } = new Random();
        #endregion

        #endregion

        #region Constructors
        public Coordinator(ILoggerFacade logger, IDatabaseManager databaseManager)
        {
            Logger = logger;
            DatabaseManager = databaseManager;
        }
        #endregion

        #region Methods

        #region ICoordinator Implementation
        public void AddTeam(Team team)
        {
            var teams = Contest.Teams;
            for (int i = 0; i < teams.Count; i++)
                if (team.Horse == teams[i].Horse)
                {
                    MessageBox.Show($"{team.Horse} has already been taken by {teams[i].Rider}. Try another horse.");
                    //MessageBox.Show("Horses are not allowed to run more than once.");
                    return;
                }
                else if (team.Rider == teams[i].Rider)
                {
                    var difference = teams.Count - i;

                    if (difference < 5)
                    {
                        MessageBox.Show($"There needs to be at least {5 - difference} more registerations before {team.Rider} can be registered again.");
                        return;
                    }
                }
            Contest.Teams.Add(team);
        }

        public void RemoveTeam(Team team)
        {
            try
            {
                Contest.Teams.Remove(team);
            }
            catch (Exception e) { Logger.Error(e); }
        }

        public void Reset()
        {
            Contest = new BarrelRaceContest();
            RaisePropertyChanged("Contest");
        }

        public void NavigateToNextRound()
        {
            if (!Contest.CanNavigateToNextRound) return;
            Contest.CurrentRound++;
        }

        public void NavigateToPreviousRound()
        {
            if (!Contest.CanNavigateToPreviousRound) return;
            Contest.CurrentRound--;
        }

        public void GenerateTeams()
        {
            while (Contest.Rounds.Count < Contest.TotalRounds)
                Contest.Rounds.Add(new ObservableCollection<Team>());
            while (Contest.Rounds.Count > Contest.TotalRounds)
                Contest.Rounds.RemoveAt(Contest.Rounds.Count - 1);

            for (int i = 0; i < Contest.TotalRounds; i++)
                for (int j = 0; j < Contest.Teams.Count; j++)
                    if (!Contest.Rounds[i].Contains(Contest.Teams[j]))
                        Contest.Rounds[i].Add(new Team(Contest.Teams[j]));
            Contest.CurrentRound = Contest.CurrentRound;
            /*
            //Contest.GeneratedTeams.Clear();
            // TODO: Make Randomization possible
            for (int i = 0; i < Contest.Teams.Count; i++)
            {
                if (!Contest.GeneratedTeams.Contains(Contest.Teams[i]))
                    Contest.GeneratedTeams.Add(Contest.Teams[i]);
            }
            */
            RaisePropertyChanged("Contest");
        }

        public void GenerateDivisions()
        {
            double fastest = double.MaxValue;
            var teams = new List<Team>();

            Team currentTeam = new Team();
            for (int i = 0; i < Contest.Teams.Count; i++)
            {
                for (int j = 0; j < Contest.Rounds.Count; j++)
                {
                    currentTeam = Contest.Rounds[j][i];
                    var runTime = currentTeam.RunTime;
                    if (runTime < fastest)
                        fastest = runTime;
                }
                teams.Add(new Team() { Rider = currentTeam.Rider, Horse = currentTeam.Horse, RunTime = fastest });
                fastest = double.MaxValue;
            }

            /*
            for (int i = 0; i < Contest.Rounds.Count; i++)
            {
                fastest = double.MaxValue;
                Team currentTeam = new Team();
                for (int j = 0; j < Contest.Rounds[i].Count; j++)
                {
                    currentTeam = Contest.Rounds[i][j];
                    var runTime = currentTeam.RunTime;
                    if (runTime < fastest)
                        fastest = runTime; 
                }
                teams.Add(new Team() { Rider = currentTeam.Rider, Horse = currentTeam.Horse, RunTime = fastest });
            }
                */

            // Create Divisions
            Contest.Divisions.Clear();
            for (int i = 1; i <= 4; i++) Contest.Divisions.Add(new Division(i));

            

            // Find the fastest runtime.
            for (int i = 0; i < teams.Count; i++)
                if (teams[i].RunTime < fastest)
                    fastest = teams[i].RunTime;

            for (int i = 0; i < teams.Count; i++)
            {
                var team = teams[i];
                var difference = team.RunTime - fastest;

                // TODO: Question Justin about this
                if (difference < .5) Contest.Divisions[0].Teams.Add(team);
                else if (difference >= .5 && difference < 1.0) Contest.Divisions[1].Teams.Add(team);
                else if (difference >= 1.0 && difference < 2.0) Contest.Divisions[2].Teams.Add(team);
                else Contest.Divisions[3].Teams.Add(team);
            }

            // Sort the divisions
            foreach (var division in Contest.Divisions)
                //division.Teams = new ObservableCollection<Team>(SortTeamsByDescending(division.Teams.ToList()));
                division.Teams = new ObservableCollection<Team>(division.Teams.OrderBy(v => v.RunTime));

            Contest.OrganizeDisplay();
        }

        public void ExportDivisions(string path)
        {
            var divisions = Contest.DisplayDivisions;
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("CloverLeaf Generated Report".ToUpper());
                writer.WriteLine($"Date: {DateTime.Now.ToLongDateString()}".ToUpper());

                writer.WriteLine("Top Fives".ToUpper());
                for (int i = 0; i < divisions.Count; i++)
                    for (int j = 0; j < 5 && j < divisions[i].Teams.Count; j++)
                    {
                        writer.WriteLine();
                        writer.WriteLine();

                        writer.WriteLine($"DIVISION {divisions[i].Index}");
                        writer.WriteLine($"\t{divisions[i].Teams[j].Rider}\t{divisions[i].Teams[j].Horse}\t\t{divisions[i].Teams[j].RunTime}");
                    }
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine();

                for (int i = 0; i < divisions.Count; i++)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine($"DIVISION {divisions[i].Index}");
                    writer.WriteLine();
                    for (int j = 0; j < divisions[i].Teams.Count; j++)
                        writer.WriteLine($"\t{divisions[i].Teams[j].Rider}\t{divisions[i].Teams[j].Horse}\t\t{divisions[i].Teams[j].RunTime}");
                }
            }
        }
        #endregion

        #endregion
    }
}

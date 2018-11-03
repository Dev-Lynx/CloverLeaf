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
using System.Xml.Serialization;

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
        BarrelRaceContest _contest = new BarrelRaceContest();
        public BarrelRaceContest Contest { get => _contest; private set => SetProperty(ref _contest, value); }

        bool _canSave = false;
        public bool CanSave { get => _canSave && Saved; set { _canSave = value; RaisePropertyChanged("CanSave"); } }
        public string SavePath { get; private set; }
        #endregion

        #region Internals
        Random Randomizer { get; } = new Random();
        bool RefreshRequired { get; set; }
        bool Saved
        {
            get
            {
                bool exists = false;
                try { exists = File.Exists(SavePath); }
                catch { return false; }
                return exists;
            }
        }
        //; public bool ContestAccuracyInvalidated { get; set; }
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
            RefreshRequired = true;
            CanSave = true;
            Contest.Teams.Add(team);
        }

        public void RemoveTeam(Team team)
        {
            try
            {
                Contest.Teams.Remove(team);
                RefreshRequired = true;
                CanSave = true;
            }
            catch (Exception e) { Logger.Error(e); }
        }

        public void Reset()
        {
            Contest = new BarrelRaceContest();
            SavePath = string.Empty;
            CanSave = false;
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

        public void GenerateTeams(bool shuffle = false)
        {
            while (Contest.Rounds.Count < Contest.TotalRounds)
                Contest.Rounds.Add(new ObservableCollection<Team>());
            while (Contest.Rounds.Count > Contest.TotalRounds)
                Contest.Rounds.RemoveAt(Contest.Rounds.Count - 1);

            if (RefreshRequired)
            {
                for (int i = 0; i < Contest.Rounds[0].Count; i++)
                    if (!Contest.Teams.Contains(Contest.Rounds[0][i]))
                        for (int j = Contest.Rounds.Count-1; j >= 0; j--)
                            Contest.Rounds[j].Remove(Contest.Rounds[0][i]);

                for (int i = 0; i < Contest.Teams.Count; i++)
                    if (!Contest.Rounds[0].Contains(Contest.Teams[i]))
                        for (int j = 0; j < Contest.Rounds.Count; j++)
                            Contest.Rounds[j].Add(new Team(Contest.Teams[i]));
            }

            // Copy all the displayed information to the round observablecollection
            for (int i = 0; i < Contest.DisplayRounds.Count; i++)
                for (int j = 0; j < Contest.DisplayRounds[i].Count; j++)
                {
                    var team = Contest.DisplayRounds[i][j];
                    var index = Contest.Rounds[i].IndexOf(team);
                    if (index < 0) continue;
                    Contest.Rounds[i][index].RunTime = team.RunTime;
                }

            var displayRounds = new ObservableCollection<ObservableCollection<Team>>();

            foreach (var round in Contest.Rounds)
            {
                displayRounds.Add(new ObservableCollection<Team>());
                
                for (int i = 0; i < round.Count; i++)
                    displayRounds[displayRounds.Count - 1].Add(new Team(round[i]));
            }


            Contest.DisplayRounds = displayRounds;

            if (shuffle && Contest.Randomize)
                for (int i = 1; i < Contest.DisplayRounds.Count; i++)
                    for (int j = 0; j < Contest.DisplayRounds[i].Count; j++)
                        ExchangeValues(Randomizer.Next(Contest.DisplayRounds[i].Count), Randomizer.Next(Contest.DisplayRounds[i].Count), Contest.DisplayRounds[i]);
                

            /*
            for (int i = 0; i < Contest.TotalRounds; i++)
                for (int j = 0; j < Contest.Teams.Count; j++)
                    for (int k = 0; k < Contest.Rounds[i].Count; k++)
                        if (Contest.Teams.Contains())
                            *

            for (int i = 0; i < Contest.TotalRounds; i++)
                for (int j = 0; j < Contest.Teams.Count; j++)
                    if (!Contest.Rounds[i].Contains(Contest.Teams[j]))
                        Contest.Rounds[i].Add(new Team(Contest.Teams[j]));
            */        

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

            // Copy all the displayed information to the round observablecollection
            for (int i = 0; i < Contest.DisplayRounds.Count; i++)
                for (int j = 0; j < Contest.DisplayRounds[i].Count; j++)
                {
                    var team = Contest.DisplayRounds[i][j];
                    var index = Contest.Rounds[i].IndexOf(team);
                    if (index < 0) return;
                    Contest.Rounds[i][index].RunTime = team.RunTime;
                }

            // Get the fastest times for each team
            Team currentTeam = new Team();
            for (int i = 0; i < Contest.Teams.Count; i++)
            {
                for (int j = 0; j < Contest.Rounds.Count; j++)
                {
                    currentTeam = Contest.Rounds[j][i];
                    var runTime = currentTeam.RunTime;
                    if (runTime < fastest && runTime != 0)
                        fastest = runTime;
                }

                if (fastest == double.MaxValue)
                    fastest = 0;
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


                if (team.RunTime == 0)
                {
                    Contest.Divisions[3].Teams.Add(team);
                    continue;
                }
                
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

        public bool LoadContest(string path)
        {
            try
            {
                BarrelRaceContest contest = null;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    contest = (BarrelRaceContest)new XmlSerializer(typeof(BarrelRaceContest)).Deserialize(stream);
                if (contest != null)
                    Contest = contest;
                else
                {
                    MessageBox.Show("Sorry, the provided contest file failed to open.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured while loading a contest.\n{0}", ex);
                MessageBox.Show("Sorry, the provided contest file failed to open.");
                return false;
            }
            SavePath = path;
            RefreshRequired = true;
            return true;
        }

        public bool SaveContest(string path)
        {
            try
            {
                if (Path.GetExtension(path) != Core.CONTEST_EXTENSION_FILE)
                    path += Core.CONTEST_EXTENSION_FILE;

                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    new XmlSerializer(typeof(BarrelRaceContest)).Serialize(stream, Contest);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while attempting to save the file. Please try again in another directory.", "Error!"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error("An error ocurred while attempting to save a contest file. \n{0}", ex);
                return false;
            }
            CanSave = false;
            SavePath = path;
            return true;
        }
        #endregion

        #region Utilities
        bool ExchangeValues<T>(int index1, int index2, IList<T> collection) where T : class
        {
            try
            {
                T value1 = collection[index1];
                T value2 = collection[index2];

                collection[index1] = value2;
                collection[index2] = value1;
            }
            catch { return false; }
            return true;
        }
        #endregion

        #endregion
    }
}

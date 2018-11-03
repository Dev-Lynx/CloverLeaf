using CloverLeaf.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Services.Interfaces
{
    public interface ICoordinator
    {
        BarrelRaceContest Contest { get; }
        bool CanSave { get; set; }
        string SavePath { get; }

        void AddTeam(Team team);
        void RemoveTeam(Team team);
        void Reset();
        void NavigateToNextRound();
        void NavigateToPreviousRound();
        void GenerateTeams(bool shuffle=false);
        void GenerateDivisions();
        void ExportDivisions(string path);
        bool LoadContest(string path);
        bool SaveContest(string path);
    }
}

using CloverLeaf.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Services.Interfaces
{
    public interface IDatabaseManager
    {
        ObservableCollection<Rider> Riders { get; }
        ObservableCollection<Horse> Horses { get; }
        void RegisterRider(Rider rider);
        void RegisterHorse(Horse horse);
        void ClearDatabase();
    }
}

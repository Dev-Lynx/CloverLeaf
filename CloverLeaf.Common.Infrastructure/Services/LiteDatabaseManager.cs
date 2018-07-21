using CloverLeaf.Common.Infrastructure.Extensions;
using CloverLeaf.Common.Infrastructure.Models;
using CloverLeaf.Common.Infrastructure.Services.Interfaces;
using LiteDB;
using Prism.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Services
{
    public class LiteDatabaseManager : BindableBase, IDatabaseManager
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        #endregion

        #region Bindables
        public ObservableCollection<Rider> Riders { get; private set; }
        public ObservableCollection<Horse> Horses { get; private set; }
        #endregion

        #region Internals
        LiteDatabase Database { get; } = new LiteDatabase(Core.DATABASE_PATH);
        #endregion

        #endregion

        #region Constructors
        public LiteDatabaseManager(ILoggerFacade logger)
        {
            Logger = logger;

            LoadRiders();
            LoadHorses();
        }
        #endregion

        #region Methods

        #region IDatabaseManager Implementation
        public void RegisterRider(Rider rider)
        {
            var collection = Database.GetCollection<Rider>(Core.RIDERS_DOC_NAME);

            if (collection.FindOne(r => r == rider) == null)
                collection.Insert(rider);
            else collection.Update(rider);


            LoadRiders();
        }

        public void RegisterHorse(Horse horse)
        {
            var collection = Database.GetCollection<Horse>(Core.HORSES_DOC_NAME);

            if (collection.FindOne(h => h == horse) == null)
                collection.Insert(horse);
            else collection.Update(horse);

            LoadHorses();
        }

        public void ClearDatabase()
        {
            try
            {
                Database.DropCollection(Core.RIDERS_DOC_NAME);
                Database.DropCollection(Core.HORSES_DOC_NAME);
                LoadRiders();
                LoadHorses();
            }
            catch (Exception e) { Logger.Error(e); }
        }
        #endregion

        #region Load Engines
        void LoadRiders()
        {
            var collection = Database.GetCollection<Rider>(Core.RIDERS_DOC_NAME);

            if (collection == null) return;

            Riders = new ObservableCollection<Rider>(collection.FindAll());
            RaisePropertyChanged("Riders");
        }

        void LoadHorses()
        {
            var collection = Database.GetCollection<Horse>(Core.HORSES_DOC_NAME);

            if (collection == null) return;

            Horses = new ObservableCollection<Horse>(collection.FindAll());
            RaisePropertyChanged("Horses");
        }
        #endregion

        #endregion
    }
}

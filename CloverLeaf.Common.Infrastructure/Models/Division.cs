using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Models
{
    public class Division
    {
        #region Properties
        public int Index { get; set; }
        public ObservableCollection<Team> Teams { get; set; } = new ObservableCollection<Team>();
        #endregion

        #region Constructors
        public Division(int index)
        {
            Index = index;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Models
{
    [Serializable]
    public class Horse
    {
        #region Properties
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        #endregion

        #region Constructors
        public Horse() { }
        public Horse(Horse horse) { this.Name = horse.Name.Trim(); }
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareHorses(Horse h1, Horse h2)
        {
            bool nullOne = object.ReferenceEquals(h1, null);
            bool nullTwo = object.ReferenceEquals(h2, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return h1.Name.ToUpper() == h2.Name.ToUpper();
        }
        #endregion

        #region Overrides
        public static bool operator ==(Horse h1, Horse h2) => CompareHorses(h1, h2);
        public static bool operator !=(Horse h1, Horse h2) => !CompareHorses(h1, h2);

        public override bool Equals(object obj)
        {
            if (!(obj is Horse)) return false;
            return CompareHorses(this, (Horse)obj);
        }

        public override int GetHashCode()
        {
            if (!string.IsNullOrWhiteSpace(Name)) return Name.GetHashCode();
            return base.GetHashCode();
        }

        public override string ToString() => Name;
        #endregion

        #endregion
    }
}

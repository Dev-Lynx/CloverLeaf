using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Models
{
    [Serializable]
    public class Rider
    {
        #region Properties
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        #endregion

        #region Constructors
        public Rider() { }
        public Rider(Rider rider) { this.Name = rider.Name.Trim(); }
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareRiders(Rider r1, Rider r2)
        {
            bool nullOne = object.ReferenceEquals(r1, null);
            bool nullTwo = object.ReferenceEquals(r2, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return r1.Name.ToUpper() == r2.Name.ToUpper();
        }
        #endregion

        #region Overrides
        public static bool operator ==(Rider r1, Rider r2) => CompareRiders(r1, r2);
        public static bool operator !=(Rider r1, Rider r2) => !CompareRiders(r1, r2);

        public override bool Equals(object obj)
        {
            if (!(obj is Rider)) return false;
            return CompareRiders(this, (Rider)obj);
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

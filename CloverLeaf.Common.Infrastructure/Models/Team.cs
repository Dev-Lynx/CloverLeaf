using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverLeaf.Common.Infrastructure.Models
{
    public class Team
    {
        #region Properties
        public Rider Rider { get; set; }
        public Horse Horse { get; set; }
        public double RunTime { get; set; }
        #endregion

        #region Constructors
        public Team() { }
        public Team(Team team) { Rider = team.Rider; Horse = team.Horse; RunTime = team.RunTime; }
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareTeams(Team t1, Team t2)
        {
            bool nullOne = object.ReferenceEquals(null, t1);
            bool nullTwo = object.ReferenceEquals(null, t2);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return t1.Rider.Name.ToLower() == t2.Rider.Name.ToLower() 
                && t1.Horse.Name.ToLower() == t2.Horse.Name.ToLower();
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Team)) return false;
            return CompareTeams(this, (Team)obj);
        }

        public override int GetHashCode()
        {
            object[] properties = new object[]
            {
                Rider, Horse, RunTime
            };

            int hash = 17;
            unchecked
            {
                for (int i = 0; i < properties.Length; i++)
                    if (!object.ReferenceEquals(null, properties[i]))
                        hash += 23 * properties[i].GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return $"{Rider} - {Horse}";
        }
        #endregion

        #endregion
    }
}

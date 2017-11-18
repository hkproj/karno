using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karno
{
    public class Group : SortedSet<string>
    {

        public Group(IEnumerable<string> collection) : base(collection)
        {

        }

        public Group() : base()
        {

        }

        public bool? IsEssential { get; set; }

        #region Equality Comparison

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;

            var other = (Group)obj;
            return this.SequenceEqual(other);
        }

        public override int GetHashCode()
        {
            if (Count > 0)
                return this.First().GetHashCode();
            else
                return GetHashCode();
        }

        #endregion

    }

}

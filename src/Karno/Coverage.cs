using System.Collections.Generic;
using System.Linq;

namespace Karno
{
    public class Coverage : HashSet<Group>
    {

        public Coverage(IEnumerable<Group> collection) : base(collection)
        {

        }

        public Coverage() : base()
        {

        }

        public long? Cost { get; set; }

        #region Equality Comparison

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;

            var other = (Coverage)obj;
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

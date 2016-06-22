using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp.Utils.Linq
{
    internal class Signature : IEquatable<Signature>
    {
        #region Fields

        public readonly List<DynamicProperty> _properties;

        private readonly int _hashCode;

        #endregion Fields

        #region Constructors

        public Signature(IEnumerable<DynamicProperty> properties)
        {
            _properties = properties.ToList();
            _hashCode = 0;
            foreach (DynamicProperty p in properties)
            {
                _hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }
        }

        #endregion Constructors

        #region Methods

        public bool Equals(Signature other)
        {
            if (_properties.Count != other._properties.Count) return false;
            bool any = false;
            for (int i = 0; i < _properties.Count; i++)
            {
                DynamicProperty t = _properties[i];
                if (t.Name != other._properties[i].Name || t.Type != other._properties[i].Type)
                {
                    any = true;
                    break;
                }
            }

            return
                !any;
        }

        public override bool Equals(object obj)
        {
            return obj is Signature && Equals((Signature) obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        #endregion Methods
    }
}

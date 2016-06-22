using System;
using System.Runtime.Serialization;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    [DataContract]
    public class Triplet<T1, T2, T3> : Pair<T1, T2>
    {
        #region Constructors and Finalizers

        public Triplet(T1 first, T2 second, T3 third)
            : base(first, second)
        {
            this.Third = third;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        [DataMember]
        public T3 Third { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static Triplet<T1, T2, T3> CreateNew(T1 first, T2 second, T3 third)
        {
            return new Triplet<T1, T2, T3>(first, second, third);
        }

        #endregion Public Methods and Operators
    }

    public class Triplet<T> : Triplet<T, T, T>
    {
        public Triplet(T first, T second, T third):base(first , second, third)
        {

        }
    }
}

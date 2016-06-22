using System;
using System.Runtime.Serialization;

namespace CSharp.Utils.Collections.Generic
{
    [Serializable]
    [DataContract]
    public class Pair<T1, T2>
    {
        #region Constructors and Finalizers

        public Pair(T1 first, T2 second)
        {
            this.First = first;
            this.Second = second;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        [DataMember]
        public T1 First { get; private set; }

        [DataMember]
        public T2 Second { get; private set; }

        #endregion Public Properties

        #region Public Methods and Operators

        public static Pair<T1, T2> CreateNew(T1 first, T2 second)
        {
            return new Pair<T1, T2>(first, second);
        }

        #endregion Public Methods and Operators
    }
}

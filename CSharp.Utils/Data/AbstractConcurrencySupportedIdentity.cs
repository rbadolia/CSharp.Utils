using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using CSharp.Utils.Contracts;
using CSharp.Utils.Data.Auditing;

namespace CSharp.Utils.Data
{
    [Serializable]
    [DataContract]
    public abstract class AbstractConcurrencySupportedIdentity : AbstractIdentity, IConcurrencySupported
    {
        [Timestamp]
        [SkipAuditing]
        [ConcurrencyCheck]
        [Column("RowVersion", Order = int.MaxValue)]
        [DataMember]
        public byte[] RowVersion { get; set; }
    }
}

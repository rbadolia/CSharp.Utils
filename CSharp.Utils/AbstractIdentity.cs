using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using CSharp.Utils.Contracts;
using CSharp.Utils.Data.Auditing;

namespace CSharp.Utils
{
    [Serializable]
    [DataContract]
    public abstract class AbstractIdentity : IIdentity
    {
        protected AbstractIdentity()
        {
            this.Id = Guid.NewGuid();
        }

        [DataMember]
        [Key]
        [SkipAuditing]
        [Column(Order = 0)]
        public virtual Guid Id { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.DirectoryServices
{
    [Serializable]
    [DataContract]
    public class ActiveDirectoryUserInfo
    {
        [DataMember]
        [Display(Order = 0)]
        public string FirstName { get; set; }

        [DataMember]
        [Display(Order = 1)]
        public string MiddleName { get; set; }

        [DataMember]
        [Display(Order = 2)]
        public string LastName { get; set; }

        [DataMember]
        [Display(Order = 3)]
        public string EmailId { get; set; }

        public override string ToString()
        {
            return DynamicToStringHelper<ActiveDirectoryUserInfo>.ExportAsString(this);
        }
    }
}

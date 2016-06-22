using System.Collections.Generic;

namespace CSharp.Utils.Authorization
{
    public class AuthorizationSettings
    {
        public AuthorizationSettings()
        {
            this.Exemption = AuthorizationExemption.None;
            this.RequiredPermissions = new List<int>();
        }

        public AuthorizationExemption Exemption { get; set; }

        public List<int> RequiredPermissions { get; set; } 
    }
}

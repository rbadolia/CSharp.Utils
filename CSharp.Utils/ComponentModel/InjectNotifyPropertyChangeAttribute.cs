using System;

namespace CSharp.Utils.ComponentModel
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
    public class InjectNotifyPropertyChangeAttribute : Attribute
    {
    }
}

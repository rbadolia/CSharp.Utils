using System;

namespace CSharp.Utils.StateManagement
{
    public sealed class StateMachineException : InvalidOperationException
    {
        public StateMachineException(string message) : base(message)
        {

        }

        public StateMachineException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}

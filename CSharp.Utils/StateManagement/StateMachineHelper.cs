using System.Collections.Generic;
using System.Text;
using CSharp.Utils.StateManagement.Configuration;
using CSharp.Utils.Validation;

namespace CSharp.Utils.StateManagement
{
    public static class StateMachineHelper
    {
        internal static Dictionary<string, KeyValuePair<ActionConfiguration, Dictionary<string, string>>> BuildActionsDictionary(
            StateMachineConfiguration configuration)
        {
            Guard.ArgumentNotNull(configuration, "configuration");
            var dictionary = new Dictionary<string, KeyValuePair<ActionConfiguration, Dictionary<string, string>>>();
            foreach (var actionConfiguration in configuration.Actions)
            {
                var innerDictionary = new Dictionary<string, string>();
                foreach (var stateTransition in actionConfiguration.StateTransitions)
                {
                    innerDictionary.Add(stateTransition.FromState, stateTransition.ToState);
                }

                dictionary.Add(actionConfiguration.Action, 
                    new KeyValuePair<ActionConfiguration, Dictionary<string, string>>(actionConfiguration, 
                        innerDictionary));
            }

            return dictionary;
        }

        internal static string GetDotGraph(IDictionary<string, KeyValuePair<ActionConfiguration, Dictionary<string, string>>> dictionary, bool excludeActionsWithNoTransition, string currentState)
        {
            Guard.ArgumentNotNull(dictionary, "dictionary");
            var sb = new StringBuilder();
            sb.AppendLine("digraph {");
            foreach (var kvp in dictionary)
            {
                foreach (var kvp1 in kvp.Value.Value)
                {
                    bool shouldAppend = true;

                    if (excludeActionsWithNoTransition)
                    {
                        if (kvp1.Key == kvp1.Value)
                        {
                            shouldAppend = false;
                        }
                    }

                    if (shouldAppend)
                    {
                        sb.AppendLine(string.Format(" \"{0}\" -> \"{1}\" [label=\"{2}\"];", kvp1.Key, kvp1.Value, kvp.Key));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(currentState))
            {
                sb.AppendLine(string.Format(" \"{0}\" [color=\"{1}\" style=\"filled\"];", currentState, "green"));
            }

            sb.AppendLine("}");
            return sb.ToString();

        }

        public static string GetDotGraph(StateMachineConfiguration configuration, bool excludeActionsWithNoTransition)
        {
            Guard.ArgumentNotNull(configuration, "configuration");
            var dictionary = BuildActionsDictionary(configuration);
            return GetDotGraph(dictionary, excludeActionsWithNoTransition, null);
        }
    }
}

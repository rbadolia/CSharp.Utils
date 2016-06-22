using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using CSharp.Utils.Diagnostics.Performance;
using CSharp.Utils.RequestProcessing;

namespace CSharp.Utils
{
    [Serializable]
    [DataContract]
    [PerfCounterCategory("UseCase Performance Metrics")]
    public class UseCasePerformanceMetrics : RequestHandlerMetrics
    {
        private static readonly ConcurrentDictionary<string, UseCasePerformanceMetrics> _metricsDictionary =
            new ConcurrentDictionary<string, UseCasePerformanceMetrics>(StringComparer.InvariantCultureIgnoreCase);

        public static UseCasePerformanceMetrics GetUseCasePerformanceMetrics(string entityOrServiceName, string useCaseName)
        {
            string key = entityOrServiceName + "." + useCaseName;
            return _metricsDictionary.GetOrAdd
                (key, (string key1) =>
                {
                    var metrics = new UseCasePerformanceMetrics();
                    PerfCounterManager.Instance.AddInstance<UseCasePerformanceMetrics>(metrics, key, 
                        CounterStorageType.Memory);
                    return metrics;
                });
        }
    }
}

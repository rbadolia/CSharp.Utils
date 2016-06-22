using System;
using System.Diagnostics;

namespace CSharp.Utils.Diagnostics
{
    public static class SharedStopWatch
    {
        #region Static Fields

        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        #endregion Static Fields

        #region Public Properties

        public static TimeSpan Elapsed
        {
            get
            {
                return _sw.Elapsed;
            }
        }

        public static long ElapsedTicks
        {
            get
            {
                return _sw.Elapsed.Ticks;
            }
        }

        #endregion Public Properties
    }
}

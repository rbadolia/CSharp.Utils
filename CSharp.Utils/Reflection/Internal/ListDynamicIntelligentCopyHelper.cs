using System.Collections.Generic;

namespace CSharp.Utils.Reflection.Internal
{
    internal class ListDynamicIntelligentCopyHelper<TS, TT>
        where TT : new()
    {
        #region Public Methods and Operators

        public static void Copy(IEnumerable<TS> sources, IList<TT> targets)
        {
            foreach (TS source in sources)
            {
                var target = new TT();
                targets.Add(target);
                DynamicIntelligentCopyHelper<TS, TT>.Copy(source, target);
            }
        }

        #endregion Public Methods and Operators
    }
}

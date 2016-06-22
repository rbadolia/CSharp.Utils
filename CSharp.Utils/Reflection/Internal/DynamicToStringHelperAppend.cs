using System.IO;

namespace CSharp.Utils.Reflection.Internal
{
    internal class DynamicToStringHelperAppend
    {
        #region Methods

        internal static void WriteToWriter(bool excludeNullValues, bool dontWritePropertyNames, string prefix, string separator, string suffix, string propertyName, object propertyValue, TextWriter writer, bool isLastProperty)
        {
            if (!(propertyValue == null && excludeNullValues))
            {
                if (prefix != null)
                {
                    writer.Write(prefix);
                }

                if (!dontWritePropertyNames)
                {
                    writer.Write(propertyName);
                    writer.Write(separator);
                }

                if (propertyValue != null)
                {
                    writer.Write(propertyValue.ToString());
                }

                if (!isLastProperty && suffix != null)
                {
                    writer.Write(suffix);
                }
            }
        }

        #endregion Methods
    }
}

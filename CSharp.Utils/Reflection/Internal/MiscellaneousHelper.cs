using System.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Reflection.Internal
{
    internal static class MiscellaneousHelper
    {
        #region Methods

        internal static void AddValidationError(string propertyDisplayName, int index, List<ValidationResult> errors)
        {
            errors.Add(new ValidationResult { ErrorMessage = propertyDisplayName + " should not be left blank/null.", PropertyName = propertyDisplayName, ItemSequenceNumber = index });
        }

        #endregion Methods
    }
}

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace CSharp.Utils.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Parameter)]
    public sealed class NotEmptyAttribute : ValidationAttribute
    {
        private const string ErrorMessageFormat = "'{0}' must have at least one element.";

        public NotEmptyAttribute()
            : base(ErrorMessageFormat)
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            var collection = value as ICollection;
            return collection != null && collection.Count > 0;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(this.ErrorMessageString, name);
        }
    }
}

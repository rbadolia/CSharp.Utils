using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharp.Utils.Validation
{
    public static class Guard
    {
        #region Constants

        private const string STRING_MUST_NOT_BE_EMPTY_EXCEPTION_MESSAGE = "Argument must not be empty";

        #endregion Constants

        #region Public Methods and Operators

        public static void ArgumentNotNull(object argumentValue, string parameterName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void ArgumentNotNull(object argumentValue, string parameterName, string detailedMessage)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(parameterName, detailedMessage);
            }
        }

        public static void ArgumentNotNullOrEmpty(string argumentValue, string parameterName)
        {
            ArgumentNotNullOrEmpty(argumentValue, parameterName, STRING_MUST_NOT_BE_EMPTY_EXCEPTION_MESSAGE);
        }

        public static void ArgumentNotNullOrEmpty(string argumentValue, string parameterName, string detailedMessage)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException(detailedMessage, parameterName);
            }
        }

        public static void ArgumentNotNullOrEmptyOrWhiteSpace(string argumentValue, string parameterName)
        {
            ArgumentNotNullOrEmptyOrWhiteSpace(argumentValue, parameterName, STRING_MUST_NOT_BE_EMPTY_EXCEPTION_MESSAGE);
        }

        public static void ArgumentNotNullOrEmptyOrWhiteSpace(string argumentValue, string parameterName, string detailedMessage)
        {
            ArgumentNotNull(argumentValue, parameterName, detailedMessage);
            for (int i = 0; i < argumentValue.Length; i++)
            {
                if (!char.IsWhiteSpace(argumentValue[i]))
                {
                    return;
                }
            }

            throw new ArgumentException(detailedMessage, parameterName);
        }

        #endregion Public Methods and Operators

        public const string IndexerName = "Item[]";

        private const string StringMustNotBeEmptyExceptionMessage =
            "Argument must not be empty";

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentBigger(int min, int argumentValue, string argumentName)
        {
            if (argumentValue <= min)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentBigger(DateTime min, DateTime argumentValue, string argumentName)
        {
            if (argumentValue <= min)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentBigger(double min, double argumentValue, string argumentName)
        {
            if (argumentValue <= min)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentDateOnly(ref DateTime argumentValue, string argumentName)
        {
            if (argumentValue != argumentValue.Date)
            {
                throw new ArgumentException("value must be date-only (no time part is allowed)", argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentInRange(int min, int max, int argumentValue, string argumentName)
        {
            if (argumentValue < min || argumentValue > max)
            {
                string errorMessage = string.Format("argument is not in the range [{0}, {1}]", min, max);
                throw new ArgumentOutOfRangeException(argumentName, argumentValue, errorMessage);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentInRange(double min, double max, double argumentValue, string argumentName)
        {
            if (argumentValue < min || argumentValue > max)
            {
                string errorMessage = string.Format("argument is not in the range [{0}, {1}]", min, max);
                throw new ArgumentOutOfRangeException(argumentName, argumentValue, errorMessage);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentNotEmpty<T>(
            IEnumerable<T> argumentValue, 
            string argumentName)
        {
            if (argumentValue == null)
            {
                return;
            }

            if (!argumentValue.Any())
            {
                throw new ArgumentException("Enumeration must not be empty", argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentNotEmpty<T>(
            T[] argumentValue, 
            string argumentName)
        {
            if (argumentValue == null)
            {
                return;
            }

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException("Enumeration must not be empty", argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentNotEmpty(ref Guid key, string argumentName)
        {
            if (key == Guid.Empty)
            {
                throw new ArgumentException("Argument must not be empty", argumentName);
            }
        }

        public static void ArgumentPropertyNotNull<T>(T argumentValue, string argumentPath, string detailedMessage)
        where T : class
        {
            if (argumentValue == null)
            {
                throw new ArgumentException(argumentPath, detailedMessage);
            }
        }

        public static void ArgumentPropertyNotNull<T>(T argumentValue, string argumentPath)
        where T : class
        {
            if (argumentValue == null)
            {
                string argumentName = argumentPath.GetPathRoot();
                string defaultMessage = string.Format(
                    "argument \"{0}\" is invalid because \"{1}\" is null", 
                    argumentName, argumentPath);

                throw new ArgumentException(argumentPath, defaultMessage);
            }
        }

        public static void ArgumentPropertyNotNullOrEmpty(string argumentValue, string argumentPath, string detailedMessage)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentException(argumentPath, detailedMessage);
            }
        }

        public static void ArgumentPropertyNotNullOrEmpty(string argumentValue, string argumentPath)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                string argumentName = argumentPath.GetPathRoot();
                string defaultMessage = string.Format("argument \"{0}\" is invalid because \"{1}\" is null or empty", argumentName, argumentPath);

                throw new ArgumentException(argumentPath, defaultMessage);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void TypeEquals(Type assignmentTargetType, Type assignmentValueType, string argumentName, string detailedMessage)
        {
            if (assignmentTargetType == null)
            {
                throw new ArgumentNullException("assignmentTargetType");
            }

            if (assignmentValueType == null)
            {
                throw new ArgumentNullException("argumentName");
            }

            if (assignmentTargetType != assignmentValueType)
            {
                throw new ArgumentException(detailedMessage, argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void TypeIsAssignable(Type assignmentTargetType, Type assignmentValueType, string argumentName)
        {
            TypeIsAssignable(assignmentTargetType, assignmentValueType, argumentName, string.Format("Types are not assignable: {0} from {1}", assignmentTargetType, assignmentValueType));
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void TypeIsAssignable(Type assignmentTargetType, Type assignmentValueType, string argumentName, string detailedMessage)
        {
            if (assignmentTargetType == null)
            {
                throw new ArgumentNullException("assignmentTargetType");
            }

            if (assignmentValueType == null)
            {
                throw new ArgumentNullException("assignmentValueType");
            }

            if (!assignmentTargetType.IsAssignableFrom(assignmentValueType))
            {
                throw new ArgumentException(detailedMessage, argumentName);
            }
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        public static void ArgumentAlphaNumericRequired(string argumentValue, string argumentName)
        {
            if (!string.IsNullOrEmpty(argumentValue) && !argumentValue.All(char.IsLetterOrDigit))
            {
                throw new ArgumentException("Argument must not contain any special character", argumentName);
            }
        }

        private static string GetPathRoot(this string source)
        {
            ArgumentNotNull(source, "source");

            int firstSeparator = source.IndexOf('.');
            if (firstSeparator > -1)
            {
                return source.Substring(0, firstSeparator);
            }
            else
            {
                return source;
            }
        }
    }
}

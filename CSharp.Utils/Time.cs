using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharp.Utils
{
    [Serializable]
    public struct Time : IComparable, IComparable<Time>
    {
        #region Constants

        public const char TIME_SEPERATOR = ':';

        #endregion Constants

        #region Fields

        private int _hourInt;

        private int _millisecondsInt;

        private int _minuteInt;

        private int _secondInt;

        #endregion Fields

        #region Constructors and Finalizers

        public Time(string value)
        {
            this._hourInt = 0;
            this._minuteInt = 0;
            this._secondInt = 0;
            this._millisecondsInt = 0;

            string[] vals = value.Split(TIME_SEPERATOR); // new char[] { ':' });
            int hour = int.Parse(vals[0]);
            int minute = int.Parse(vals[1]);
            int second = 0;
            int millisecond = 0;
            if (vals.Length > 2)
            {
                second = int.Parse(vals[2]);
                if (vals.Length > 3)
                {
                    millisecond = int.Parse(vals[3]);
                }
            }

            this.validateAndSet(hour, minute, second, millisecond);
        }

        public Time(int hour, int minute, int second, int millisecond)
        {
            this._hourInt = 0;
            this._minuteInt = 0;
            this._secondInt = 0;
            this._millisecondsInt = millisecond;

            this.validateAndSet(hour, minute, second, millisecond);
        }

        public Time(DateTime dateTime)
            : this(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond)
        {
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static Time Now
        {
            get
            {
                DateTime dt = DateTime.Now;
                return new Time(dt);
            }
        }

        public static Time UtcNow
        {
            get
            {
                DateTime dt = DateTime.UtcNow;
                return new Time(dt);
            }
        }

        public int Hour
        {
            get
            {
                return this._hourInt;
            }

            set
            {
                if (value < 0 || value > 23)
                {
                    throw new ArgumentException(@"Hour should be >=0 and <=23", "value");
                }

                this._hourInt = value;
            }
        }

        public int Millisecond
        {
            get
            {
                return this._millisecondsInt;
            }

            set
            {
                if (value < 0 || value > 999)
                {
                    throw new ArgumentException(@"Millisecond should be >=0 and <=999", "value");
                }

                this._millisecondsInt = value;
            }
        }

        public int Minute
        {
            get
            {
                return this._minuteInt;
            }

            set
            {
                if (value < 0 || value > 59)
                {
                    throw new ArgumentException(@"Minute should be >=0 and <=59", "value");
                }

                this._minuteInt = value;
            }
        }

        public int Second
        {
            get
            {
                return this._secondInt;
            }

            set
            {
                if (value < 0 || value > 59)
                {
                    throw new ArgumentException(@"Second should be >=0 and <=59", "value");
                }

                this._secondInt = value;
            }
        }

        #endregion Public Properties

        #region Public Methods and Operators

        public static int Compare(Time x, Time y)
        {
            int val = x.Hour.CompareTo(y.Hour);
            if (val != 0)
            {
                return val;
            }

            val = x.Minute.CompareTo(y.Minute);
            if (val != 0)
            {
                return val;
            }

            return x.Second.CompareTo(y.Second);
        }

        public static bool Equals(Time x, Time y)
        {
            return Compare(x, y) == 0;
        }

        public static Time GetTimeFromMilliseconds(int milliseconds)
        {
            int seconds = milliseconds / 1000;
            milliseconds = milliseconds % 1000;
            int mins = seconds / 60;
            seconds = seconds % 60;

            int hours = mins / 60;
            mins = mins % 60;

            return new Time(hours, mins, seconds, milliseconds);
        }

        public static bool IsPassed(DateTime referenceTime, DateTime currentTime, Time time)
        {
            var dt = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
            return referenceTime <= dt && currentTime >= dt;
        }

        public static bool LiesBetween(Time from, Time to, Time time)
        {
            return time >= from && time <= to;
        }

        public static bool LiesBetween(IList<KeyValuePair<Time, Time>> timings, Time time, bool trueIfNoItems)
        {
            if (timings == null || timings.Count == 0)
            {
                return trueIfNoItems;
            }

            return timings.Select(kvp => LiesBetween(kvp.Key, kvp.Value, time)).FirstOrDefault();
        }

        public static Time Parse(string value)
        {
            try
            {
                return new Time(value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw new ApplicationException("Error parsing time!");
            }
        }

        public static TimeSpan TimeDiff(Time x, Time y)
        {
            DateTime dtNow = DateTime.Now;
            var dtX = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, x.Hour, x.Minute, x.Second, x.Millisecond);
            var dtY = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, y.Hour, y.Minute, y.Second, y.Millisecond);
            return dtX - dtY;
        }

        public static bool operator ==(Time x, Time y)
        {
            return Equals(x, y);
        }

        public static explicit operator Time(string s)
        {
            return Parse(s);
        }

        public static bool operator >(Time t1, Time t2)
        {
            if (t1.Hour != t2.Hour)
            {
                return t1.Hour > t2.Hour;
            }

            if (t1.Minute != t2.Minute)
            {
                return t1.Minute > t2.Minute;
            }

            if (t1.Second != t2.Second)
            {
                return t1.Second > t2.Second;
            }

            if (t1.Millisecond != t2.Millisecond)
            {
                return t1.Millisecond > t2.Millisecond;
            }

            return false;
        }

        public static bool operator >=(Time t1, Time t2)
        {
            if (t1 > t2)
            {
                return true;
            }

            return t1 == t2;
        }

        public static bool operator !=(Time x, Time y)
        {
            return !Equals(x, y);
        }

        public static bool operator <(Time t1, Time t2)
        {
            return t2 > t1;
        }

        public static bool operator <=(Time t1, Time t2)
        {
            return t2 >= t1;
        }

        public int CompareTo(object obj)
        {
            var t = (Time)obj;
            return Compare(this, t);
        }

        public int CompareTo(Time other)
        {
            return Compare(this, other);
        }

        public override bool Equals(object obj)
        {
            var t = (Time)obj;
            return Equals(this, t);
        }

        public override int GetHashCode()
        {
            return ~this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", this.Hour, this.Minute, this.Second, this.Millisecond);
        }

        #endregion Public Methods and Operators

        #region Methods

        private void validateAndSet(int hour, int minute, int second, int millisecond)
        {
            this.Hour = hour;
            this.Minute = minute;
            this.Second = second;
            this.Millisecond = millisecond;
        }

        #endregion Methods
    }
}

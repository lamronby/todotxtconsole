using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonExtensions
{
	public static class DotNetExtensions
    {

        #region IEnumerable

        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (!items.IsNullOrEmpty())
            {
                foreach (var item in items)
                    if (item != null)
                        action(item);
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || items.Count() == 0;
        }

        #endregion

        #region String

        public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source.IndexOf(toCheck, comp) >= 0;
		}

		public static bool IsDateGreaterThan(this string dateString, DateTime date)
		{
			if (dateString.IsNullOrEmpty())
				return false;

			DateTime comparisonDate;
			if (!DateTime.TryParse(dateString, out comparisonDate))
				return false;

			return comparisonDate.Date > date.Date;
		}

		public static bool IsDateLessThan(this string dateString, DateTime date)
		{
			if (dateString.IsNullOrEmpty())
				return false;

			DateTime comparisonDate;
			if (!DateTime.TryParse(dateString, out comparisonDate))
				return false;

			return comparisonDate.Date < date.Date;
        }

        #endregion

        #region Enums

	    /// <summary>
	    /// Parse a string to a specific enum. Ignore case, and use the
	    /// first value of the enum if the string does not parse.
	    /// </summary>
	    /// <param name="value">The enum string value to parse.</param>
	    /// <returns>The enum value for the string, or the default
	    /// value if the value cannot be parsed.</returns>
	    public static T ParseEnum<T>(string value) where T : struct, IConvertible
	    {
	        if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
	        T defValue = (T)((System.Collections.IList)Enum.GetValues(typeof(T)))[0];

	        if (string.IsNullOrEmpty(value)) return defValue;

	        try
	        {
	            return (T)Enum.Parse(typeof(T), value, true);
	        }
	        catch (ArgumentException)
	        {
	            return defValue;
	        }
	    }

	    /// <summary>
	    /// Parse a string to a specific enum. Ignore case, and use the default
	    /// value if the string does not parse.
	    /// </summary>
	    /// <param name="value">The enum string value to parse.</param>
	    /// <param name="defaultValue">A default enum to use if the
	    /// value cannot be parsed.</param>
	    /// <returns>The enum value for the string, or the default
	    /// value if the value cannot be parsed.</returns>
	    public static T ParseEnum<T>(string value, T defaultValue) where T : struct, IConvertible
	    {
	        if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
	        if (string.IsNullOrEmpty(value)) return defaultValue;

	        try
	        {
	            return (T)Enum.Parse(typeof(T), value, true);
	        }
	        catch (ArgumentException)
	        {
	            return defaultValue;
	        }
	    }

	    #endregion
    }
}

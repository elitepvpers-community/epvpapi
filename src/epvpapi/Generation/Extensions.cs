using System;
using System.Globalization;

namespace epvpapi.Generation
{
    class Extensions
    {
        public static T To<T>(this string value) where T : IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}

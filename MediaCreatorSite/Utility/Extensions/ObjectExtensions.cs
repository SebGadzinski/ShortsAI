using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MediaCreatorSite.Utility.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns true if the content of one object is the same as the other
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="comparedItem"></param>
        /// <param name="propertiesToCompare"></param>
        /// <param name="propertiesToNotCompare"></param>
        /// <returns></returns>
        public static bool Compare<T>(this T item, T comparedItem, bool noWhiteSpaces = false, List<string>? propertiesToCompare = null, List<string>? propertiesToNotCompare = null)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));
            if (properties == null) return false;
            if (propertiesToCompare == null)
            {
                propertiesToCompare = new List<string>();
                foreach (PropertyDescriptor prop in properties)
                    propertiesToCompare.Add(prop.Name);
            }
            if (propertiesToNotCompare != null)
            {
                propertiesToCompare.RemoveAll(x => propertiesToNotCompare.Any(y => y.Equals(x)));
            }
            foreach (var property in propertiesToCompare)
            {
                var itemProp = properties.Find(property, false);
                if (itemProp != null)
                {
                    var itemValue = itemProp.GetValue(item);
                    var comparedItemValue = itemProp.GetValue(comparedItem);
                    if (noWhiteSpaces)
                    {
                        if (itemValue != null && itemValue.GetType() == typeof(string)) itemValue = sWhitespace.Replace(itemValue.ToString(), "");
                        if (comparedItemValue != null && comparedItemValue.GetType() == typeof(string)) comparedItemValue = sWhitespace.Replace(comparedItemValue.ToString(), "");
                    }
                    if (comparedItemValue == null && itemValue == null) continue;
                    if (itemValue != null && itemValue.GetType() == typeof(string) && itemValue.Equals(comparedItemValue)) continue;
                    if ((comparedItemValue == null && itemValue != null) || (comparedItemValue != null && itemValue == null) || (comparedItemValue.GetType() != typeof(DateTime) && itemValue == comparedItemValue) || (comparedItemValue.GetType() != typeof(DateTime) && !itemValue.Equals(comparedItemValue)) || (comparedItemValue != null && itemValue == null))
                        return false;
                }
            }
            return true;
        }

        public static readonly Regex sWhitespace = new Regex(@"\s+");

    }
}

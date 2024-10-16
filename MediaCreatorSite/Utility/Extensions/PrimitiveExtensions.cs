namespace MediaCreatorSite.Utility.Extensions
{
    public static class PrimitiveExtensions
    {
        public static string Cut(this string target, int cutoff)
        {
            return target.Length < cutoff ? target : target[..(cutoff - 1)];
        }
        public static bool EqualsNoCase(this string value, string target)
        {
            return value.ToLower().Equals(target.ToLower());
        }
    }
}

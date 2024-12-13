using System.Text.RegularExpressions;

namespace Device.Identification.Optimization
{
    public class PatternHelper
    {
        public static Regex BuildWildcardRegex(string input)
        {
            input = input.Replace("*", "<star>");
            input = Regex.Escape(input);
            input = input.Replace("<star>", ".*");
            return new Regex(string.Concat("^", input, "$"), RegexOptions.IgnoreCase);
        }
    }
}

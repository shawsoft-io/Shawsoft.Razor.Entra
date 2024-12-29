using System.Text.RegularExpressions;

namespace Shawsoft.Razor.Entra.Utilities
{
    public class AvatarHelper
    {
        public static string GetRandomColor(string seed)
        {
            string[] colors = ["#093A3E", "#FFB238", "#BF4342", "#2D93AD", "#2D93AD"];
            int index = seed.Length % colors.Length;

            return colors[index];
        }

        public static string ReplaceWithBold(string? str)
        {
            var pattern = @"!\[(.+?)\]";
            var replacement = @"<span class='font-medium text-gray-900'>$1</span>";

            return Regex.Replace(str ?? "", pattern, replacement);
        }

    }
}

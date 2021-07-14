using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WiseWeather
{
    static public class APIParser
    {
        public static Dictionary<string, Dictionary<string, string>> Parse(string response) //Yes, it is a dictionary inside a dictionary...
        {
            var APIDictionary = new Dictionary<string, Dictionary<string, string>>();
            response.Trim(' ', '{', '}');

            for (int i = 0; i < response.Length; i++)
            {
                if (response[i] == '"')
                {
                    int nextIndex = FindNextIndex(response, i, '"');
                    if (response[nextIndex + 2] != '{' && response[nextIndex + 2] != '[') continue;
                    int nextLeftBracket = FindNextIndex(response, nextIndex, '{');
                    int nextRightBracket = FindNextIndex(response, nextLeftBracket, '}');

                    APIDictionary.Add(response.Substring(i + 1, nextIndex - i - 1), DivideSubstring(response.Substring(nextLeftBracket, nextRightBracket - nextLeftBracket + 1)));
                    i = nextRightBracket;   
                }
            }
            return APIDictionary;
        }

        private static int FindNextIndex(string text, int startIndex, char character)
        {
            for (int i = startIndex + 1; i < text.Length; i++)
            {
                if (text[i] == character) return i;
            }
            return 0;
        }

        private static Dictionary<string, string> DivideSubstring(string substring)
        {
            Dictionary<string, string> substringDictionary = new Dictionary<string, string>();
            substring = substring.Trim(' ', '{', '}');

            string[] pares = substring.Split(',');

            foreach (string pare in pares)
            {
                string[] pareParts = pare.Split(':');
                substringDictionary.Add(pareParts[0].Trim(' ', '"'), pareParts[1].Trim(' ', '"'));
            }
            return substringDictionary;
        }
    }
}

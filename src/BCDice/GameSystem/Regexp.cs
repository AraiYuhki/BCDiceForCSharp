using System;
using System.Text.RegularExpressions;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Ruby Regexp compatibility helper for auto-converted game systems.
    /// Provides thread-safe last match storage similar to Ruby's Regexp.last_match.
    /// </summary>
    internal static class Regexp
    {
        [ThreadStatic]
        private static Match _lastMatch;

        /// <summary>
        /// Tests if input matches pattern and stores the match result.
        /// </summary>
        public static bool IsMatch(string input, Regex pattern)
        {
            if (input == null) return false;
            _lastMatch = pattern.Match(input);
            return _lastMatch.Success;
        }

        /// <summary>
        /// Tests if input matches pattern string and stores the match result.
        /// </summary>
        public static bool IsMatch(string input, string pattern)
        {
            if (input == null) return false;
            _lastMatch = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            return _lastMatch.Success;
        }

        /// <summary>
        /// Tests if input matches pattern (reversed argument order for Ruby compat).
        /// </summary>
        public static bool IsMatch(Regex pattern, string input)
        {
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// Gets the captured group from the last match.
        /// Returns null if no match or group doesn't exist.
        /// </summary>
        public static string LastMatch(int group)
        {
            if (_lastMatch == null || !_lastMatch.Success) return null;
            if (group >= _lastMatch.Groups.Count) return null;
            var value = _lastMatch.Groups[group].Value;
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Gets the captured group as int, with a default value.
        /// Ruby: Regexp.last_match[n] || default
        /// </summary>
        public static int LastMatchInt(int group, int defaultValue = 0)
        {
            var value = LastMatch(group);
            if (value == null) return defaultValue;
            if (int.TryParse(value, out var result)) return result;
            return defaultValue;
        }

        /// <summary>
        /// Gets the captured group as string, with a default value.
        /// Ruby: Regexp.last_match[n] || default
        /// </summary>
        public static string LastMatchStr(int group, string defaultValue = "")
        {
            return LastMatch(group) ?? defaultValue;
        }

        /// <summary>
        /// Performs regex replacement with a match evaluator (Ruby gsub with block).
        /// </summary>
        public static string Replace(string input, Regex pattern, MatchEvaluator evaluator)
        {
            if (input == null) return null;
            return pattern.Replace(input, evaluator);
        }

        /// <summary>
        /// Performs regex replacement with a string (Ruby gsub with string).
        /// </summary>
        public static string Replace(string input, Regex pattern, string replacement)
        {
            if (input == null) return null;
            return pattern.Replace(input, replacement ?? "");
        }

        /// <summary>
        /// Gets the full match from the last match.
        /// </summary>
        public static Match GetLastMatch()
        {
            return _lastMatch;
        }

        /// <summary>
        /// Stores a match result for later retrieval via LastMatch.
        /// </summary>
        public static void SetLastMatch(Match match)
        {
            _lastMatch = match;
        }
    }
}
﻿namespace Unosquare.Swan
{
    using Formatters;
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    partial class Extensions
    {
        #region Private Declarations

        private const RegexOptions StandardRegexOptions =
            RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant;

        private static readonly Lazy<MD5> Md5Hasher = new Lazy<MD5>(MD5.Create, true);
        private static readonly Lazy<SHA1> SHA1Hasher = new Lazy<SHA1>(SHA1.Create, true);
        private static readonly Lazy<SHA256> SHA256Hasher = new Lazy<SHA256>(SHA256.Create, true);
        private static readonly Lazy<SHA512> SHA512Hasher = new Lazy<SHA512>(SHA512.Create, true);

        private static readonly Lazy<Regex> SplitLinesRegex =
            new Lazy<Regex>(
                () => new Regex("\r\n|\r|\n", StandardRegexOptions));

        private static readonly Lazy<Regex> UnderscoreRegex =
            new Lazy<Regex>(
                () => new Regex(@"_", StandardRegexOptions));

        private static readonly Lazy<Regex> CamelCaseRegEx =
            new Lazy<Regex>(
                () =>
                    new Regex(@"[a-z][A-Z]",
                        StandardRegexOptions));

        private static readonly Lazy<MatchEvaluator> SplitCamelCaseString = new Lazy<MatchEvaluator>(() =>
        {
            return ((m) =>
            {
                var x = m.ToString();
                return x[0] + " " + x.Substring(1, x.Length - 1);
            });
        });

        #endregion

        /// <summary>
        /// Computes the MD5 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeMD5(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return Md5Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-1 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeSha1(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return SHA1Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-256 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeSha256(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return SHA256Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-512 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeSha512(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return SHA512Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static string ToStringInvariant(this object item)
        {
            if (item == null)
                return string.Empty;

            var itemType = item.GetType();

            if (itemType == typeof(string))
                return item as string;

            return Constants.BasicTypesInfo.ContainsKey(itemType)
                ? Constants.BasicTypesInfo[itemType].ToStringInvariant(item)
                : item.ToString();
        }

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static string ToStringInvariant<T>(this T item)
        {
            if (typeof(string) == typeof(T))
                return item == null ? string.Empty : item as string;

            return ToStringInvariant(item as object);
        }

        /// <summary>
        /// Removes the control characters from a string except for those sepcified.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="excludeChars">When specified, these characters will not be removed.</param>
        /// <returns></returns>
        public static string RemoveControlCharsExcept(this string input, params char[] excludeChars)
        {
            if (excludeChars == null)
                excludeChars = new char[] {};

            return new string(input
                .Where(c => char.IsControl(c) == false || excludeChars.Contains(c))
                .ToArray());
        }

        /// <summary>
        /// Removes all control characters from a string, including new line sequences.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string RemoveControlChars(this string input)
        {
            return input.RemoveControlCharsExcept(null);
        }

        /// <summary>
        /// Outputs JSON representing this object
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> format the output.</param>
        /// <returns></returns>
        public static string ToJson(this object obj, bool format = true)
        {
            if (obj == null) return string.Empty;
            return Json.Serialize(obj, format);
        }

        /// <summary>
        /// Returns text representing the properties of the specified object in a human-readable format.
        /// While this method is fairly expensive computationally speaking, it provides an easy way to
        /// examine objects.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string Stringify(this object obj)
        {
            if (obj == null) return string.Empty;
            var jsonText = Json.Serialize(obj, false, "$type");
            var jsonData = Json.Deserialize(jsonText);

            if (jsonData == null) return string.Empty;
            var readableData = HumanizeJson(jsonData, 0);
            return readableData;
        }

        /// <summary>
        /// Retrieves a section of the string, inclusive of both, the start and end indexes.
        /// This behavior is unlike JavaScript's Slice behavior where the end index is non-inclusive
        /// If the string is null it returns an empty string
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns></returns>
        public static string Slice(this string str, int startIndex, int endIndex)
        {
            if (str == null) return string.Empty;
            endIndex = endIndex.Clamp(startIndex, str.Length - 1);
            if (startIndex >= endIndex) return string.Empty;
            return str.Substring(startIndex, (endIndex - startIndex) + 1);
        }

        /// <summary>
        /// Gets a part of the string clamping the length and startIndex parameters to safe values.
        /// If the string is null it returns an empty string
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string SliceLength(this string str, int startIndex, int length)
        {
            if (str == null) return string.Empty;
            startIndex = startIndex.Clamp(0, str.Length - 1);
            length = length.Clamp(0, str.Length - startIndex);

            if (length == 0) return string.Empty;
            return str.Substring(startIndex, length);
        }

        /// <summary>
        /// Splits the specified text into r, n or rn separated lines
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string[] ToLines(this string text)
        {
            if (text == null) return new string[] { };
            return SplitLinesRegex.Value.Split(text);
        }

        /// <summary>
        /// Humanizes (make more human-readable) an identifier-style string 
        /// in either camel case or snake case. For example, CamelCase will be converted to 
        /// Camel Case and Snake_Case will be converted to Snake Case.
        /// </summary>
        /// <param name="identifierString">The identifier-style string.</param>
        /// <returns></returns>
        public static string Humanize(this string identifierString)
        {
            var returnValue = identifierString ?? string.Empty;
            returnValue = UnderscoreRegex.Value.Replace(returnValue, " ");
            returnValue = CamelCaseRegEx.Value.Replace(returnValue, SplitCamelCaseString.Value);
            return returnValue;
        }

        /// <summary>
        /// Indents the specified multi-line text with the given amount of spaces.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="spaces">The spaces.</param>
        /// <returns></returns>
        public static string Indent(this string text, int spaces = 4)
        {
            if (text == null) text = string.Empty;
            if (spaces <= 0) return text;

            var lines = text.ToLines();
            var builder = new StringBuilder();
            var indentStr = new string(' ', spaces);
            foreach (var line in lines)
            {
                builder.AppendLine($"{indentStr}{line}");
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>
        /// To the one character string.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public static string ToOneCharString(this char c)
        {
            return new string(c, 1);
        }

        // TODO: Test Humanize, Add pluralize, singularize, dashize, capitalize titleize, etc.
    }
}
﻿namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;

    partial class Terminal
    {
        #region ReadKey

        /// <summary>
        /// Reads a key from the Terminal. This is the closest equivalent to Console.ReadKey
        /// </summary>
        /// <param name="intercept">if set to <c>true</c> the pressed key will not be rendered to the output.</param>
        /// <param name="disableLocking">if set to <c>true</c> the output will continue to be shown. 
        /// This is useful for services and daemons that are running as console applications and wait for a key to exit the program.
        /// </param>
        public static ConsoleKeyInfo ReadKey(bool intercept, bool disableLocking = false)
        {
            if (IsConsolePresent == false) return default(ConsoleKeyInfo);
            if (disableLocking) return Console.ReadKey(intercept);

            lock (SyncLock)
            {
                Flush();
                InputDone.Reset();
                try
                {
                    Console.CursorVisible = true;
                    return Console.ReadKey(intercept);
                }
                finally
                {
                    Console.CursorVisible = false;
                    InputDone.Set();
                }
            }

        }

        /// <summary>
        /// Reads a key from the Terminal
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="preventEcho">if set to <c>true</c> [prevent echo].</param>
        public static ConsoleKeyInfo ReadKey(this string prompt, bool preventEcho)
        {
            if (IsConsolePresent == false) return default(ConsoleKeyInfo);

            lock (SyncLock)
            {
                if (prompt != null)
                    ($" {(string.IsNullOrWhiteSpace(Settings.LoggingTimeFormat) ? string.Empty : DateTime.Now.ToString(Settings.LoggingTimeFormat) + " ")}" +
                        $"{Settings.UserInputPrefix} << {prompt} ").Write(ConsoleColor.White);

                var input = ReadKey(true);
                var echo = preventEcho ? string.Empty : input.Key.ToString();
                echo.WriteLine();
                return input;
            }

        }

        /// <summary>
        /// Reads a key from the terminal preventing the key from being echoed.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <returns></returns>
        public static ConsoleKeyInfo ReadKey(this string prompt)
        {
            return prompt.ReadKey(true);
        }

        #endregion

        #region Other Terminal Read Methods

        /// <summary>
        /// Reads a line of text from the console
        /// </summary>
        public static string ReadLine()
        {
            if (IsConsolePresent == false) return default(string);

            lock (SyncLock)
            {
                Flush();
                InputDone.Reset();
                try
                {
                    Console.CursorVisible = true;
                    return Console.ReadLine();
                }
                finally
                {
                    Console.CursorVisible = false;
                    InputDone.Set();
                }
            }

        }

        /// <summary>
        /// Reads a number from the input. If unable to parse, it returns the default number
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="defaultNumber">The default number.</param>
        /// <returns></returns>
        public static int ReadNumber(this string prompt, int defaultNumber)
        {
            if (IsConsolePresent == false) return defaultNumber;

            lock (SyncLock)
            {
                $" {DateTime.Now:HH:mm:ss} USR << {prompt} (default is {defaultNumber}): ".Write(ConsoleColor.White);
                var input = ReadLine();
                int parsedInt;
                if (int.TryParse(input, out parsedInt) == false)
                {
                    parsedInt = defaultNumber;
                }

                return parsedInt;
            }

        }

        /// <summary>
        /// Creates a table prompt where the user can enter an option based on the options dictionary provided
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="options">The options.</param>
        /// <param name="anyKeyOption">Any key option.</param>
        /// <returns></returns>
        public static ConsoleKeyInfo ReadPrompt(this string title, Dictionary<ConsoleKey, string> options, string anyKeyOption)
        {
            if (IsConsolePresent == false) return default(ConsoleKeyInfo);

            const ConsoleColor textColor = ConsoleColor.White;
            var lineLength = Console.BufferWidth;
            var lineAlign = -(lineLength - 2);
            var textFormat = "{0," + lineAlign + "}";

            lock (SyncLock) // lock the output as an atomic operation
            {
                { // Top border
                    Table.TopLeft();
                    Table.Horizontal(-lineAlign);
                    Table.TopRight();
                }

                { // Title
                    Table.Vertical();
                    var titleText = string.Format(textFormat,
                        string.IsNullOrWhiteSpace(title) ?
                            " Select an option from the list below." :
                            $" {title}");
                    titleText.Write(textColor); //, titleText);
                    Table.Vertical();
                }

                { // Title Bottom
                    Table.LeftTee();
                    Table.Horizontal(lineLength - 2);
                    Table.RightTee();
                }

                // Options
                foreach (var kvp in options)
                {
                    Table.Vertical();
                    string.Format(textFormat,
                        $"    {"[ " + kvp.Key + " ]",-10}  {kvp.Value}").Write(textColor);
                    Table.Vertical();
                }

                // Any Key Options
                if (string.IsNullOrWhiteSpace(anyKeyOption) == false)
                {
                    Table.Vertical();
                    string.Format(textFormat, " ").Write(ConsoleColor.Gray);
                    Table.Vertical();

                    Table.Vertical();
                    string.Format(textFormat,
                        $"    {" ",-10}  {anyKeyOption}").Write(ConsoleColor.Gray);
                    Table.Vertical();
                }

                { // Input section
                    Table.LeftTee();
                    Table.Horizontal(lineLength - 2);
                    Table.RightTee();

                    Table.Vertical();
                    string.Format(textFormat,
                        Settings.UserOptionText).Write(ConsoleColor.Green);
                    Table.Vertical();

                    Table.BottomLeft();
                    Table.Horizontal(lineLength - 2);
                    Table.BottomRight();
                }

            }

            var inputLeft = Settings.UserOptionText.Length + 3;

            SetCursorPosition(inputLeft, CursorTop - 2);
            var userInput = ReadKey(true);
            userInput.Key.ToString().Write(ConsoleColor.Gray);

            SetCursorPosition(0, CursorTop + 2);
            return userInput;
        }

        #endregion
    }
}

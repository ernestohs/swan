﻿namespace Unosquare.Swan
{
    using System.Text;

    /// <summary>
    /// Contains useful constants and definitions
    /// </summary>
    public static partial class Definitions
    {
        /// <summary>
        /// Initializes the <see cref="Definitions"/> class.
        /// </summary>
        static Definitions()
        {

            CurrentAnsiEncoding = Encoding.GetEncoding(default(int));

            try
            {
                Windows1252Encoding = Encoding.GetEncoding(1252);
            }
            catch
            {
                // ignore, the codepage is not available use default
                Windows1252Encoding = CurrentAnsiEncoding;
            }
        }

        /// <summary>
        /// The MS Windows codepage 1252 encoding used in some legacy scenarios
        /// such as default CSV text encoding from Excel
        /// </summary>
        public static readonly Encoding Windows1252Encoding;

        /// <summary>
        /// The encoding associated with the default ANSI code page in the operating 
        /// system's regional and language settings
        /// </summary>
        public static readonly Encoding CurrentAnsiEncoding;

        #region Network

        /// <summary>
        /// The DNS default port
        /// </summary>
        public const int DnsDefaultPort = 53;

        /// <summary>
        /// The NTP default port
        /// </summary>
        public const int NtpDefaultPort = 123;
        
        #endregion
    }
}

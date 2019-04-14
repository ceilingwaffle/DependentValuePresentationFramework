namespace DVPF.Core
{
    using System;

    /// <summary>
    /// Helpers class
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Instance of <classref name="System.Random">System.Random</classref>
        /// </summary>
        private static readonly Random Rand = new Random();

        /// <summary>
        /// Returns the current unix timestamp
        /// </summary>
        /// <returns>The current unix timestamp</returns>
        internal static int GetCurrentUnixTimestamp()
        {
            return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// Returns a random number between (inclusive) <paramref name="from"/> and <paramref name="to"/>.
        /// </summary>
        /// <param name="from">'Greater than or equal to' this number</param>
        /// <param name="to">'Less than or equal to' this number</param>
        /// <returns>a random number between (inclusive) <paramref name="from"/> and <paramref name="to"/></returns>
        internal static int GetRandomNumberBetween(int from, int to)
        {
            return Rand.Next(from, to + 1);
        }
    }
}

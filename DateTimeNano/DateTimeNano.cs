/****************************************************************************************************************
 * DateTimeNano - A DateTime with nanosecond precision.
 * Copyright (c) 2025 Nich Overend (nich@nixnet.com)
 * Licensed under the MIT License. See LICENSE in the project root for license information.
****************************************************************************************************************/

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Seerstone
{
    /// <summary>
    /// DateTimeNano - A DateTime with nanosecond precision.
    /// Written to replace NodaTime which is just too much for what we actually need.
    /// Stores the number of nanoseconds since the Unix epoch (1970-01-01 00:00:00 UTC) internally
    /// and everything else is calculated from that. So if you don't look at anything except that
    /// it should be really really quick.
    /// DataBento timestamps are mainly in nanoseconds since epoch.
    /// </summary>
    [ProtoContract]
    public struct DateTimeNano
    {
        /// <summary>
        /// Unix Epoch Date/Time
        /// </summary>
        public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// DateTime part of the DateTimeNano (excludes the nanoseconds portion - not even the hundreths!)
        /// </summary>
        public DateTime DateTime => new DateTime(TotalTicks);

        public DateTime Date => DateTime.Date;

        /// <summary>
        /// Nanoseconds since epoch
        /// </summary>
        [ProtoMember(1)]
        public ulong NanosecondsSinceEpoch { get; set; }

        /// <summary>
        /// Total Ticks (excludes the nanoseconds portion)
        /// </summary>
        public long TotalTicks => Epoch.AddTicks((long)(NanosecondsSinceEpoch / 1000) * 10).Ticks;

        /// <summary>
        /// Nanoseconds (just the last three digits)
        /// </summary>
        public int Nanoseconds => (int)(NanosecondsSinceEpoch % 1000);

        /// <summary>
        /// Fractional seconds in 10^9 (nanoseconds)
        /// </summary>
        public long SecondsFractionInNanoseconds => DateTime.Millisecond * 1000_000 + DateTime.Microsecond * 1000 + Nanoseconds;

        /// <summary>
        /// Parse a string in the format "yyyy-MM-dd HH:mm:ss.fffffffff"
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns></returns>
        public static DateTimeNano Parse(string dateTimeString)
        {
            var dateTimeRegex =
                new Regex(
                    @"(?<year>\d+)-(?<month>\d+)-(?<day>\d+)\D(?<hour>\d+):(?<minute>\d+):(?<second>\d+)\.*(?<millisecond>\d{0,3})(?<microsecond>\d{0,3})(?<nanosecond>\d{0,3})");

            var match = dateTimeRegex.Match(dateTimeString);
            if (match.Success)
            {
                var year = int.Parse(match.Groups["year"].Value);
                var month = int.Parse(match.Groups["month"].Value);
                var day = int.Parse(match.Groups["day"].Value);
                var hour = int.Parse(match.Groups["hour"].Value);
                var minute = int.Parse(match.Groups["minute"].Value);
                var second = int.Parse(match.Groups["second"].Value);
                var millisecond = !string.IsNullOrEmpty(match.Groups["millisecond"].Value) ? int.Parse(match.Groups["millisecond"].Value) : 0;
                var microsecond = !string.IsNullOrEmpty(match.Groups["microsecond"].Value) ? int.Parse(match.Groups["microsecond"].Value) : 0;
                var nanosecond = !string.IsNullOrEmpty(match.Groups["nanosecond"].Value) ? int.Parse(match.Groups["nanosecond"].Value) : 0;
                return new DateTimeNano(year, month, day, hour, minute, second, millisecond, microsecond, nanosecond);
            }

            throw new ArgumentException("Invalid DateTimeNano format\n yyyy-MM-dd hh:mm:ss.fffffffff");
        }

        /// <summary>
        /// Create DateTimeNano from DateTime
        /// </summary>
        /// <param name="datetime"></param>
        public DateTimeNano(DateTime datetime)
        {
            NanosecondsSinceEpoch = (ulong)datetime.Subtract(Epoch).Ticks * 100;
        }

        /// <summary>
        /// Create a new DateTimeNano from a Unix nanosecond epoch.
        /// Use this constructor for DataBento timestamps.
        /// </summary>
        /// <param name="nanosecondsSinceEpoch"></param>
        public DateTimeNano(ulong nanosecondsSinceEpoch)
        {
            NanosecondsSinceEpoch = nanosecondsSinceEpoch;
        }

        /// <summary>
        /// Create a new DateTimeNano from it's parts.
        /// These are not validated before they are used to construct a DateTime object internally,
        /// so the same rules apply here as for DateTime.
        /// Microseconds are Added to the DateTime after construction.
        /// Nanoseconds are added after the DateTime conversion to NanosecondsSinceEpoch.
        /// </summary>
        /// <param name="year">year as int</param>
        /// <param name="month">month as int</param>
        /// <param name="day">day as int</param>
        /// <param name="hour">hour as int</param>
        /// <param name="minute">minute as int</param>
        /// <param name="second">second as int</param>
        /// <param name="millisecond">millisecond as int</param>
        /// <param name="microseconds">microsecond as int</param>
        /// <param name="nanoseconds">nanosecond as int</param>
        public DateTimeNano(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0,
            int microseconds = 0, int nanoseconds = 0)
        {
            var dateTime = new DateTime(year, month, day, hour, minute, second, millisecond).AddMicroseconds(microseconds);
            NanosecondsSinceEpoch = (ulong)(dateTime.Subtract(Epoch).Ticks * 100 + nanoseconds);
        }

        /// <summary>
        /// Add nanoseconds to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="nanoseconds">nanoseconds to add</param>
        /// <returns>A new DateTimeNano</returns>
        public DateTimeNano AddNanoseconds(long nanoseconds)
        {
            return new DateTimeNano(NanosecondsSinceEpoch + (ulong)nanoseconds);
        }

        /// <summary>
        /// Add microseconds to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="microseconds"></param>
        /// <returns></returns>
        public DateTimeNano AddMicroseconds(long microseconds)
        {
            return AddNanoseconds(microseconds * 1000);
        }

        public DateTimeNano AddMilliseconds(long milliseconds)
        {
            return AddNanoseconds(milliseconds * 1000_000);
        }

        public DateTimeNano AddSeconds(long seconds)
        {
            return AddNanoseconds(seconds * 1000_000_000);
        }

        public DateTimeNano AddMinutes(long minutes)
        {
            return AddNanoseconds(minutes * 60 * 1000_000_000);
        }

        public DateTimeNano AddHours(long hours)
        {
            return AddNanoseconds(hours * 60 * 60 * 1000_000_000);
        }

        public DateTimeNano AddDays(int days)
        {
            return AddNanoseconds(days * 24 * 60 * 60 * 1000_000_000);
        }

        public DateTimeNano AddMonths(int months)
        {
            return new DateTimeNano(DateTime.AddMonths(months)).AddNanoseconds(Nanoseconds);
        }

        /// <summary>
        /// Returns the DateTime portion of the DateTimeNano as a DateTime object.
        /// This will include microseconds, but not nanoseconds.
        /// These can be accessed separatedly using DateTimeNano.Nanoseconds.
        /// </summary>
        /// <returns></returns>
        public DateTime ToDateTimeUtc()
        {
            return DateTime;
        }

        /// <summary>
        /// Returns the number of nanoseconds since the Unix epoch (1970-01-01 00:00:00 UTC).
        /// </summary>
        /// <returns></returns>
        public ulong ToUnixNanoseconds()
        {
            return NanosecondsSinceEpoch;
        }

        /// <summary>
        /// Default implementation of ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{DateTime:yyyy-MM-dd HH:mm:ss}.{SecondsFractionInNanoseconds:D9}";
        }

        public static implicit operator DateTime(DateTimeNano d) => d.ToDateTimeUtc();

        public static explicit operator DateTimeNano(DateTime b) => new DateTimeNano(b);
    }
}

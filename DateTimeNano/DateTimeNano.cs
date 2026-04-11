/****************************************************************************************************************
 * DateTimeNano - A DateTime with nanosecond precision.
 * Copyright (c) 2025 Nich Overend (nich@nixnet.com)
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 ****************************************************************************************************************/

using ProtoBuf;
using System;
using System.Text.RegularExpressions;

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
    public struct DateTimeNano : IEquatable<DateTimeNano>, IComparable<DateTimeNano>
    {
        /// <summary>
        /// Unix Epoch Date/Time (1970-01-01 00:00:00 UTC).
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly Regex DateTimeRegex = new Regex(
            @"(?<year>\d+)-(?<month>\d+)-(?<day>\d+)\D(?<hour>\d+):(?<minute>\d+):(?<second>\d+)\.*(?<millisecond>\d{0,3})(?<microsecond>\d{0,3})(?<nanosecond>\d{0,3})",
            RegexOptions.Compiled);

        /// <summary>
        /// UTC DateTime part of the DateTimeNano (excludes the sub-microsecond nanoseconds portion).
        /// </summary>
        public DateTime DateTime => new DateTime(TotalTicks, DateTimeKind.Utc);

        /// <summary>
        /// The UTC date portion of the DateTimeNano, with time set to midnight.
        /// </summary>
        public DateTime Date => DateTime.Date;

        /// <summary>
        /// Nanoseconds since epoch
        /// </summary>
        [ProtoMember(1)]
        public ulong NanosecondsSinceEpoch { get; set; }

        /// <summary>
        /// Total ticks (100 ns units) truncated to microsecond precision; excludes sub-microsecond nanoseconds.
        /// </summary>
        public long TotalTicks => Epoch.AddTicks((long)(NanosecondsSinceEpoch / 1000) * 10).Ticks;

        /// <summary>
        /// Sub-microsecond nanoseconds (0-999), i.e. the last three digits of the nanosecond timestamp.
        /// </summary>
        public int Nanoseconds => (int)(NanosecondsSinceEpoch % 1000);

        /// <summary>
        /// Fractional seconds expressed in nanoseconds (0-999_999_999).
        /// </summary>
        public long SecondsFractionInNanoseconds => DateTime.Millisecond * 1000_000 + DateTime.Microsecond * 1000 + Nanoseconds;

        /// <summary>
        /// Parse a string in the format "yyyy-MM-dd HH:mm:ss.fffffffff".
        /// Throws <see cref="ArgumentException"/> if the string does not match the expected format.
        /// </summary>
        /// <param name="dateTimeString">The string to parse.</param>
        /// <returns>A new <see cref="DateTimeNano"/>.</returns>
        public static DateTimeNano Parse(string dateTimeString)
        {
            if (TryParse(dateTimeString, out var result))
                return result;

            throw new ArgumentException("Invalid DateTimeNano format. Expected: yyyy-MM-dd HH:mm:ss.fffffffff", nameof(dateTimeString));
        }

        /// <summary>
        /// Attempts to parse a string in the format "yyyy-MM-dd HH:mm:ss.fffffffff".
        /// </summary>
        /// <param name="dateTimeString">The string to parse.</param>
        /// <param name="result">
        ///   When this method returns <see langword="true"/>, contains the parsed <see cref="DateTimeNano"/>;
        ///   otherwise contains <see langword="default"/>.
        /// </param>
        /// <returns><see langword="true"/> if the string was successfully parsed; otherwise <see langword="false"/>.</returns>
        public static bool TryParse(string? dateTimeString, out DateTimeNano result)
        {
            result = default;
            if (string.IsNullOrEmpty(dateTimeString))
                return false;

            var match = DateTimeRegex.Match(dateTimeString);
            if (!match.Success)
                return false;

            try
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
                result = new DateTimeNano(year, month, day, hour, minute, second, millisecond, microsecond, nanosecond);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create a <see cref="DateTimeNano"/> from a <see cref="System.DateTime"/>.
        /// If the <see cref="System.DateTime"/> has <see cref="DateTimeKind.Local"/>, it is first
        /// converted to UTC before the nanosecond value is computed.
        /// </summary>
        /// <param name="datetime">The source <see cref="System.DateTime"/>.</param>
        public DateTimeNano(DateTime datetime)
        {
            var utc = datetime.Kind == DateTimeKind.Local ? datetime.ToUniversalTime() : datetime;
            NanosecondsSinceEpoch = (ulong)utc.Subtract(Epoch).Ticks * 100;
        }

        /// <summary>
        /// Create a new DateTimeNano from a Unix nanosecond epoch.
        /// Use this constructor for DataBento timestamps.
        /// </summary>
        /// <param name="nanosecondsSinceEpoch">Nanoseconds since 1970-01-01 00:00:00 UTC.</param>
        public DateTimeNano(ulong nanosecondsSinceEpoch)
        {
            NanosecondsSinceEpoch = nanosecondsSinceEpoch;
        }

        /// <summary>
        /// Create a new DateTimeNano from its parts.
        /// These are not validated before they are used to construct a DateTime object internally,
        /// so the same rules apply here as for DateTime.
        /// Microseconds are added to the DateTime after construction.
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
        /// <param name="nanoseconds">nanosecond as int (0-999)</param>
        public DateTimeNano(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0,
            int microseconds = 0, int nanoseconds = 0)
        {
            var dateTime = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc).AddMicroseconds(microseconds);
            NanosecondsSinceEpoch = (ulong)(dateTime.Subtract(Epoch).Ticks * 100 + nanoseconds);
        }

        /// <summary>
        /// Add nanoseconds to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="nanoseconds">Nanoseconds to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   Thrown when the result would underflow below the Unix epoch or overflow the maximum representable value.
        /// </exception>
        public DateTimeNano AddNanoseconds(long nanoseconds)
        {
            if (nanoseconds < 0)
            {
                var absNs = (ulong)(-nanoseconds);
                if (absNs > NanosecondsSinceEpoch)
                    throw new ArgumentOutOfRangeException(nameof(nanoseconds), "Result would be before the Unix epoch.");
                return new DateTimeNano(NanosecondsSinceEpoch - absNs);
            }

            var result = NanosecondsSinceEpoch + (ulong)nanoseconds;
            if (result < NanosecondsSinceEpoch)
                throw new ArgumentOutOfRangeException(nameof(nanoseconds), "Result would overflow the maximum representable value.");
            return new DateTimeNano(result);
        }

        /// <summary>
        /// Add microseconds to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="microseconds">Microseconds to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddMicroseconds(long microseconds)
        {
            return AddNanoseconds(microseconds * 1000);
        }

        /// <summary>
        /// Add milliseconds to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="milliseconds">Milliseconds to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddMilliseconds(long milliseconds)
        {
            return AddNanoseconds(milliseconds * 1000_000);
        }

        /// <summary>
        /// Add seconds to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="seconds">Seconds to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddSeconds(long seconds)
        {
            return AddNanoseconds(seconds * 1000_000_000);
        }

        /// <summary>
        /// Add minutes to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="minutes">Minutes to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddMinutes(long minutes)
        {
            return AddNanoseconds(minutes * 60 * 1000_000_000);
        }

        /// <summary>
        /// Add hours to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="hours">Hours to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddHours(long hours)
        {
            return AddNanoseconds(hours * 60 * 60 * 1000_000_000);
        }

        /// <summary>
        /// Add days to the DateTimeNano and return a new DateTimeNano.
        /// </summary>
        /// <param name="days">Days to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddDays(int days)
        {
            return AddNanoseconds((long)days * 24 * 60 * 60 * 1_000_000_000);
        }

        /// <summary>
        /// Add months to the DateTimeNano and return a new DateTimeNano.
        /// Sub-microsecond nanoseconds are preserved.
        /// </summary>
        /// <param name="months">Months to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddMonths(int months)
        {
            return new DateTimeNano(DateTime.AddMonths(months)).AddNanoseconds(Nanoseconds);
        }

        /// <summary>
        /// Add years to the DateTimeNano and return a new DateTimeNano.
        /// Sub-microsecond nanoseconds are preserved.
        /// </summary>
        /// <param name="years">Years to add. May be negative to subtract.</param>
        /// <returns>A new DateTimeNano.</returns>
        public DateTimeNano AddYears(int years)
        {
            return new DateTimeNano(DateTime.AddYears(years)).AddNanoseconds(Nanoseconds);
        }

        /// <summary>
        /// Returns the difference between this instance and <paramref name="other"/> in nanoseconds.
        /// A positive result means this instance is later than <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The value to subtract.</param>
        /// <returns>Nanosecond difference as a <see cref="long"/>.</returns>
        /// <exception cref="OverflowException">
        ///   Thrown when the absolute difference exceeds <see cref="long.MaxValue"/> nanoseconds (~292 years).
        /// </exception>
        public long Subtract(DateTimeNano other)
        {
            if (NanosecondsSinceEpoch >= other.NanosecondsSinceEpoch)
            {
                var diff = NanosecondsSinceEpoch - other.NanosecondsSinceEpoch;
                if (diff > (ulong)long.MaxValue)
                    throw new OverflowException("The difference between the two DateTimeNano values is too large to fit in a long.");
                return (long)diff;
            }
            else
            {
                var diff = other.NanosecondsSinceEpoch - NanosecondsSinceEpoch;
                if (diff > (ulong)long.MaxValue + 1)
                    throw new OverflowException("The difference between the two DateTimeNano values is too large to fit in a long.");
                return -(long)diff;
            }
        }

        /// <summary>
        /// Returns the UTC DateTime portion of the DateTimeNano as a <see cref="System.DateTime"/> object.
        /// This will include microseconds, but not sub-microsecond nanoseconds.
        /// Sub-microsecond nanoseconds can be accessed via <see cref="Nanoseconds"/>.
        /// </summary>
        /// <returns>A UTC <see cref="System.DateTime"/>.</returns>
        public DateTime ToDateTimeUtc()
        {
            return DateTime;
        }

        /// <summary>
        /// Converts the UTC value of this instance to the local time in the specified time zone.
        /// The returned <see cref="System.DateTime"/> includes microsecond precision but not sub-microsecond nanoseconds.
        /// </summary>
        /// <param name="timeZone">The target time zone.</param>
        /// <returns>
        ///   A <see cref="System.DateTime"/> with <see cref="DateTimeKind.Unspecified"/> representing local time
        ///   in <paramref name="timeZone"/>.
        /// </returns>
        public DateTime ToDateTimeInTimeZone(TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(ToDateTimeUtc(), timeZone);
        }

        /// <summary>
        /// Returns the number of nanoseconds since the Unix epoch (1970-01-01 00:00:00 UTC).
        /// </summary>
        /// <returns>Nanoseconds since the Unix epoch.</returns>
        public ulong ToUnixNanoseconds()
        {
            return NanosecondsSinceEpoch;
        }

        /// <summary>
        /// Returns a string representation in the format "yyyy-MM-dd HH:mm:ss.fffffffff".
        /// </summary>
        /// <returns>Formatted string.</returns>
        public override string ToString()
        {
            return $"{DateTime:yyyy-MM-dd HH:mm:ss}.{SecondsFractionInNanoseconds:D9}";
        }

        /// <inheritdoc/>
        public bool Equals(DateTimeNano other) => NanosecondsSinceEpoch == other.NanosecondsSinceEpoch;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is DateTimeNano other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => NanosecondsSinceEpoch.GetHashCode();

        /// <inheritdoc/>
        public int CompareTo(DateTimeNano other) => NanosecondsSinceEpoch.CompareTo(other.NanosecondsSinceEpoch);

        /// <summary>Returns the nanosecond difference <c>left - right</c>.</summary>
        public static long operator -(DateTimeNano left, DateTimeNano right) => left.Subtract(right);

        /// <summary>Returns <see langword="true"/> if both instances represent the same nanosecond.</summary>
        public static bool operator ==(DateTimeNano left, DateTimeNano right) => left.Equals(right);

        /// <summary>Returns <see langword="true"/> if the instances represent different nanoseconds.</summary>
        public static bool operator !=(DateTimeNano left, DateTimeNano right) => !left.Equals(right);

        /// <summary>Returns <see langword="true"/> if <paramref name="left"/> is earlier than <paramref name="right"/>.</summary>
        public static bool operator <(DateTimeNano left, DateTimeNano right) => left.NanosecondsSinceEpoch < right.NanosecondsSinceEpoch;

        /// <summary>Returns <see langword="true"/> if <paramref name="left"/> is later than <paramref name="right"/>.</summary>
        public static bool operator >(DateTimeNano left, DateTimeNano right) => left.NanosecondsSinceEpoch > right.NanosecondsSinceEpoch;

        /// <summary>Returns <see langword="true"/> if <paramref name="left"/> is earlier than or equal to <paramref name="right"/>.</summary>
        public static bool operator <=(DateTimeNano left, DateTimeNano right) => left.NanosecondsSinceEpoch <= right.NanosecondsSinceEpoch;

        /// <summary>Returns <see langword="true"/> if <paramref name="left"/> is later than or equal to <paramref name="right"/>.</summary>
        public static bool operator >=(DateTimeNano left, DateTimeNano right) => left.NanosecondsSinceEpoch >= right.NanosecondsSinceEpoch;

        /// <summary>Implicitly converts a <see cref="DateTimeNano"/> to a UTC <see cref="System.DateTime"/>.</summary>
        public static implicit operator DateTime(DateTimeNano d) => d.ToDateTimeUtc();

        /// <summary>Implicitly converts a <see cref="System.DateTime"/> to a <see cref="DateTimeNano"/>.</summary>
        public static implicit operator DateTimeNano(DateTime d) => new DateTimeNano(d);
    }
}

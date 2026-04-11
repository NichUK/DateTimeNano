namespace DateTimeNano.Tests
{
    public class DateTimeNanoTests
    {
        public const ulong EpochTicks = 621355968000000000L;

        private Seerstone.DateTimeNano _baseDateTimeNano;
        private ulong _baseNanoseconds;

        [SetUp]
        public void Setup()
        {
            _baseNanoseconds = 1_700_000_000_000_000_000; // Example nanoseconds value (Epoch + ~53 years)
            _baseDateTimeNano = new Seerstone.DateTimeNano(_baseNanoseconds);
        }

        // ── Add* methods ─────────────────────────────────────────────────────────

        [Test]
        public void AddNanoseconds_ShouldIncreaseCorrectly()
        {
            const long nanosecondsToAdd = 500;
            var result = _baseDateTimeNano.AddNanoseconds(nanosecondsToAdd);

            Assert.That(_baseNanoseconds + (ulong)nanosecondsToAdd, Is.EqualTo(result.ToUnixNanoseconds()));
        }

        [Test]
        public void AddMicroseconds_ShouldIncreaseByThousandNanoseconds()
        {
            const long microsecondsToAdd = 1_000; // 1,000 microseconds = 1,000,000 nanoseconds
            var result = _baseDateTimeNano.AddMicroseconds(microsecondsToAdd);

            Assert.That(_baseNanoseconds + (ulong)(microsecondsToAdd * 1_000), Is.EqualTo(result.ToUnixNanoseconds()));
        }

        [Test]
        public void AddMilliseconds_ShouldIncreaseByMillionNanoseconds()
        {
            const long millisecondsToAdd = 2; // 2 milliseconds = 2,000,000 nanoseconds
            var result = _baseDateTimeNano.AddMilliseconds(millisecondsToAdd);

            Assert.That(_baseNanoseconds + (ulong)(millisecondsToAdd * 1_000_000), Is.EqualTo(result.ToUnixNanoseconds()));
        }

        [Test]
        public void AddSeconds_ShouldIncreaseByBillionNanoseconds()
        {
            const long secondsToAdd = 3; // 3 seconds = 3,000,000,000 nanoseconds
            var result = _baseDateTimeNano.AddSeconds(secondsToAdd);

            Assert.That(_baseNanoseconds + (ulong)(secondsToAdd * 1_000_000_000), Is.EqualTo(result.ToUnixNanoseconds()));
        }

        [Test]
        public void AddMinutes_ShouldIncreaseCorrectly()
        {
            const long minutesToAdd = 4; // 4 minutes = 4 * 60 * 1,000,000,000 nanoseconds
            var result = _baseDateTimeNano.AddMinutes(minutesToAdd);

            Assert.That(_baseNanoseconds + (ulong)(minutesToAdd * 60 * 1_000_000_000), Is.EqualTo(result.ToUnixNanoseconds()));
        }

        [Test]
        public void AddHours_ShouldIncreaseCorrectly()
        {
            const long hoursToAdd = 5; // 5 hours = 5 * 60 * 60 * 1,000,000,000 nanoseconds
            var result = _baseDateTimeNano.AddHours(hoursToAdd);

            Assert.That(_baseNanoseconds + (ulong)(hoursToAdd * 60 * 60 * 1_000_000_000), Is.EqualTo(result.ToUnixNanoseconds()));
        }

        [Test]
        public void AddDays_ShouldIncreaseCorrectly()
        {
            const int daysToAdd = 1; // 1 day = 24 * 60 * 60 * 1,000,000,000 nanoseconds
            var result = _baseDateTimeNano.AddDays(daysToAdd);

            const long expectedNanosecondsPerDay = (long)daysToAdd * 24 * 60 * 60 * 1_000_000_000;
            Assert.That(_baseNanoseconds + (ulong)expectedNanosecondsPerDay, Is.EqualTo(result.ToUnixNanoseconds()));
        }

        [Test]
        public void AddMonths_ShouldIncreaseByExpectedMonths()
        {
            const int monthsToAdd = 2;
            var expectedDateTime = _baseDateTimeNano.ToDateTimeUtc().AddMonths(monthsToAdd);
            var result = _baseDateTimeNano.AddMonths(monthsToAdd);

            Assert.That(expectedDateTime, Is.EqualTo(result.ToDateTimeUtc()));
        }

        [Test]
        public void AddYears_ShouldIncreaseByExpectedYears()
        {
            const int yearsToAdd = 3;
            var expectedDateTime = _baseDateTimeNano.ToDateTimeUtc().AddYears(yearsToAdd);
            var result = _baseDateTimeNano.AddYears(yearsToAdd);

            Assert.That(expectedDateTime, Is.EqualTo(result.ToDateTimeUtc()));
        }

        [Test]
        public void AddYears_ShouldPreserveSubMicrosecondNanoseconds()
        {
            var nano = new Seerstone.DateTimeNano(1_739_219_232_123_456_789UL);
            var result = nano.AddYears(1);

            Assert.That(result.Nanoseconds, Is.EqualTo(789));
        }

        [Test]
        public void AddNanoseconds_Negative_ShouldThrow_WhenResultBeforeEpoch()
        {
            var nano = new Seerstone.DateTimeNano(500UL);
            Assert.Throws<ArgumentOutOfRangeException>(() => nano.AddNanoseconds(-501));
        }

        // ── Conversions ───────────────────────────────────────────────────────────

        [Test]
        public void ToDateTimeUtc_ShouldReturnUtcKind()
        {
            var result = _baseDateTimeNano.ToDateTimeUtc();
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void ToDateTimeUtc_ShouldReturnCorrectDateTime()
        {
            // 1_700_000_000_000_000_000 ns => 2023-11-14 22:13:20.000000000 UTC
            var expected = new DateTime(2023, 11, 14, 22, 13, 20, DateTimeKind.Utc);
            Assert.That(_baseDateTimeNano.ToDateTimeUtc(), Is.EqualTo(expected));
        }

        [Test]
        public void ToUnixNanoseconds_ShouldReturnCorrectValue()
        {
            Assert.That(_baseNanoseconds, Is.EqualTo(_baseDateTimeNano.ToUnixNanoseconds()));
        }

        [Test]
        public void ToDateTimeInTimeZone_ShouldConvertCorrectly()
        {
            var utcNano = new Seerstone.DateTimeNano(new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc));

            // Try both Windows and IANA identifiers to support all platforms.
            TimeZoneInfo? eastern = null;
            foreach (var id in new[] { "Eastern Standard Time", "America/New_York" })
            {
                if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out eastern))
                    break;
            }

            if (eastern is null)
                Assert.Ignore("Eastern time zone not available on this platform.");


            var local = utcNano.ToDateTimeInTimeZone(eastern!);

            // Eastern Daylight Time is UTC-4 in summer
            Assert.That(local.Hour, Is.EqualTo(8));
            Assert.That(local.Minute, Is.EqualTo(0));
        }

        // ── ToString ──────────────────────────────────────────────────────────────

        [Test]
        public void ToString_ShouldReturnFormattedString()
        {
            var expectedFormat = $"{_baseDateTimeNano.ToDateTimeUtc():yyyy-MM-dd HH:mm:ss}.{_baseDateTimeNano.ToUnixNanoseconds() % 1_000_000_000:D9}";
            Assert.That(expectedFormat, Is.EqualTo(_baseDateTimeNano.ToString()));
        }

        // ── Construction ─────────────────────────────────────────────────────────

        [Test]
        public void ShouldCreateDateTimeNanoEpoch()
        {
            var datetime = new DateTime(1970, 01, 01);
            var dateTimeNano = new Seerstone.DateTimeNano(datetime);
            Assert.That(dateTimeNano.DateTime, Is.EqualTo(datetime));
            Assert.That(dateTimeNano.NanosecondsSinceEpoch, Is.EqualTo(0));
            Assert.That(dateTimeNano.TotalTicks, Is.EqualTo(EpochTicks));
        }

        [Test]
        public void ShouldCreateDateTimeNanoFromDateTime()
        {
            var datetime = new DateTime(2025, 02, 10, 20, 27, 12, 123, DateTimeKind.Utc);
            var dateTimeNano = new Seerstone.DateTimeNano(datetime);
            Assert.That(dateTimeNano.DateTime, Is.EqualTo(datetime));
            Assert.That(dateTimeNano.NanosecondsSinceEpoch, Is.EqualTo(1739219232123000000));
            Assert.That(dateTimeNano.Nanoseconds, Is.EqualTo(0));
        }

        [Test]
        public void ShouldCreateNewDateTimeWithNanosecondsFromNanoSeconds()
        {
            var nanoseconds = (UInt64)1739219232123456789L;
            var dateTimeWithNanoseconds = new Seerstone.DateTimeNano(nanoseconds);
            Assert.That(dateTimeWithNanoseconds.NanosecondsSinceEpoch, Is.EqualTo(nanoseconds));
            Assert.That(dateTimeWithNanoseconds.DateTime, Is.EqualTo(new DateTime(2025, 2, 10, 20, 27, 12, 123, DateTimeKind.Utc).AddMicroseconds(456)));
            Assert.That(dateTimeWithNanoseconds.Nanoseconds, Is.EqualTo(789));
            Assert.That(dateTimeWithNanoseconds.SecondsFractionInNanoseconds, Is.EqualTo(123456789));
        }

        [Test]
        public void ShouldCorrectlyFormatDateTimeNano()
        {
            var nanoseconds = (UInt64)1739219232123456789L;
            var dateTimeWithNanoseconds = new Seerstone.DateTimeNano(nanoseconds);
            Assert.That(dateTimeWithNanoseconds.ToString(), Is.EqualTo("2025-02-10 20:27:12.123456789"));
        }

        [TestCase("2025-02-10 20:27:12.123456789", 2025, 2, 10, 20, 27, 12, 123, 456, 789)]
        [TestCase("2025-02-10 20:27:12.123456", 2025, 2, 10, 20, 27, 12, 123, 456, 0)]
        [TestCase("2025-02-10 20:27:12.123", 2025, 2, 10, 20, 27, 12, 123, 0, 0)]
        [TestCase("2025-02-10 20:27:12", 2025, 2, 10, 20, 27, 12, 0, 0, 0)]
        public void ShouldParseDateTimeNano(string dateTimeString, int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond, int nanosecond)
        {
            var dateTimeNano = Seerstone.DateTimeNano.Parse(dateTimeString);
            Assert.That(dateTimeNano.DateTime, Is.EqualTo(new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc).AddMicroseconds(microsecond)));
            Assert.That(dateTimeNano.Nanoseconds, Is.EqualTo(nanosecond));
        }

        [TestCase("2025-02-10 20:27:12", 123456789, "2025-02-10 20:27:12.123456789")]
        [TestCase("2025-02-10 20:27:12.123456789", -123456789, "2025-02-10 20:27:12.000000000")]
        public void ShouldAddNanosecondsToDateTimeNano(string dateTimeString, long nanosecondsToAdd, string expectedTimeString)
        {
            var dateTimeNano = Seerstone.DateTimeNano.Parse(dateTimeString);
            var newDateTimeNano = dateTimeNano.AddNanoseconds(nanosecondsToAdd);
            Assert.That(newDateTimeNano.ToString(), Is.EqualTo(expectedTimeString));
        }

        // ── TryParse ──────────────────────────────────────────────────────────────

        [Test]
        public void TryParse_ShouldReturnTrue_ForValidString()
        {
            var success = Seerstone.DateTimeNano.TryParse("2025-02-10 20:27:12.123456789", out var result);
            Assert.That(success, Is.True);
            Assert.That(result.ToString(), Is.EqualTo("2025-02-10 20:27:12.123456789"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not-a-date")]
        public void TryParse_ShouldReturnFalse_ForInvalidStrings(string? input)
        {
            var success = Seerstone.DateTimeNano.TryParse(input, out var result);
            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(default(Seerstone.DateTimeNano)));
        }

        [Test]
        public void Parse_ShouldThrow_ForInvalidString()
        {
            Assert.Throws<ArgumentException>(() => Seerstone.DateTimeNano.Parse("not-a-date"));
        }

        // ── Equality and comparison ───────────────────────────────────────────────

        [Test]
        public void Equality_SameTick_ShouldBeEqual()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            var b = new Seerstone.DateTimeNano(1_000_000_000UL);
            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equality_DifferentTick_ShouldNotBeEqual()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            var b = new Seerstone.DateTimeNano(1_000_000_001UL);
            Assert.That(a != b, Is.True);
            Assert.That(a == b, Is.False);
        }

        [Test]
        public void ComparisonOperators_ShouldOrderCorrectly()
        {
            var earlier = new Seerstone.DateTimeNano(1_000_000_000UL);
            var later = new Seerstone.DateTimeNano(2_000_000_000UL);

            Assert.That(earlier < later, Is.True);
            Assert.That(earlier <= later, Is.True);
            Assert.That(later > earlier, Is.True);
            Assert.That(later >= earlier, Is.True);
            var same = new Seerstone.DateTimeNano(1_000_000_000UL);
            Assert.That(earlier <= same, Is.True);
            Assert.That(earlier >= same, Is.True);
        }

        [Test]
        public void CompareTo_ShouldReturnNegative_WhenEarlier()
        {
            var earlier = new Seerstone.DateTimeNano(1_000_000_000UL);
            var later = new Seerstone.DateTimeNano(2_000_000_000UL);
            Assert.That(earlier.CompareTo(later), Is.LessThan(0));
            Assert.That(later.CompareTo(earlier), Is.GreaterThan(0));
            Assert.That(earlier.CompareTo(earlier), Is.EqualTo(0));
        }

        // ── Subtract / - operator ─────────────────────────────────────────────────

        [Test]
        public void Subtract_ShouldReturnPositiveDifference()
        {
            var a = new Seerstone.DateTimeNano(2_000_000_000UL);
            var b = new Seerstone.DateTimeNano(1_000_000_000UL);
            Assert.That(a.Subtract(b), Is.EqualTo(1_000_000_000L));
            Assert.That(a - b, Is.EqualTo(1_000_000_000L));
        }

        [Test]
        public void Subtract_ShouldReturnNegativeDifference()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            var b = new Seerstone.DateTimeNano(2_000_000_000UL);
            Assert.That(a.Subtract(b), Is.EqualTo(-1_000_000_000L));
            Assert.That(a - b, Is.EqualTo(-1_000_000_000L));
        }

        [Test]
        public void Subtract_SameValue_ShouldReturnZero()
        {
            var a = new Seerstone.DateTimeNano(1_500_000_000UL);
            Assert.That(a.Subtract(a), Is.EqualTo(0L));
        }
    }
}

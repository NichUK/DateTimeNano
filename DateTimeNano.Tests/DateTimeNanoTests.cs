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

        // ── TimeSpan interop ──────────────────────────────────────────────────────

        [Test]
        public void Add_TimeSpan_Positive_ShouldShiftForward()
        {
            var ts = TimeSpan.FromSeconds(1);
            var result = _baseDateTimeNano.Add(ts);
            Assert.That(result.ToUnixNanoseconds(), Is.EqualTo(_baseNanoseconds + 1_000_000_000UL));
        }

        [Test]
        public void Add_TimeSpan_Negative_ShouldShiftBack()
        {
            var ts = TimeSpan.FromMilliseconds(-500);
            var result = _baseDateTimeNano.Add(ts);
            Assert.That(result.ToUnixNanoseconds(), Is.EqualTo(_baseNanoseconds - 500_000_000UL));
        }

        [Test]
        public void OperatorPlus_TimeSpan_ShouldShiftForward()
        {
            var ts = TimeSpan.FromMicroseconds(250);
            var result = _baseDateTimeNano + ts;
            Assert.That(result.ToUnixNanoseconds(), Is.EqualTo(_baseNanoseconds + 250_000UL));
        }

        [Test]
        public void OperatorMinus_TimeSpan_ShouldShiftBack()
        {
            var ts = TimeSpan.FromMicroseconds(250);
            var result = _baseDateTimeNano - ts;
            Assert.That(result.ToUnixNanoseconds(), Is.EqualTo(_baseNanoseconds - 250_000UL));
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
        public void ToUnixMilliseconds_ShouldReturnWholeMilliseconds()
        {
            // 1_500_000_000 ns = 1_500 ms (sub-millisecond truncated)
            var dt = new Seerstone.DateTimeNano(1_500_000_999UL);
            Assert.That(dt.ToUnixMilliseconds(), Is.EqualTo(1_500L));
        }

        [Test]
        public void ToUnixSeconds_ShouldReturnWholeSeconds()
        {
            // 3_000_000_000 ns = 3 s (sub-second truncated)
            var dt = new Seerstone.DateTimeNano(3_000_000_999UL);
            Assert.That(dt.ToUnixSeconds(), Is.EqualTo(3L));
        }

        [Test]
        public void FromUnixMilliseconds_ShouldRoundTrip()
        {
            var dt = new Seerstone.DateTimeNano(2_000_000_000UL); // 2 seconds exactly
            var ms = dt.ToUnixMilliseconds();
            var restored = Seerstone.DateTimeNano.FromUnixMilliseconds(ms);
            Assert.That(restored.NanosecondsSinceEpoch, Is.EqualTo(2_000_000_000UL));
        }

        [Test]
        public void FromUnixSeconds_ShouldRoundTrip()
        {
            var dt = new Seerstone.DateTimeNano(5_000_000_000UL); // 5 seconds exactly
            var secs = dt.ToUnixSeconds();
            var restored = Seerstone.DateTimeNano.FromUnixSeconds(secs);
            Assert.That(restored.NanosecondsSinceEpoch, Is.EqualTo(5_000_000_000UL));
        }

        [Test]
        public void FromUnixMilliseconds_Negative_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Seerstone.DateTimeNano.FromUnixMilliseconds(-1));
        }

        [Test]
        public void FromUnixSeconds_Negative_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Seerstone.DateTimeNano.FromUnixSeconds(-1));
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

        // ── Arithmetic operators ──────────────────────────────────────────────────

        [Test]
        public void PlusOperator_ShouldAddNanoseconds()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            var result = a + 500L;
            Assert.That(result.NanosecondsSinceEpoch, Is.EqualTo(1_000_000_500UL));
        }

        [Test]
        public void MinusOperator_Long_ShouldSubtractNanoseconds()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            var result = a - 500L;
            Assert.That(result.NanosecondsSinceEpoch, Is.EqualTo(999_999_500UL));
        }

        [Test]
        public void PlusOperator_NegativeValue_ShouldSubtract()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            var result = a + (-500L);
            Assert.That(result.NanosecondsSinceEpoch, Is.EqualTo(999_999_500UL));
        }

        [Test]
        public void MinusOperator_Long_ShouldThrow_WhenResultBeforeEpoch()
        {
            var a = new Seerstone.DateTimeNano(100UL);
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = a - 200L; });
        }

        // ── Static members ────────────────────────────────────────────────────────

        [Test]
        public void MinValue_ShouldBeEpoch()
        {
            Assert.That(Seerstone.DateTimeNano.MinValue.NanosecondsSinceEpoch, Is.EqualTo(0UL));
            Assert.That(Seerstone.DateTimeNano.MinValue.ToDateTimeUtc(), Is.EqualTo(Seerstone.DateTimeNano.Epoch));
        }

        [Test]
        public void MaxValue_ShouldBeUlongMaxValue()
        {
            Assert.That(Seerstone.DateTimeNano.MaxValue.NanosecondsSinceEpoch, Is.EqualTo(ulong.MaxValue));
        }

        [Test]
        public void Now_ShouldReturnRecentUtcTime()
        {
            var before = DateTime.UtcNow;
            var now = Seerstone.DateTimeNano.Now;
            var after = DateTime.UtcNow;

            Assert.That(now.ToDateTimeUtc(), Is.GreaterThanOrEqualTo(before));
            Assert.That(now.ToDateTimeUtc(), Is.LessThanOrEqualTo(after));
        }

        // ── Parts constructor ─────────────────────────────────────────────────────

        [Test]
        public void Constructor_Parts_ShouldCreateCorrectValue()
        {
            var dt = new Seerstone.DateTimeNano(2025, 2, 10, 20, 27, 12, 123, 456, 789);
            Assert.That(dt.NanosecondsSinceEpoch, Is.EqualTo(1739219232123456789UL));
        }

        [Test]
        public void Constructor_Parts_DefaultsToMidnight()
        {
            var dt = new Seerstone.DateTimeNano(2025, 1, 1);
            var expected = new Seerstone.DateTimeNano(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(dt.NanosecondsSinceEpoch, Is.EqualTo(expected.NanosecondsSinceEpoch));
        }

        [Test]
        public void Constructor_LocalDateTime_ShouldConvertToUtc()
        {
            var local = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);
            var utc = local.ToUniversalTime();
            var dtn = new Seerstone.DateTimeNano(local);
            var expected = new Seerstone.DateTimeNano(utc);
            Assert.That(dtn.NanosecondsSinceEpoch, Is.EqualTo(expected.NanosecondsSinceEpoch));
        }

        // ── Date property ─────────────────────────────────────────────────────────

        [Test]
        public void Date_Property_ShouldReturnMidnight()
        {
            var dt = new Seerstone.DateTimeNano(2025, 2, 10, 15, 30, 45, 123, 456, 789);
            var date = dt.Date;
            Assert.That(date, Is.EqualTo(new DateTime(2025, 2, 10, 0, 0, 0, DateTimeKind.Utc)));
        }

        // ── AddNanoseconds overflow guard ─────────────────────────────────────────

        [Test]
        public void AddNanoseconds_ShouldThrow_WhenPositiveOverflow()
        {
            var nano = Seerstone.DateTimeNano.MaxValue;
            Assert.Throws<ArgumentOutOfRangeException>(() => nano.AddNanoseconds(1));
        }

        // ── Negative Add* methods ─────────────────────────────────────────────────

        [Test]
        public void AddMonths_Negative_ShouldDecreaseCorrectly()
        {
            const int monthsToSubtract = 2;
            var expected = _baseDateTimeNano.ToDateTimeUtc().AddMonths(-monthsToSubtract);
            var result = _baseDateTimeNano.AddMonths(-monthsToSubtract);
            Assert.That(result.ToDateTimeUtc(), Is.EqualTo(expected));
        }

        [Test]
        public void AddYears_Negative_ShouldDecreaseCorrectly()
        {
            const int yearsToSubtract = 3;
            var expected = _baseDateTimeNano.ToDateTimeUtc().AddYears(-yearsToSubtract);
            var result = _baseDateTimeNano.AddYears(-yearsToSubtract);
            Assert.That(result.ToDateTimeUtc(), Is.EqualTo(expected));
        }

        // ── Equals(object?) override ──────────────────────────────────────────────

        [Test]
        public void Equals_Object_ShouldReturnTrue_ForEqualValue()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            object b = new Seerstone.DateTimeNano(1_000_000_000UL);
            Assert.That(a.Equals(b), Is.True);
        }

        [Test]
        public void Equals_Object_ShouldReturnFalse_ForNull()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            Assert.That(a.Equals(null), Is.False);
        }

        [Test]
        public void Equals_Object_ShouldReturnFalse_ForDifferentType()
        {
            var a = new Seerstone.DateTimeNano(1_000_000_000UL);
            Assert.That(a.Equals("not a DateTimeNano"), Is.False);
        }

        // ── Implicit conversion operators ─────────────────────────────────────────

        [Test]
        public void ImplicitConversion_DateTimeNano_To_DateTime()
        {
            Seerstone.DateTimeNano dtn = new Seerstone.DateTimeNano(1_739_219_232_123_456_000UL);
            DateTime dt = dtn;
            Assert.That(dt.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(dt, Is.EqualTo(dtn.ToDateTimeUtc()));
        }

        [Test]
        public void ImplicitConversion_DateTime_To_DateTimeNano()
        {
            var dt = new DateTime(2025, 2, 10, 20, 27, 12, 123, DateTimeKind.Utc);
            Seerstone.DateTimeNano dtn = dt;
            Assert.That(dtn.NanosecondsSinceEpoch, Is.EqualTo(1739219232123000000UL));
        }

        // ── TryParse: T separator (ISO 8601) ──────────────────────────────────────

        [Test]
        public void TryParse_WithTSeparator_ShouldSucceed()
        {
            var success = Seerstone.DateTimeNano.TryParse("2025-02-10T20:27:12.123456789", out var result);
            Assert.That(success, Is.True);
            Assert.That(result.ToString(), Is.EqualTo("2025-02-10 20:27:12.123456789"));
        }

        [Test]
        public void Parse_WithTSeparator_ShouldSucceed()
        {
            var result = Seerstone.DateTimeNano.Parse("2025-02-10T20:27:12.000000000");
            Assert.That(result.ToDateTimeUtc(), Is.EqualTo(new DateTime(2025, 2, 10, 20, 27, 12, DateTimeKind.Utc)));
        }

        // ── Subtract overflow ─────────────────────────────────────────────────────

        [Test]
        public void Subtract_ShouldThrow_WhenDifferenceExceedsLongMaxValue()
        {
            // ulong.MaxValue ns ≈ 584 years; a gap >292 years exceeds long.MaxValue
            var large = Seerstone.DateTimeNano.MaxValue;
            var zero = Seerstone.DateTimeNano.MinValue;
            Assert.Throws<OverflowException>(() => large.Subtract(zero));
        }
    }
}

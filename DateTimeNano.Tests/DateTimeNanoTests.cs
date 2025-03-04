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
            unchecked
            {
                Assert.That(_baseNanoseconds + (ulong)(daysToAdd * 24 * 60 * 60 * 1_000_000_000), Is.EqualTo(result.ToUnixNanoseconds()));
            }
            
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
        public void ToDateTimeUtc_ShouldReturnCorrectDateTime()
        {
            var expectedDateTime = _baseDateTimeNano.ToDateTimeUtc();

            Assert.That(expectedDateTime, Is.EqualTo(_baseDateTimeNano.ToDateTimeUtc()));
        }

        [Test]
        public void ToUnixNanoseconds_ShouldReturnCorrectValue()
        {
            Assert.That(_baseNanoseconds, Is.EqualTo(_baseDateTimeNano.ToUnixNanoseconds()));
        }

        [Test]
        public void ToString_ShouldReturnFormattedString()
        {
            var expectedFormat = $"{_baseDateTimeNano.ToDateTimeUtc():yyyy-MM-dd HH:mm:ss}.{_baseDateTimeNano.ToUnixNanoseconds() % 1_000_000_000:D9}";
            Assert.That(expectedFormat, Is.EqualTo(_baseDateTimeNano.ToString()));
        }

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
            var datetime = new DateTime(2025, 02, 10, 20, 27, 12, 123);
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
            Assert.That(dateTimeWithNanoseconds.DateTime, Is.EqualTo(new DateTime(2025, 2, 10, 20, 27, 12, 123).AddMicroseconds(456)));
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
            Assert.That(dateTimeNano.DateTime, Is.EqualTo(new DateTime(year, month, day, hour, minute, second, millisecond).AddMicroseconds(microsecond)));
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
    }
}

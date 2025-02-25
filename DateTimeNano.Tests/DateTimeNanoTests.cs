using Seerstone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBento.Tests.Utilities
{
    public class DateTimeNanoTests
    {
        public const UInt64 EpochTicks = 621355968000000000L;

        [Test]
        public void ShouldCreateDateTimeNanoEpoch()
        {
            var datetime = new DateTime(1970, 01, 01);
            var dateTimeNano = new DateTimeNano(datetime);
            Assert.That(dateTimeNano.DateTime, Is.EqualTo(datetime));
            Assert.That(dateTimeNano.NanosecondsSinceEpoch, Is.EqualTo(0));
            Assert.That(dateTimeNano.TotalTicks, Is.EqualTo(EpochTicks));
        }

        [Test]
        public void ShouldCreateDateTimeNanoFromDateTime()
        {
            var datetime = new DateTime(2025, 02, 10, 20, 27, 12, 123);
            var dateTimeNano = new DateTimeNano(datetime);
            Assert.That(dateTimeNano.DateTime, Is.EqualTo(datetime));
            Assert.That(dateTimeNano.NanosecondsSinceEpoch, Is.EqualTo(1739219232123000000));
            Assert.That(dateTimeNano.Nanoseconds, Is.EqualTo(0));
        }

        [Test]
        public void ShouldCreateNewDateTimeWithNanosecondsFromNanoSeconds()
        {
            var nanoseconds = (UInt64)1739219232123456789L;
            var dateTimeWithNanoseconds = new DateTimeNano(nanoseconds);
            Assert.That(dateTimeWithNanoseconds.NanosecondsSinceEpoch, Is.EqualTo(nanoseconds));
            Assert.That(dateTimeWithNanoseconds.DateTime, Is.EqualTo(new DateTime(2025, 2, 10, 20, 27, 12, 123).AddMicroseconds(456)));
            Assert.That(dateTimeWithNanoseconds.Nanoseconds, Is.EqualTo(789));
            Assert.That(dateTimeWithNanoseconds.SecondsFractionInNanoseconds, Is.EqualTo(123456789));
        }

        [Test]
        public void ShouldCorrectlyFormatDateTimeNano()
        {
            var nanoseconds = (UInt64)1739219232123456789L;
            var dateTimeWithNanoseconds = new DateTimeNano(nanoseconds);
            Assert.That(dateTimeWithNanoseconds.ToString(), Is.EqualTo("2025-02-10 20:27:12.123456789"));
        }

        [TestCase("2025-02-10 20:27:12.123456789", 2025, 2, 10, 20, 27, 12, 123, 456, 789)]
        [TestCase("2025-02-10 20:27:12.123456", 2025, 2, 10, 20, 27, 12, 123, 456, 0)]
        [TestCase("2025-02-10 20:27:12.123", 2025, 2, 10, 20, 27, 12, 123, 0, 0)]
        [TestCase("2025-02-10 20:27:12", 2025, 2, 10, 20, 27, 12, 0, 0, 0)]
        public void ShouldParseDateTimeNano(string dateTimeString, int year, int month, int day, int hour, int minute, int second, int millisecond, int microsecond, int nanosecond)
        {
            var dateTimeNano = DateTimeNano.Parse(dateTimeString);
            Assert.That(dateTimeNano.DateTime, Is.EqualTo(new DateTime(year, month, day, hour, minute, second, millisecond).AddMicroseconds(microsecond)));
            Assert.That(dateTimeNano.Nanoseconds, Is.EqualTo(nanosecond));
        }

        [TestCase("2025-02-10 20:27:12", 123456789, "2025-02-10 20:27:12.123456789")]
        [TestCase("2025-02-10 20:27:12.123456789", -123456789, "2025-02-10 20:27:12.000000000")]
        public void ShouldAddNanosecondsToDateTimeNano(string dateTimeString, long nanosecondsToAdd, string expectedTimeString)
        {
            var dateTimeNano = DateTimeNano.Parse(dateTimeString);
            var newDateTimeNano = dateTimeNano.AddNanoseconds(nanosecondsToAdd);
            Assert.That(newDateTimeNano.ToString(), Is.EqualTo(expectedTimeString));
        }
    }
}

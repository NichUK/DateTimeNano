# DateTimeNano

A **simple** library to handle date and time in nanosecond precision.

Originally written to handle Nanosecond timestamps in (DataBento)[https://www.databento.com] trading data, 
this library maintains the date and time as a ulong (unsigned long) representing the number of nanoseconds since the Unix epoch
(1970-01-01T00:00:00).

The date and time can be accessed as a `DateTime` object, for compatibility with the rest of the .NET ecosystem, but nanosecond 
precision is lost when doing so.

Minimal manipulation facilities are provided, such as adding time intervals (subtract by adding a negative number).

The library is written in C# and is available as a NuGet package.

PRs welcome for additional features, bug fixes, tests, etc.

## Getting Started

````csharp
var datetime = new DateTime(1970, 01, 01);
var dateTimeNano = new DateTimeNano(datetime);
Assert.That(dateTimeNano.DateTime, Is.EqualTo(datetime));
Assert.That(dateTimeNano.NanosecondsSinceEpoch, Is.EqualTo(0));
Assert.That(dateTimeNano.TotalTicks, Is.EqualTo(621355968000000000L));

dateTimeNano = new DateTimeNano(621355968000000000L);
Assert.That(dateTimeNano.DateTime, Is.EqualTo(datetime));
````
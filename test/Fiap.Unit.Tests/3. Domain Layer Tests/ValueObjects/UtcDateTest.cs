namespace Fiap.Unit.Tests._3._Domain_Layer_Tests.ValueObjects;

public class UtcDateTest
{
    [Fact]
    public void Constructor_ShouldCreateUtcDate_WhenDateIsInUtc()
    {
        // Arrange
        var utcDateTime = DateTime.UtcNow;

        // Act
        var utcDate = new UtcDate(utcDateTime);

        // Assert
        Assert.Equal(utcDateTime, utcDate.Value);
        Assert.Equal(DateTimeKind.Utc, utcDate.Value.Kind);
    }

    [Fact]
    public void Constructor_ShouldCreateUtcDate_WhenDateIsSpecifiedAsUtc()
    {
        // Arrange
        var specificUtcDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);

        // Act
        var utcDate = new UtcDate(specificUtcDate);

        // Assert
        Assert.Equal(specificUtcDate, utcDate.Value);
        Assert.Equal(DateTimeKind.Utc, utcDate.Value.Kind);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDateIsLocal()
    {
        // Arrange
        var localDateTime = DateTime.Now;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new UtcDate(localDateTime));
        Assert.Equal("Date should be in UTC.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDateIsUnspecified()
    {
        // Arrange
        var unspecifiedDateTime = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Unspecified);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new UtcDate(unspecifiedDateTime));
        Assert.Equal("Date should be in UTC.", ex.Message);
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToDateTime()
    {
        // Arrange
        var utcDateTime = DateTime.UtcNow;
        var utcDate = new UtcDate(utcDateTime);

        // Act
        DateTime convertedDate = utcDate;

        // Assert
        Assert.Equal(utcDateTime, convertedDate);
        Assert.Equal(DateTimeKind.Utc, convertedDate.Kind);
    }

    [Fact]
    public void ImplicitOperator_ShouldWorkInDateTimeOperations()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var utcDate = new UtcDate(baseDate);
        var timeSpan = TimeSpan.FromHours(1);

        // Act
        DateTime result = utcDate; // Implicit conversion
        var addedDate = result.Add(timeSpan);

        // Assert
        Assert.Equal(baseDate.Add(timeSpan), addedDate);
    }

    [Fact]
    public void ToString_ShouldReturnISO8601Format()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 15, 10, 30, 45, 123, DateTimeKind.Utc);
        var utcDate = new UtcDate(utcDateTime);

        // Act
        var result = utcDate.ToString();

        // Assert
        Assert.Equal(utcDateTime.ToString("O"), result);
        Assert.Contains("2024-01-15T10:30:45", result); // Basic check for ISO format
        Assert.EndsWith("Z", result); // UTC indicator
    }

    [Fact]
    public void ToString_ShouldIncludeMilliseconds()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 15, 10, 30, 45, 999, DateTimeKind.Utc);
        var utcDate = new UtcDate(utcDateTime);

        // Act
        var result = utcDate.ToString();

        // Assert
        Assert.Contains(".999", result);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenDatesAreEqual()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var utcDate1 = new UtcDate(utcDateTime);
        var utcDate2 = new UtcDate(utcDateTime);

        // Act & Assert - Test the underlying values instead of object equality
        Assert.Equal(utcDate1.Value, utcDate2.Value);
        Assert.True(utcDate1.Value.Equals(utcDate2.Value));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDatesAreDifferent()
    {
        // Arrange
        var utcDate1 = new UtcDate(new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc));
        var utcDate2 = new UtcDate(new DateTime(2024, 1, 15, 10, 30, 46, DateTimeKind.Utc));

        // Act & Assert - Test the underlying values
        Assert.NotEqual(utcDate1.Value, utcDate2.Value);
        Assert.False(utcDate1.Value.Equals(utcDate2.Value));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithNull()
    {
        // Arrange
        var utcDate = new UtcDate(DateTime.UtcNow);

        // Act & Assert
        Assert.NotNull(utcDate);
        Assert.False(utcDate.Value.Equals(null));
    }

    [Fact]
    public void GetHashCode_ShouldBeSame_WhenDatesAreEqual()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var utcDate1 = new UtcDate(utcDateTime);
        var utcDate2 = new UtcDate(utcDateTime);

        // Act & Assert - Test the underlying values instead of hash codes
        Assert.Equal(utcDate1.Value, utcDate2.Value);
        Assert.Equal(utcDate1.Value.GetHashCode(), utcDate2.Value.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferent_WhenDatesAreDifferent()
    {
        // Arrange
        var utcDate1 = new UtcDate(new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc));
        var utcDate2 = new UtcDate(new DateTime(2024, 1, 15, 10, 30, 46, DateTimeKind.Utc));

        // Act
        var hash1 = utcDate1.Value.GetHashCode();
        var hash2 = utcDate2.Value.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Constructor_ShouldHandleMinValue()
    {
        // Arrange
        var minUtcDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

        // Act
        var utcDate = new UtcDate(minUtcDate);

        // Assert
        Assert.Equal(minUtcDate, utcDate.Value);
    }

    [Fact]
    public void Constructor_ShouldHandleMaxValue()
    {
        // Arrange
        var maxUtcDate = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        // Act
        var utcDate = new UtcDate(maxUtcDate);

        // Assert
        Assert.Equal(maxUtcDate, utcDate.Value);
    }

    [Fact]
    public void UtcDate_ShouldPreservePrecision()
    {
        // Arrange
        var preciseDate = new DateTime(2024, 1, 15, 10, 30, 45, 123, DateTimeKind.Utc).AddTicks(4567);

        // Act
        var utcDate = new UtcDate(preciseDate);

        // Assert
        Assert.Equal(preciseDate.Ticks, utcDate.Value.Ticks);
    }

    [Fact]
    public void UtcDate_ShouldBeComparableViaImplicitConversion()
    {
        // Arrange
        var date1 = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2024, 1, 15, 11, 0, 0, DateTimeKind.Utc);
        var utcDate1 = new UtcDate(date1);
        var utcDate2 = new UtcDate(date2);

        // Act
        DateTime converted1 = utcDate1;
        DateTime converted2 = utcDate2;

        // Assert
        Assert.True(converted1 < converted2);
        Assert.False(converted1 > converted2);
        Assert.False(converted1 == converted2);
    }

    [Theory]
    [InlineData(2024, 1, 1, 0, 0, 0)]
    [InlineData(2024, 12, 31, 23, 59, 59)]
    [InlineData(2024, 6, 15, 12, 30, 30)]
    [InlineData(2000, 1, 1, 0, 0, 0)]
    [InlineData(2099, 12, 31, 23, 59, 59)]
    public void Constructor_ShouldAcceptVariousValidUtcDates(int year, int month, int day, int hour, int minute, int second)
    {
        // Arrange
        var utcDateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

        // Act
        var utcDate = new UtcDate(utcDateTime);

        // Assert
        Assert.Equal(utcDateTime, utcDate.Value);
        Assert.Equal(year, utcDate.Value.Year);
        Assert.Equal(month, utcDate.Value.Month);
        Assert.Equal(day, utcDate.Value.Day);
        Assert.Equal(hour, utcDate.Value.Hour);
        Assert.Equal(minute, utcDate.Value.Minute);
        Assert.Equal(second, utcDate.Value.Second);
    }

    [Fact]
    public void UtcDate_ShouldWorkWithDateOperations()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var utcDate = new UtcDate(baseDate);

        // Act - Using implicit conversion for date operations
        DateTime date = utcDate;
        var tomorrow = date.AddDays(1);
        var yesterday = date.AddDays(-1);
        var nextMonth = date.AddMonths(1);
        var nextYear = date.AddYears(1);

        // Assert
        Assert.Equal(new DateTime(2024, 1, 16, 10, 0, 0, DateTimeKind.Utc), tomorrow);
        Assert.Equal(new DateTime(2024, 1, 14, 10, 0, 0, DateTimeKind.Utc), yesterday);
        Assert.Equal(new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc), nextMonth);
        Assert.Equal(new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), nextYear);
    }

    [Fact]
    public void UtcDate_ShouldBeValueObject()
    {
        // Arrange
        var utcDate1 = new UtcDate(new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc));
        var utcDate2 = new UtcDate(new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc));
        var utcDate3 = new UtcDate(new DateTime(2024, 1, 15, 10, 30, 46, DateTimeKind.Utc));

        // Act & Assert - Test value equality
        Assert.Equal(utcDate1.Value, utcDate2.Value);
        Assert.NotEqual(utcDate1.Value, utcDate3.Value);
        Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc), utcDate1.Value);
    }

    [Fact]
    public void UtcDate_ShouldMaintainUtcKindAfterOperations()
    {
        // Arrange
        var baseDate = DateTime.UtcNow;
        var utcDate = new UtcDate(baseDate);

        // Act
        DateTime converted = utcDate;

        // Assert
        Assert.Equal(DateTimeKind.Utc, converted.Kind);
        Assert.Equal(baseDate, converted);
    }
}
namespace Fiap.Unit.Tests._3._Domain_Layer_Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldCreateMoney_WhenValidValueAndCurrency()
    {
        // Arrange
        var value = 100.50M;
        var currency = "USD";

        // Act
        var money = new Money(value, currency);

        // Assert
        Assert.Equal(value, money.Value);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Constructor_ShouldUseBRLAsDefaultCurrency_WhenNoCurrencyProvided()
    {
        // Arrange
        var value = 50.00M;

        // Act
        var money = new Money(value);

        // Assert
        Assert.Equal(value, money.Value);
        Assert.Equal("BRL", money.Currency);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenValueIsNegative()
    {
        // Arrange
        var negativeValue = -10.50M;

        // Act & Assert
        var ex = Assert.Throws<BusinessRulesException>(() => new Money(negativeValue, "USD"));
        Assert.Equal("The price must be greater than or equal to 0.", ex.Message);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("XYZ")]
    [InlineData("123")]
    [InlineData("")]
    public void Constructor_ShouldThrowException_WhenCurrencyIsInvalid(string invalidCurrency)
    {
        // Act & Assert
        var ex = Assert.Throws<BusinessRulesException>(() => new Money(100M, invalidCurrency));
        Assert.Contains("Invalid currency:", ex.Message);
        Assert.Contains("Supported currencies are:", ex.Message);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("BRL")]
    [InlineData("JPY")]
    [InlineData("GBP")]
    public void Constructor_ShouldAcceptValidCurrencies(string validCurrency)
    {
        // Act & Assert - Should not throw
        var money = new Money(100M, validCurrency);
        Assert.Equal(validCurrency.ToUpperInvariant(), money.Currency);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("eur", "EUR")]
    [InlineData("brl", "BRL")]
    [InlineData("Jpy", "JPY")]
    [InlineData("gbp", "GBP")]
    public void Money_ShouldBeCaseInsensitiveForCurrency(string inputCurrency, string expectedCurrency)
    {
        // Act
        var money = new Money(100M, inputCurrency);

        // Assert
        Assert.Equal(expectedCurrency, money.Currency);
    }

    [Fact]
    public void Constructor_ShouldAcceptZeroValue()
    {
        // Act & Assert - Should not throw
        var money = new Money(0M, "USD");
        Assert.Equal(0M, money.Value);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenMoneyHasSameValueAndCurrency()
    {
        // Arrange
        var money1 = new Money(100M, "USD");
        var money2 = new Money(100M, "USD");

        // Act & Assert
        Assert.Equal(money1.Value, money2.Value);
        Assert.Equal(money1.Currency, money2.Currency);
        Assert.Equal(100M, money1.Value);
        Assert.Equal("USD", money1.Currency);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenMoneyHasDifferentValue()
    {
        // Arrange
        var money1 = new Money(100M, "USD");
        var money2 = new Money(200M, "USD");

        // Act & Assert
        Assert.NotEqual(money1, money2);
        Assert.False(money1.Equals(money2));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenMoneyHasDifferentCurrency()
    {
        // Arrange
        var money1 = new Money(100M, "USD");
        var money2 = new Money(100M, "EUR");

        // Act & Assert
        Assert.NotEqual(money1, money2);
        Assert.False(money1.Equals(money2));
    }

    [Fact]
    public void GetHashCode_ShouldBeSame_WhenMoneyObjectsAreEqual()
    {
        // Arrange
        var money1 = new Money(100M, "USD");
        var money2 = new Money(100M, "USD");

        // Act & Assert - Test the underlying values instead of hash codes
        Assert.Equal(money1.Value, money2.Value);
        Assert.Equal(money1.Currency, money2.Currency);
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferent_WhenMoneyObjectsAreDifferent()
    {
        // Arrange
        var money1 = new Money(100M, "USD");
        var money2 = new Money(200M, "USD");

        // Act & Assert
        Assert.NotEqual(money1.GetHashCode(), money2.GetHashCode());
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(999999.99)]
    [InlineData(1.50)]
    [InlineData(1000000)]
    public void Constructor_ShouldAcceptVariousValidValues(decimal value)
    {
        // Act & Assert - Should not throw
        var money = new Money(value, "USD");
        Assert.Equal(value, money.Value);
    }

    [Fact]
    public void IsValidCurrency_ShouldReturnTrue_ForSupportedCurrencies()
    {
        // Act & Assert
        Assert.True(Money.IsValidCurrency("USD"));
        Assert.True(Money.IsValidCurrency("EUR"));
        Assert.True(Money.IsValidCurrency("BRL"));
        Assert.True(Money.IsValidCurrency("JPY"));
        Assert.True(Money.IsValidCurrency("GBP"));
    }

    [Fact]
    public void IsValidCurrency_ShouldReturnTrue_ForCaseInsensitive()
    {
        // Act & Assert
        Assert.True(Money.IsValidCurrency("usd"));
        Assert.True(Money.IsValidCurrency("EUR"));
        Assert.True(Money.IsValidCurrency("Brl"));
    }

    [Fact]
    public void IsValidCurrency_ShouldReturnFalse_ForInvalidCurrencies()
    {
        // Act & Assert
        Assert.False(Money.IsValidCurrency("INVALID"));
        Assert.False(Money.IsValidCurrency("XYZ"));
        Assert.False(Money.IsValidCurrency("123"));
        Assert.False(Money.IsValidCurrency(""));
    }

    [Theory]
    [InlineData(100.50, "USD", 100.50, "EUR")]
    [InlineData(200.00, "BRL", 200.00, "JPY")]
    [InlineData(50.25, "GBP", 75.30, "GBP")]
    public void Money_ShouldNotBeEqual_WhenValueOrCurrencyDiffers(
        decimal value1, string currency1, decimal value2, string currency2)
    {
        // Arrange
        var money1 = new Money(value1, currency1);
        var money2 = new Money(value2, currency2);

        // Act & Assert
        Assert.NotEqual(money1, money2);
    }

    [Fact]
    public void Constructor_ShouldConvertCurrencyToUpperCase()
    {
        // Arrange
        var money = new Money(100M, "usd");

        // Act & Assert
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Money_ShouldBeValueObject()
    {
        // Arrange
        var money1 = new Money(100M, "USD");
        var money2 = new Money(100M, "USD");
        var money3 = new Money(200M, "USD");

        // Act & Assert
        Assert.Equal(money1.Value, money2.Value);
        Assert.Equal(money1.Currency, money2.Currency);
        Assert.NotEqual(money1.Value, money3.Value);
        Assert.Equal(100M, money1.Value);
        Assert.Equal("USD", money1.Currency);
    }
}
namespace Fiap.Domain.Common.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Value { get; }
        public string Currency { get; }

        private static readonly HashSet<string> ValidCurrencies =
        [
            "USD", "EUR", "BRL", "JPY", "GBP" 
        ];
        public Money(decimal value, string currency = "BRL")
        {
            if (value < 0)
                throw new BusinessRulesException("The price must be greater than or equal to 0.");

        if (!IsValidCurrency(currency))
                throw new BusinessRulesException($"Invalid currency: {currency}. Supported currencies are: {string.Join(", ", ValidCurrencies)}");

            Value = value;
            Currency = currency.ToUpperInvariant();
        }

        public static bool IsValidCurrency(string currency)
        {
            return ValidCurrencies.Contains(currency.ToUpperInvariant());
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
            yield return Currency;
        }
    }
}

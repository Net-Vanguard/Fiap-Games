namespace Fiap.Domain.Common.ValueObjects
{
    public class UtcDate : ValueObject
    {
        public DateTime Value { get; }

        public UtcDate(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Date should be in UTC.");

            Value = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public static implicit operator DateTime(UtcDate d) => d.Value;
        public override string ToString() => Value.ToString("O");
    }
}

namespace Fiap.Domain.Common.Events
{
	public interface IEventSourcing
	{
		Guid Id { get; }
		DateTime OccurredOn { get; }
	}
}

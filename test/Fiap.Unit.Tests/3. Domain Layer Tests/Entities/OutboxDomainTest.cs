namespace Fiap.Unit.Tests._3._Domain_Layer_Tests.Entities;

public class OutboxDomainTest
{
    [Fact]
    public void OutboxDomainSuccess()
    {
        #region Arrange
        var game = new GameCreatedIntegrationEvent(1, "Name", "M", 10, 1);
		var mockOutbox = new OutboxMessage
        {
            Type = "object",
			Content = JsonSerializer.Serialize(game),
			OccuredOn = DateTime.UtcNow,
            ProcessedOn = DateTime.UtcNow
        };
        
        #endregion
        
        #region Act

        var mockOutboxDomainAct = new OutboxMessage(
            mockOutbox.Type,
			game,
            mockOutbox.OccuredOn,
            mockOutbox.ProcessedOn
        );
        
        #endregion
        
        #region Assert
        
        Assert.Equal(mockOutbox.Type, mockOutboxDomainAct.Type);
        Assert.Equal(mockOutbox.Content, mockOutboxDomainAct.Content);
        Assert.Equal(mockOutbox.OccuredOn, mockOutboxDomainAct.OccuredOn);
        Assert.Equal(mockOutbox.ProcessedOn, mockOutboxDomainAct.ProcessedOn);
        #endregion
    }
}
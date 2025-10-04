namespace Fiap.Infra.CrossCutting.Common.Elastic.Models;
public class PromotionDocument
{
    public int Id { get; set; }
    public decimal Discount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<int> GameIds { get; set; } = [];
    public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    public List<string> Tags { get; set; } = [];
}
namespace Helios.Application.Common.Events;

public class CreateProductReviewEvent
{
    public Guid ReviewId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Point { get; set; }
}

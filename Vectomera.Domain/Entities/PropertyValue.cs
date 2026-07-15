using Vectomera.Domain.Common;

namespace Vectomera.Domain.Entities;

public class PropertyValue : BaseEntity
{
    public string Value { get; set; } = string.Empty;

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;
}


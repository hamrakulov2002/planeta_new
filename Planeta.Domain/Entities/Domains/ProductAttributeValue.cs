namespace Planeta.Domain.Entities;

public class ProductAttributeValue
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;

    public int AttributeId { get; set; }
    public virtual Attribute Attribute { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}
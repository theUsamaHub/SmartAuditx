public abstract class BaseEntity
{
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// we will inherit it in other classes 
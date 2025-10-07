namespace ToDoBackend.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public int IsActive { get; set; } = 1;
    public int PermissionId { get; set; }

    // Navigation property
    public virtual Permission Permission { get; set; } = null!;
}

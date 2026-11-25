using Microsoft.AspNetCore.Identity;

namespace Portal.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = "";
    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
}

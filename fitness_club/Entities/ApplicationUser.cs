using Microsoft.AspNetCore.Identity;

namespace fitness_club.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0m;
    }
}

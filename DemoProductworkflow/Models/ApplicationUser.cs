using Microsoft.AspNetCore.Identity;

namespace DemoProductworkflow.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}

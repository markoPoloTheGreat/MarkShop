using System.ComponentModel.DataAnnotations;

namespace MarkShop.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Added Role property: "Admin" or "Customer"
        public string Role { get; set; } = "Customer";
    }
}

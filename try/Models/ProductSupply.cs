using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkShop.Models
{
    public class ProductSupply
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Quantity in Stock")]
        public int Quantity { get; set; }
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}

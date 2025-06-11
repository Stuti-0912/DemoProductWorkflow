using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoProductworkflow.Models
{
    public enum ProductStatus
    {
        Draft, Approver1, Approver2, Active, Inactive, Rejected
    }
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        [Required]
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

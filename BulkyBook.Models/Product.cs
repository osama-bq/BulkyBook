using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BulkyBook.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        [Display(Name = "List Price")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1 and 1000.")]
        public double ListPrice { get; set; }

        [Display(Name = "Price for 1-50")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1 and 1000.")]
        public double Price { get; set; }

        [Display(Name = "Price for 50+")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1 and 1000.")]
        public double Price50 { get; set; }

        [Display(Name = "Price for 100+")]
        [Range(1, 1000, ErrorMessage = "Price must be between 1 and 1000.")]
        public double Price100 { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [ValidateNever]
        [Display(Name = "Product Image")]
        public string? ImageUrl { get; set; } = string.Empty;
    }
}
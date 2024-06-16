using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace OsepMVC.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; }

        public IFormFile? Image { get; set; } 
        public string ImagePath { get; set; }
        public decimal? Price { get; set; }
        public string Address { get; set; }
    }
}

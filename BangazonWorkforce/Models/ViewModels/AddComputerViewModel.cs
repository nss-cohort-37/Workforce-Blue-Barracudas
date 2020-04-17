using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class AddComputerViewModel
    {
        public int ComputerId { get; set; }

        [Display(Name = "Computer Model")]
        public string Model { get; set; }

        [Display (Name ="Computer Manufacturer")]
        public string Make { get; set; }

        [Display(Name = "Purchase Date")]
        [Required]
        public DateTime PurchaseDate { get; set; }

        public DateTime DecomissionDate { get; set; }

        public int? EmployeeId { get; set; }


        public List<SelectListItem> EmployeeOptions { get; set; }
    }
}

﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class EmployeeEditViewModel
    {
        public int EmployeeId { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public int DepartmentId { get; set; }
        public string Email { get; set; }
        public bool IsSupervisor { get; set; }
        public int? ComputerId { get; set; }
        public List<SelectListItem> DepartmentOptions { get; set; }
        public List<SelectListItem> ComputerOptions { get; set; }
    }
}

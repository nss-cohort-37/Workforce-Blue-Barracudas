using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class DepartmentListViewModel
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }

        [Display(Name = "Size of Department")]
        public int EmployeeCount { get; set; }
    }
}

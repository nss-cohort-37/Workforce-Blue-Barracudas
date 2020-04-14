using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class TrainingProgramDetailsViewModel
    {
        public int TrainingProgramId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxAttendees { get; set; }
        public List<Employee> Employees { get; set; }

    }
}

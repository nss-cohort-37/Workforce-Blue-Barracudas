using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonWorkforce.Models;

<<<<<<< HEAD
<<<<<<< HEAD
namespace BangazonWorkforce.Models
=======
namespace BangazonWorforce.Models
>>>>>>> 1fd05da3b8631ab9bc66a7f7a0ce39e8d81920a6
=======
namespace BangazonWorkforce.Models
>>>>>>> 4b9a774f1e9ee568a710860f4adbeffe457f13f9
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<Product> Products { get; set; }
    }
}

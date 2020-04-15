
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

<<<<<<< HEAD
namespace BangazonWorkforce.Models
=======
namespace BangazonWorforce.Models
>>>>>>> 1fd05da3b8631ab9bc66a7f7a0ce39e8d81920a6
{
    public class Product
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        //CustomerId is the seller
        public int CustomerId { get; set; }
        public int ProductTypeId { get; set; }
        public DateTime DateAdded { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BangazonWorkforce.Models

{
    public class CustomerProduct
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        public int ProductId { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BangazonWorkforce.Models

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

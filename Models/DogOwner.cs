using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoggyDistance.Models
{
    public class DogOwner
    {
        public int Id { get; set; }
        public string OwnerName { get; set; }
        public string OwnerAddress { get; set; }
        public int NeighborhoodId { get; set; }

        public string PhoneNumber { get; set; }

        //public List<Dog> DogsByOwner { get; set; }

    }
}
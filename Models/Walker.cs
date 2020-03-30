using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoggyDistance.Models
{
    public class Walker
    {
        public int Id { get; set; }
        public string WalkerName { get; set; }
        public int NeighborhoodId { get; set; }

        //public List<Walks> DogWalks { get; set; }
    }
}

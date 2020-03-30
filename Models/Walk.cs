using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoggyDistance.Models
{
    public class Walk
    { 
        public int Id { get; set; }

        public int WalkDuration { get; set; }

        public DateTime  Date {get; set; }

        public int DogId { get; set;  }

        public int WalkerId { get; set; }

    }
}

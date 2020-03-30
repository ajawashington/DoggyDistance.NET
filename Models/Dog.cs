using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoggyDistance.Models
{
    public class Dog
    {
        public int Id { get; set; }
        public string DogName { get; set; }
        public string Breed { get; set; }
        public int DogOwnerId { get; set; }
        public string Notes { get; set; }
    }
}


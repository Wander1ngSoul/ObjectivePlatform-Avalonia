using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class LandDemands
    {
        public int Id { get; set; }
        public int DemandId { get; set; }
        public Demands Demand { get; set; } = null!;

        public double MinArea { get; set; }
        public double MaxArea { get; set; }
    }

}

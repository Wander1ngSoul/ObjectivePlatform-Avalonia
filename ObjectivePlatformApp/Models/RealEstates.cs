using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class RealEstates
    {
        public int Id { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public int? House { get; set; }
        public int? Flat { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? Floor { get; set; }
        public int? Rooms { get; set; }
        public double? Area { get; set; }

        public int RealEstateTypeId { get; set; }
        public RealEstateType RealEstateType { get; set; } = null!;

        public int? DistrictId { get; set; }
        public Districts? District { get; set; }
    }

}

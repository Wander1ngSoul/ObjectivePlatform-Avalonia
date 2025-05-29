using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class Deal
    {
        public int Id { get; set; }

        public int DemandId { get; set; }
        public Demands Demand { get; set; } = null!;

        public int OfferId { get; set; }
        public Offers Offer { get; set; } = null!;

        public double CompanyDeductions { get; set; }
        public double BuyerAgentDeductions { get; set; }
        public double SellerAgentDeductions { get; set; }
    }
}
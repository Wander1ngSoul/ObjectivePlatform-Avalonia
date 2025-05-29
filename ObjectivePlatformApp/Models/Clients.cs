using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class Clients
    {
        public int Id { get; set; }

        [Required] public string FirstName { get; set; } = "";
        [Required] public string MiddleName { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}

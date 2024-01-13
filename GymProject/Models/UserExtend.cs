using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace GymProject.Models
{
    public partial class User
    {
        [NotMapped]
        public Claim[] Claims { get; set; }
        [NotMapped]
        public string Token { get; set; }

    }
}

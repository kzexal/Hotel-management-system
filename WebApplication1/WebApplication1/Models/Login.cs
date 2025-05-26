using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Login", Schema = "Authentication")]
    public class Login
    {
        [Key]
        public int LoginId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string NewUser { get; set; }

        public int TypeAccount { get; set; }
    }
}

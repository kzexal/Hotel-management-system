using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

[Table("Guests", Schema = "Hotel")]
public class Guest
{
    [Key]
    public string GuestId { get; set; }

    [Required]
    [StringLength(50)]
    public string GuestFirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string GuestLastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(50)]
    public string GuestEmailAddress { get; set; }

    [Required]
    [Phone]
    [StringLength(15)]
    public string GuestContactNumber { get; set; }

    [Required]
    [StringLength(50)]
    public string Street { get; set; }

    [Required]
    [StringLength(20)]
    public string City { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; }

    public int? UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual Login Login { get; set; }
}
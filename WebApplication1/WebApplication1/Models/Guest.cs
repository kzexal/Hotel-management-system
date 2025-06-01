using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
[Table("Guests", Schema = "Hotels")]
public class Guest
     
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GuestId { get; set; }

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
    public string Zip { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; }
}

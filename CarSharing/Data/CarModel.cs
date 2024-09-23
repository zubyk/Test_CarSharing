using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarSharing.Data
{
    [Table("Cars")]
    internal class CarModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DateTime Date { get; set; }

        [Required(AllowEmptyStrings = false)]
        public String Name { get; set; } = string.Empty;
    }
}

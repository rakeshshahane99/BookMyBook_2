using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookMyBook_Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage="Dislpay order must be between 1 to 100!")]
        public int DisplayOrder { get; set; }
        public DateTime CreatedDatetime { get; set; } = DateTime.Now;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DbModel.Entities
{
    [Table("Accounts", Schema = "dbo")]
    public class Account
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
    }
}

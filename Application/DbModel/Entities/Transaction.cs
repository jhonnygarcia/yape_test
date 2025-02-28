using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DbModel.Entities
{
    public enum TransactionStatusEnum : int
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum TransferTypeEnum : int
    {
        Deposit = 1,
        Withdrawal = 2
    }

    [Table("Transactions", Schema = "dbo")]
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public TransferTypeEnum TransferType { get; set; }
        public decimal Value { get; set; }
        public TransactionStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Note { get; set; }
        public virtual Account SourceAccount { get; set; }
        public virtual Account TargetAccount { get; set; }
    }

}

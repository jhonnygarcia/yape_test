namespace Application.Features.Accounts.Queries.GetAll
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
    }
}

using Payment.API.Data.Repositories;

namespace Payment.API.Features.DepositFunds
{
    public class DepositFundsHandler : IRequestHandler<DepositFundsCommand, DepositFundsResult>
    {
        private readonly IPendingDepositRepository _pendingDepositRepository;
        private readonly IConfiguration _configuration;

        public DepositFundsHandler(
            IPendingDepositRepository pendingDepositRepository,
            IConfiguration configuration)
        {
            _pendingDepositRepository = pendingDepositRepository;
            _configuration = configuration;
        }

        public async Task<DepositFundsResult> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            // Generate a unique code for this deposit in format ORD + numbers
            string depositCode = GenerateUniqueCode();

            // Create a pending deposit record
            var pendingDeposit = new PendingDeposit
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Amount = request.Amount,
                Code = depositCode,
                Description = request.Description ?? "Deposit funds",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _pendingDepositRepository.AddAsync(pendingDeposit, cancellationToken);

            // Get bank information from configuration
            var bankInfo = _configuration.GetSection("Sepay:BankInfo").Value ??
                "Please transfer to our bank account with the exact amount and include the code in the transfer description.";

            return new DepositFundsResult
            {
                DepositId = pendingDeposit.Id,
                DepositCode = depositCode,
                Amount = request.Amount,
                BankInfo = bankInfo
            };
        }

        private string GenerateUniqueCode()
        {
            // Generate a code in the format ORD + 10 random digits
            string randomDigits = new Random().Next(1000000000, 2147483647).ToString();
            return $"ORD{randomDigits}";
        }
    }
}
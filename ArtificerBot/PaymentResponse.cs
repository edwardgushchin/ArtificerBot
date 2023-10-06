using Artificer.Utility;

namespace Artificer.Bot;

public class PaymentResponse
{
    public PaymentResponse(PaymentStatus status, string? type = null, string? confirmationUrl = null, double? amount = null)
    {
        Status = status;
        Amount = amount;
        Type = type;
        ConfirmationUrl = confirmationUrl;
    }
    
    public PaymentStatus Status { get; }
    public double? Amount { get; }
    
    public string? Type { get; }
    
    public string? ConfirmationUrl { get; }
}
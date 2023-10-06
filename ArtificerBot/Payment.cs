using Artificer.Utility;

namespace Artificer.Bot;

public class Payment
{
    private PaymentStatus _status;
    public Payment(string id, long userId, double amount, PaymentStatus status, DateTime datetime)
    {
        ID = id;
        UserId = userId;
        Amount = amount;
        _status = status;
        DateTime = datetime;
    }
    
    public string ID { get; }
    
    public long UserId { get; }
    
    public double Amount { get; }

    public PaymentStatus Status
    {
        get => _status;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetPaymentStatus(ID, value);
            #pragma warning restore CS4014
            _status = value;
        }
    }
    
    public DateTime DateTime { get; }
}
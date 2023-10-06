using System.Text;
using Artificer.Utility;
using Newtonsoft.Json.Linq;

namespace Artificer.Bot;

public class Payments
{
    private readonly List<Payment> _payments;
    private Thread? _thread;

    public Payments()
    {
        Debug.Log("Initializing an Payments Subsystem Instance...", Debug.Sender.Payments, Debug.MessageStatus.INFO);
        _payments = Kernel.Database.GetAllPayments();
        _thread = new Thread(Start);
        _thread.Start();
        Debug.Log($"The Payments Subsystem instance initialized successfully. {_payments.Count} payments loaded.", Debug.Sender.Payments, Debug.MessageStatus.INFO);
    }

    public void Add(string id, long userId, double amount, PaymentStatus status)
    {
        _thread = null;
        var payment = new Payment(id, userId, amount, status, DateTime.Now);
        #pragma warning disable CS4014
        Kernel.Database.AddPaymentAsync(payment.ID, payment.UserId, payment.Amount, payment.Status, payment.DateTime);
        #pragma warning restore CS4014
        _payments.Add(payment);
    }

    private void Start()
    {
        while (true)
        {
            try
            {
                var copyPayments = new List<Payment>(_payments);
                var removePayments = new List<Payment>();
                foreach (var payment in copyPayments)
                {
                    removePayments.Clear();
                    switch (payment.Status)
                    {
                        case PaymentStatus.Canceled:
                        case PaymentStatus.Succeeded:
                        case PaymentStatus.Pending:
                        {
                            var timeSpan = DateTime.Now - payment.DateTime;
                            if (timeSpan.Days > 30)
                                removePayments.Add(payment);
                            break;
                        }
                        case PaymentStatus.Undefined:
                            break;
                        case PaymentStatus.Error:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var p = GetStatus(payment.ID).GetAwaiter().GetResult();
                    if(payment.Status == PaymentStatus.Succeeded) continue;
                    switch (p.Status)
                    {
                        case PaymentStatus.Canceled:
                            payment.Status = PaymentStatus.Canceled;
                            break;
                        case PaymentStatus.Succeeded:
                        {
                            payment.Status = PaymentStatus.Succeeded;
                            var user = Kernel.Users.GetUserFromChatId(payment.UserId);
                            user!.Balance += payment.Amount;
                            
                            var msg = $"✅ Ваш счет успешно пополнен на {p.Amount}₽";
                            Kernel.SendMessageFromUserId(payment.UserId, msg).GetAwaiter().GetResult();
                            Debug.Log($"[Message] {msg}", user.ID);
                            Debug.Log($"User id{user.ID} received a payment in the amount of {payment.Amount} rubles.", Debug.Sender.Payments, Debug.MessageStatus.INFO);
                            Kernel.Database.AddAction(user.ID, ActionType.UserPayment).GetAwaiter().GetResult();
                            break;
                        }
                        case PaymentStatus.Undefined:
                            break;
                        case PaymentStatus.Pending:
                            break;
                        case PaymentStatus.Error:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                removePayments.ForEach(payment => _payments.Remove(payment));
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            catch (Exception e)
            {
                Debug.Log($"[Start] {e.Message} {e.StackTrace}", Debug.Sender.Payments, Debug.MessageStatus.FAIL);
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static async Task<PaymentResponse> GetStatus(string id)
    {
        try
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.yookassa.ru/v3/payments/{id}");

            request.Headers.Add("Authorization",
                "Basic " + Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{Kernel.Settings.ShopId}:{Kernel.Settings.YMAccessToken}")));

            var response = await httpClient.SendAsync(request);
            var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            switch (jsonResponse["status"].ToString())
            {
                case "canceled":
                    return new PaymentResponse(PaymentStatus.Canceled);
                case "pending":
                    return new PaymentResponse(PaymentStatus.Pending);
                case "succeeded":
                {
                    var amount = double.Parse(jsonResponse["amount"]["value"].ToString().Replace('.', ','));
                    return new PaymentResponse(PaymentStatus.Succeeded, amount: amount);
                }
                default:
                    return new PaymentResponse(PaymentStatus.Undefined);
            }
        }
        catch (Exception)
        {
            return new PaymentResponse(PaymentStatus.Undefined);
        }
    }
}
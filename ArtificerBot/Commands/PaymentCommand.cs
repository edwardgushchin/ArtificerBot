using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Commands;

public static class PaymentCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, string? message, CancellationToken cancellationToken)
    {
        var isNumber = int.TryParse(message, out var numericValue);
        if (isNumber)
        {
            Debug.Log($"[COMMAND] PaymentCommand {message}р.", user.ID);
            if (numericValue is >= 10 and < int.MaxValue)
            {
                var response = await Kernel.GetYKassaResponse(user, numericValue);

                if (response.Status == PaymentStatus.Error)
                {
                    var msg = Kernel.Messages.Get("payment_error");
                    Debug.Log($"[MESSAGE] {msg}", user.ID);
                    await botClient.SendTextMessageAsync(user.ID, msg, cancellationToken: cancellationToken);
                }

                if (response.Type == "redirect")
                {
                    var msg = Kernel.Messages.Get("payment_command");
                    Debug.Log($"[MESSAGE] {msg}", user.ID);
                    await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: PaymentKeyboard.GetKeyboard(response.ConfirmationUrl!), cancellationToken: cancellationToken);
                }
                
                var msg2 = "Главное меню:";
                Debug.Log($"[MESSAGE] {msg2}", user.ID);
                await botClient.SendTextMessageAsync(user.ID, msg2, replyMarkup: UserKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
                user.Stage = UserStage.Ready;
            }
            else
            {
                var msg2 = Kernel.Messages.Get("payment_lenght");
                Debug.Log($"[MESSAGE] {msg2}", user.ID);
                await botClient.SendTextMessageAsync(user.ID, msg2, cancellationToken: cancellationToken);
            }
            
            
            await Kernel.Database.AddAction(user.ID, ActionType.PaymentCommand);
        }
        else
        {
            var msg2 = Kernel.Messages.Get("payment_validation").Replace("{%MESSAGE%}", message);
            await botClient.SendTextMessageAsync(user.ID, msg2, cancellationToken: cancellationToken);
            Debug.Log($"[MESSAGE] {msg2}", user.ID);
            
            await Kernel.Database.AddAction(user.ID, ActionType.PaymentCommand);
        }
    }
}
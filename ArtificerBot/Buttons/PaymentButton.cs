using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Bot.Buttons;

public static class PaymentButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("payment_button");
        await botClient.SendTextMessageAsync(user.ID, msg, disableWebPagePreview: true, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
        
        user.Stage = UserStage.Paymant;
        
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.BALANCE_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.PaymentButton);
    }
}
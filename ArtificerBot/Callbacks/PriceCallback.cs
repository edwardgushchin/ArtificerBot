using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Artificer.Bot.Callbacks;

public static class PriceCallback
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, int messageId, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("price");
        await botClient.EditMessageTextAsync(new ChatId(user.ID), messageId, msg, ParseMode.Markdown,
            replyMarkup: RegistrationKeyboard.GetKeyboard(messageId), cancellationToken: cancellationToken);
        
        Debug.Log($"[CALLBACK] PriceCallback", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.PriceCallback);
    }
}
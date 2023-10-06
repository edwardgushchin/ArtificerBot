using Artificer.Utility;
using Telegram.Bot;
using Artificer.Utility.Keyboards;
using Telegram.Bot.Types;

namespace Artificer.Bot.Callbacks;

public static class UserCallback
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, int messageId, CancellationToken cancellationToken)
    {
        var message = Kernel.Messages.Get("telephone");
        await botClient.DeleteMessageAsync(user.ID, messageId, cancellationToken: cancellationToken);
        await botClient.SendTextMessageAsync(user.ID, message, replyMarkup: GetContactKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
        
        Debug.Log($"[CALLBACK] UserCallback", user.ID);
        Debug.Log($"[MESSAGE] {message}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.UserCallback);
    }
}
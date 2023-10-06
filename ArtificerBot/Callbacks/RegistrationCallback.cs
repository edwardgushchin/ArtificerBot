using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
namespace Artificer.Bot.Callbacks;

public static class RegistrationCallback
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, int messageId, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("user_or_organization");
        await botClient.EditMessageTextAsync(new ChatId(user.ID), messageId, msg,
            replyMarkup: UserOrOrganizationKeyboard.GetKeyboard(messageId), cancellationToken: cancellationToken);
        
        Debug.Log($"[CALLBACK] RegistrationCallback", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.RegistrationCallback);
    }
    
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var message = Kernel.Messages.Get("user_or_organization");
        var msg = await botClient.SendTextMessageAsync(user.ID, message, cancellationToken: cancellationToken);
        await botClient.EditMessageReplyMarkupAsync(new ChatId(user.ID), msg.MessageId,
            replyMarkup: UserOrOrganizationKeyboard.GetKeyboard(msg.MessageId), cancellationToken: cancellationToken);
        
        Debug.Log($"[CALLBACK] RegistrationCallback", user.ID);
        Debug.Log($"[MESSAGE] {message}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.RegistrationCallback);
    }
}
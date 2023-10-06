using Artificer.Utility;
using Telegram.Bot;
using Artificer.Utility.Keyboards;
using Telegram.Bot.Types;

namespace Artificer.Bot.Callbacks;

public static class OrganizationCallback
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, int messageId, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("organization_type");
        await botClient.EditMessageTextAsync(new ChatId(user.ID), messageId, msg, 
            replyMarkup: ContractOrBalanceKeyboard.GetKeyboard(messageId), cancellationToken: cancellationToken);
        
        var organization = Kernel.Organizations.Create(user.ID);
        user.Organization = organization.Manager;
        
        Debug.Log($"[CALLBACK] OrganizationCallback", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.OrganizationCallback);
    }
}
using Artificer.Utility;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Artificer.Bot.Callbacks;

public static class BalanceCallback
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, int messageId, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("organization_name");
        await botClient.EditMessageTextAsync(new ChatId(user.ID), messageId, msg, cancellationToken: cancellationToken);
        
        var organization = Kernel.Organizations.GetOrganizationFromManager(user.ID);
        organization!.Type = OrganizationType.Balance;
        organization.Stage = OrganizationStage.SetName;
        
        Debug.Log($"[CALLBACK] BalanceCallback", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.BalanceCallback);
    }
}
using Artificer.Utility;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Artificer.Bot.Callbacks;

public static class ContractCallback
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, int messageId, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("organization_name");
        await botClient.EditMessageTextAsync(new ChatId(user.ID), messageId, Kernel.Messages.Get("organization_name"), cancellationToken: cancellationToken);
        
        var organization = Kernel.Organizations.GetOrganizationFromManager(user.ID);
        organization!.Type = OrganizationType.Contract;
        organization.Stage = OrganizationStage.SetName;
        
        Debug.Log($"[CALLBACK] ContractCallback", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.ContractCallback);
    }
}
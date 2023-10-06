using Artificer.Utility;
using Telegram.Bot;
using Artificer.Utility.Keyboards;

namespace Artificer.Bot.Handlers;

public static class OrganizationNameValidation
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, Organization organization, string message, CancellationToken cancellationToken)
    {
        switch (organization.Type)
        {
            case OrganizationType.Contract:
                await botClient.SendTextMessageAsync(user.ID, Kernel.Messages.Get("organization_contract_contact"), replyMarkup: GetContactKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
                break;
            case OrganizationType.Balance:
                await botClient.SendTextMessageAsync(user.ID, Kernel.Messages.Get("organization_balance_contact"), replyMarkup: GetContactKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
                break;
            case OrganizationType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        organization.Name = message;
        Debug.Log($"[OrganizationNameValidation] {message}", user.ID);
        await Kernel.Database.AddAction(user.ID, ActionType.OrganizationNameValidation);
    }
}
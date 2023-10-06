using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class StatusButton
{
    public static async Task HandlerAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var balance = $"Баланс: {user.Balance:0.00}₽";
        var msg = balance;
        
        if(user.Organization == null)
            await botClient.SendTextMessageAsync(user.ID, balance, disableWebPagePreview: true,
                cancellationToken: cancellationToken);
        else
        {
            var organization = Kernel.Organizations.GetOrganizationFromManager(user.ID);
            if (organization!.Type == OrganizationType.Balance)
            {
                await botClient.SendTextMessageAsync(user.ID, balance, disableWebPagePreview: true,
                    cancellationToken: cancellationToken);
            }
            else await DevelopButton.HandleAsync(botClient, user, cancellationToken);
        }
        
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.STATUS_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);

        await Kernel.Database.AddAction(user.ID, ActionType.StatusButton);
    }
}
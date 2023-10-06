using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class SettingsButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("settings_button");

        if (user.Trial)
        {
            await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: SettingsKeyboard.GetTrialKeyboard(), cancellationToken: cancellationToken);
        }

        else if (user.IsManager)
        {
            var organization = Kernel.Organizations.GetOrganizationFromManager(user.ID);
            await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: SettingsKeyboard.GetManagerKeyboard(organization!.Type), cancellationToken: cancellationToken);
        }

        else if (user.IsEmployeer)
        {
            await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: SettingsKeyboard.GetEmployeeKeyboard(), cancellationToken: cancellationToken);
        }

        else await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: SettingsKeyboard.GetUserKeyboard(), cancellationToken: cancellationToken);
        
        Debug.Log($"[KEYBOARD] {UserKeyboard.SETTINGS_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.SettingsButton);
    }
}
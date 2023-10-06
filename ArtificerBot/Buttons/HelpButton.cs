using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Artificer.Bot.Buttons;

public static class HelpButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var message = user.Type switch
        {
            UserType.AnonymousUser => Kernel.Messages.Get("help_anonymous_button"),
            UserType.RegisterUser => Kernel.Messages.Get("help_user_button"),
            UserType.Employer => Kernel.Messages.Get("help_employer_button"),
            UserType.ManagerBalance => Kernel.Messages.Get("help_manager_balance_button"),
            UserType.ManagerContract => Kernel.Messages.Get("help_manager_contract_button"),
            _ => throw new ArgumentOutOfRangeException()
        };

        await botClient.SendTextMessageAsync(user.ID, message, ParseMode.Markdown, disableWebPagePreview: true,
            cancellationToken: cancellationToken);
        
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.HELP_BUTTON}", user.ID);

        await Kernel.Database.AddAction(user.ID, ActionType.HelpButton);
    }
}
using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class DevelopButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(user.ID, "Кнопка в разработке.", disableWebPagePreview: true,
            cancellationToken: cancellationToken);
        
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.DEL_EMPLOYEE_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] Кнопка в разработке.", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.DevelopButton);
    }
}
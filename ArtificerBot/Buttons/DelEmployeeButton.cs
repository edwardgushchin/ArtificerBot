using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class DelEmployeeButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, "Кнопка в разработке.", disableWebPagePreview: true,
            cancellationToken: cancellationToken);
        
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.DEL_EMPLOYEE_BUTTON}", chatId);
        Debug.Log($"[MESSAGE] Кнопка в разработке.", chatId);
        
        await Kernel.Database.AddAction(chatId, ActionType.DelEmployeeButton);
    }
}
using Artificer.Utility;
using Telegram.Bot;
using Artificer.Utility.Keyboards;

namespace Artificer.Bot.Buttons;

public static class ManagerButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("manage_button");
        await botClient.SendTextMessageAsync(chatId, msg, replyMarkup: SupportKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
        
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.MANAGER_BUTTON}", chatId);
        Debug.Log($"[MESSAGE] {msg}", chatId);
        
        await Kernel.Database.AddAction(chatId, ActionType.ManagerButton);
    }
}
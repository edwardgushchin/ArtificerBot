using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class MainmenuButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        const string msg = "Главное меню:";
        await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: UserKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
        
        Debug.Log($"[KEYBOARD] {ScenariosKeyboard.MAINMENU_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.MainmenuButton);
    }
}
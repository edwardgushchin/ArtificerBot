using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class ScenariosButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("scenarios_button");
        await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: ScenariosKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
        
        Debug.Log($"[KEYBOARD] {UserKeyboard.SCENARIOS_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.ScenariosButton);
    }
}
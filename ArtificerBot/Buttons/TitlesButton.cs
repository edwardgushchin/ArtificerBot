using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Bot.Buttons;

public static class TitlesButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("titles_button");
        await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
        user.Stage = UserStage.Titles;
        
        Debug.Log($"[KEYBOARD] {ScenariosKeyboard.TITLES_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.TitlesButton);
    }
}
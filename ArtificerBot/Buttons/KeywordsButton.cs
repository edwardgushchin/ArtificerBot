using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Bot.Buttons;

public static class KeywordsButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("keywords_button");
        await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
        user.Stage = UserStage.Keywords;
        
        Debug.Log($"[KEYBOARD] {ScenariosKeyboard.KEYWORDS_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.KeywordsButton);
    }
}
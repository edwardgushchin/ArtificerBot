using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Bot.Buttons;

public static class HashtagButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        var msg = Kernel.Messages.Get("hash_button");
        await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
        user.Stage = UserStage.Hash;
        
        Debug.Log($"[KEYBOARD] {ScenariosKeyboard.HASHTAG_BUTTON}", user.ID);
        Debug.Log($"[MESSAGE] {msg}", user.ID);
        
        await Kernel.Database.AddAction(user.ID, ActionType.HashtagButton);
    }
}
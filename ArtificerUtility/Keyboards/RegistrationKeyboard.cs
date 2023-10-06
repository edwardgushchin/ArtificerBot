using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class RegistrationKeyboard
{
    public static InlineKeyboardMarkup GetKeyboard(long messageId)
    {

        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🙍🏼‍♂️ Зарегистрироваться", $"registration {messageId}"),
                InlineKeyboardButton.WithCallbackData("💳️ А сколько это стоит?", $"price {messageId}"),
            }
        });
    }
}
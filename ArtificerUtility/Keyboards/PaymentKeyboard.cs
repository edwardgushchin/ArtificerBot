using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class PaymentKeyboard
{
    public static InlineKeyboardMarkup GetKeyboard(string redirectUrl)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithUrl("💳 Перейти для оплаты", redirectUrl),
            }
        });
    }
}
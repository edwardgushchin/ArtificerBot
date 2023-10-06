using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class PaymentErrorKeyboard
{
    public static InlineKeyboardMarkup GetKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("💰 Пополнить счет", "payment_error_keyboard"),
            }
        });
    }
}
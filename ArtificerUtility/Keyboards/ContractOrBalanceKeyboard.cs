using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class ContractOrBalanceKeyboard
{
    public static InlineKeyboardMarkup GetKeyboard(long messageId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📝️ По договору", $"contract {messageId}"),
                InlineKeyboardButton.WithCallbackData("💵 По зачислению",$"balance {messageId}")
            }
        });
    }
}
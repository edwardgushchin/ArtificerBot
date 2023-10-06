using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class SupportKeyboard
{
    public static InlineKeyboardMarkup GetKeyboard()
    {

        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithUrl("👩🏼‍🔧 Написать менеджеру", "https://t.me/eduardgushchin"),
            }
        });
    }
}
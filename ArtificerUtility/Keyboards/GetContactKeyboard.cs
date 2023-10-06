using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class GetContactKeyboard
{
    private const string TELEPHONE_BUTTON = "☎️ Оставить свой контакт";

    public static ReplyKeyboardMarkup GetKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                KeyboardButton.WithRequestContact(TELEPHONE_BUTTON),
            }
        })
        {
            ResizeKeyboard = true
        };
    }
}
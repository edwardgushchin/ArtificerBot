using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class UserKeyboard
{
    public const string SCENARIOS_BUTTON = "🎑 Сценарии";

    public const string SETTINGS_BUTTON = "⚙️ Настройки";

    public static ReplyKeyboardMarkup GetKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(SCENARIOS_BUTTON),
                new KeyboardButton(SETTINGS_BUTTON),
            }
        })
        {
            ResizeKeyboard = true
        };
    }
}
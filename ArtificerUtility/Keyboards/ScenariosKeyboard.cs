using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class ScenariosKeyboard
{
    public const string MAINMENU_BUTTON = "⬅️ В главное меню";
    public const string TITLES_BUTTON = "🔥 Заголовки";
    public const string HASHTAG_BUTTON = "#️⃣ Хештеги";
    public const string SINONYMS_BUTTON = "📃 Синонимы";
    public const string KEYWORDS_BUTTON = "🔑 Ключевые слова";
    //public const string AUDIOTOTEXT_BUTTON = "🎙 Аудио в текст";

    public static ReplyKeyboardMarkup GetKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(TITLES_BUTTON),
                new KeyboardButton(HASHTAG_BUTTON),
            },
            new[]
            {
                new KeyboardButton(SINONYMS_BUTTON),
                new KeyboardButton(KEYWORDS_BUTTON),
            },
            new[]
            {
                //new KeyboardButton(AUDIOTOTEXT_BUTTON),
                new KeyboardButton(MAINMENU_BUTTON),
            },
        })
        {
            ResizeKeyboard = true
        };
    }
}
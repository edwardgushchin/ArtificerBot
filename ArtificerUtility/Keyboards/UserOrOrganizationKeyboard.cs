using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class UserOrOrganizationKeyboard
{
    public static InlineKeyboardMarkup GetKeyboard(long messageId)
    {

        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🙍🏼‍♂️ Физическое лицо", $"user {messageId}"),
                InlineKeyboardButton.WithCallbackData("🏢 Юридическое лицо",$"organization {messageId}")
            }
        });
    }
}
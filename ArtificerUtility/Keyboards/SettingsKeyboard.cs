using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Utility.Keyboards;

public static class SettingsKeyboard
{
    public const string MAINMENU_BUTTON = "⬅️ В главное меню";

    public const string STATUS_BUTTON = "ℹ Информация";
    public const string ISSUE_INVOICE = "📝 Выписать счет";
    public const string BALANCE_BUTTON = "💰 Пополнить счет";

    public const string ADD_EMPLOYEE_BUTTON = "🙍🏼‍♂️ Добавить сотрудника";
    public const string DEL_EMPLOYEE_BUTTON = "🙅🏼‍♂️ Удалить сотрудника";

    public const string DEL_ORGANIZATION_BUTTON = "🙅🏼‍♂️ Покинуть организацию";

    public const string HELP_BUTTON = "❓ Справка";
    public const string MANAGER_BUTTON = "💬 Менеджер";

    public const string REGISTRATION_BUTTON = "📖 Регистрация";

    public const string PRICE_BUTTON = "💰 Прайс";

    public static ReplyKeyboardMarkup GetManagerKeyboard(OrganizationType type)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(STATUS_BUTTON),
                new KeyboardButton(type == OrganizationType.Balance ? BALANCE_BUTTON : ISSUE_INVOICE),
            },
            new[]
            {
                new KeyboardButton(ADD_EMPLOYEE_BUTTON),
                new KeyboardButton(DEL_EMPLOYEE_BUTTON),
            },
            new[]
            {
                new KeyboardButton(HELP_BUTTON),
                new KeyboardButton(MANAGER_BUTTON),
            },
            new[]
            {
                new KeyboardButton(MAINMENU_BUTTON),
            },
        })
        {
            ResizeKeyboard = true
        };
    }

    public static ReplyKeyboardMarkup GetUserKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(STATUS_BUTTON),
                new KeyboardButton(BALANCE_BUTTON),
            },
            new[]
            {
                new KeyboardButton(HELP_BUTTON),
                new KeyboardButton(MANAGER_BUTTON),
            },
            new[]
            {
                new KeyboardButton(MAINMENU_BUTTON),
            },
        })
        {
            ResizeKeyboard = true
        };
    }
    
    public static ReplyKeyboardMarkup GetEmployeeKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(HELP_BUTTON),
                new KeyboardButton(DEL_ORGANIZATION_BUTTON),
            },
            new[]
            {
                new KeyboardButton(MAINMENU_BUTTON),
            }
        })
        {
            ResizeKeyboard = true
        };
    }

    public static ReplyKeyboardMarkup GetTrialKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(STATUS_BUTTON),
                new KeyboardButton(REGISTRATION_BUTTON),
            },
            new[]
            {
                new KeyboardButton(PRICE_BUTTON),
                new KeyboardButton(MANAGER_BUTTON),
            },
            new[]
            {
                new KeyboardButton(HELP_BUTTON),
                new KeyboardButton(MAINMENU_BUTTON),
            }
        })
        {
            ResizeKeyboard = true
        };
    }
}
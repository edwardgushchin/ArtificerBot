using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class DelOrganizationButton
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.DEL_ORGANIZATION_BUTTON}", user.ID);
        
        var organization = Kernel.Organizations.GetOrganizationFromManager(user.Organization);
        if (organization != null)
        {
            var msg = Kernel.Messages.Get("del_organization").Replace("{%ORGANIZATION_NAME%}", organization.Name);
            user.Organization = null;
            await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: SettingsKeyboard.GetUserKeyboard(), cancellationToken: cancellationToken);
            var managerMsg = Kernel.Messages.Get("employeer_exit").Replace("{%USER_NAME%}", user.FirstName);
            await botClient.SendTextMessageAsync(organization.Manager, managerMsg, cancellationToken: cancellationToken);
            
            Debug.Log($"[MESSAGE] {msg}", user.ID);
            Debug.Log($"[MESSAGE] {managerMsg}", organization.Manager);
        }
        else
        {
            var msg = $"❌ Произошла ошибка. Организации {user.ID} не существует";
            await botClient.SendTextMessageAsync(user.ID, msg, cancellationToken: cancellationToken);
            Debug.Log($"[ERROR] {msg}", user.ID);
        }
        
        await Kernel.Database.AddAction(user.ID, ActionType.DelOrganizationButton);
    }
}
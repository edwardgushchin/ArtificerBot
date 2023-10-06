using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;

namespace Artificer.Bot.Buttons;

public static class AddEmployeeButton
{
    public static async Task HandlerAsync(ITelegramBotClient botClient, User user, CancellationToken cancellationToken)
    {
        Debug.Log($"[KEYBOARD] {SettingsKeyboard.ADD_EMPLOYEE_BUTTON}", user.ID);
        
        var invite = Guid.NewGuid().ToString();
        var organization = Kernel.Organizations.GetOrganizationFromManager(user.ID);
        if (organization != null)
        {
            var inviteString = $"Invite:{organization.Manager}:{invite}";
            var msg = Kernel.Messages.Get("add_employee_button").Replace("{%INVITE%}", inviteString);
            
            await botClient.SendTextMessageAsync(user.ID, msg, disableWebPagePreview: true, cancellationToken: cancellationToken);
            
            Debug.Log($"[MESSAGE] {msg}", user.ID);
            Kernel.Invites.Add(organization.Manager, invite);
        }
        else
        {
            var msg = $"❌ Произошла ошибка. Организации {user.ID} не существует";
            
            await botClient.SendTextMessageAsync(user.ID, msg, cancellationToken: cancellationToken);
            
            Debug.Log($"[ERROR] {msg}", user.ID);
        }
        
        await Kernel.Database.AddAction(user.ID, ActionType.AddEmployeeButton);
    }
}
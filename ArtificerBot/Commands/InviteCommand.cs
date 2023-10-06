using Telegram.Bot;
using Artificer.Utility;
using Artificer.Utility.Keyboards;

namespace Artificer.Bot.Commands;

public static class InviteCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, string? message, CancellationToken cancellationToken)
    {
        var lines = message!.Split('\n');
        var invite = false;
        foreach (var line in lines)
        {
            if (!line.StartsWith("Invite")) continue;
            invite = true;
            var splites = line.Split(':');
            var organizationId = Convert.ToInt64(splites[1]);
            var uidd = splites[2];

            var organization = Kernel.Organizations.GetOrganizationFromManager(organizationId);

            if (organization != null)
            {
                var access = false;
                var invites = Kernel.Invites.GetInvitesFromOrganizationID(organizationId);
                foreach (var i in invites.Where(i => i.UUID == uidd))
                {
                    access = true;
                    if (user.Organization != null)
                    {
                        await Kernel.Organizations.Delete(user.Organization.Value);
                    }
                    else user.Organization = organization.Manager;
                    Kernel.Invites.Remove(i);
                }

                if (!access)
                {
                    var msg = "❌ Инвайт устарел и больше не существует.";
                    await botClient.SendTextMessageAsync(user.ID, msg, cancellationToken: cancellationToken);
                    Debug.Log($"[MESSAGE] {message}", user.ID);
                }
                else
                {
                    user.Organization = organizationId;
                    user.Stage = UserStage.Ready;

                    var msg = $"✅ Вы успешно присоеденились к \"{organization.Name}\".";
                    var msg2 = Kernel.Messages.Get("employee_success");
                    var msg3 = $"✅ Пользователь {user.FirstName} успешно присоединился к вашей организации.";

                    await botClient.SendTextMessageAsync(user.ID, msg, replyMarkup: UserKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(user.ID, msg2, cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(organization.Manager, msg3, cancellationToken: cancellationToken);

                    Debug.Log($"[MESSAGE] {msg}", user.ID);
                    Debug.Log($"[MESSAGE] {msg2}", user.ID);
                    Debug.Log($"[MESSAGE] {msg3}", organization.Manager);
                        
                    Debug.Log($"User id{user.ID} joined organization id{organization.Manager}.", Debug.Sender.Telegram, Debug.MessageStatus.INFO);
                }
            }
            else
            {
                var msg = $"❌ Произошла ошибка. Организации {organizationId} не существует";
                await botClient.SendTextMessageAsync(user.ID, msg, cancellationToken: cancellationToken);
                Debug.Log($"[MESSAGE] {msg}", user.ID);
            }
            
            Debug.Log($"[COMMAND] InviteCommand", user.ID);

            await Kernel.Database.AddAction(user.ID, ActionType.InviteCommand);
        }

        if (!invite)
        {
            var msg = "❌ Инвайт не найден";
            await botClient.SendTextMessageAsync(user.ID, msg, cancellationToken: cancellationToken);
            Debug.Log($"[MESSAGE] {msg}", user.ID);
            
            await Kernel.Database.AddAction(user.ID, ActionType.InviteCommand);
        }
    }
}
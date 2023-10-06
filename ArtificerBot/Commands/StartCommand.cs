using Artificer.Utility;
using Telegram.Bot;
using Artificer.Utility.Keyboards;

namespace Artificer.Bot.Commands;

public static class StartCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        Debug.Log($"[COMMAND] StartCommand", chatId);
        
        var user = Kernel.Users.Create(chatId);
        user.Balance = Kernel.Settings.StartBalance;
        user.Trial = true;
        
        try
        {
           
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part1"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part1")));
            
            await Telegram.SendWaitMessage(botClient, user, 8130, Kernel.Messages.Get("start_part1"), cancellationToken);
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part2"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part2")));

            await Telegram.SendWaitMessage(botClient, user, 11640, Kernel.Messages.Get("start_part2"), cancellationToken);
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part3"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part3")));

            await Telegram.SendWaitMessage(botClient, user, 7170, Kernel.Messages.Get("start_part3"), cancellationToken);
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part4"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part4")));
            
            await Telegram.SendWaitMessage(botClient, user, 8040, Kernel.Messages.Get("start_part4"), cancellationToken);
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part5"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part5")));

            await Telegram.SendWaitMessage(botClient, user, 5320, Kernel.Messages.Get("start_part5"), cancellationToken);
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part6"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part6")));

            await Telegram.SendWaitMessage(botClient, user, 6920, Kernel.Messages.Get("start_part6"), cancellationToken);
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part7"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part7")));

            await Telegram.SendWaitMessage(botClient, user, 3110, Kernel.Messages.Get("start_part7"), cancellationToken);
            
            //user.AddConversation("assistant", Kernel.Messages.Get("start_part8"), await Kernel.GetTokenCount(Kernel.Messages.Get("start_part8")));
            
            await botClient.SendTextMessageAsync(user.ID, Kernel.Messages.Get("start_part8"), replyMarkup: UserKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
            
            //user.UpdateConversation();

            await Kernel.Database.AddAction(user.ID, ActionType.StartCommand);
        }
        catch (Exception e)
        {
            if (e.Message.StartsWith("Forbidden: bot was blocked by the user"))
            {
                user.Enabled = false;
                
                await Kernel.Database.AddAction(user.ID, ActionType.UserLeave);
            }
        }
    }
}
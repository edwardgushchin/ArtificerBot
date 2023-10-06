using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Artificer.Bot.Commands;

public static class SocialmediaCommand
{
    public static async Task HandleAsync(ITelegramBotClient botClient, User user, string prompt, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendChatActionAsync(user.ID, ChatAction.Typing, cancellationToken);

            if (Kernel.CheckBalance(user, out var errorMessage, out var errorKeyboard) == false)
            {
                if (user.Trial)
                {
                    var del = await botClient.SendTextMessageAsync(user.ID, "_", replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                    await botClient.DeleteMessageAsync(new ChatId(user.ID), del.MessageId,
                        cancellationToken: cancellationToken);
                    var m = await botClient.SendTextMessageAsync(user.ID, errorMessage!,
                        cancellationToken: cancellationToken);
                    await botClient.EditMessageReplyMarkupAsync(user.ID, m.MessageId,
                        replyMarkup: RegistrationKeyboard.GetKeyboard(m.MessageId),
                        cancellationToken: cancellationToken);
                }
                else
                {
                    if(errorKeyboard != null)
                        await botClient.SendTextMessageAsync(user.ID, errorMessage!, replyMarkup: errorKeyboard, cancellationToken: cancellationToken);
                    else await botClient.SendTextMessageAsync(user.ID, errorMessage!, cancellationToken: cancellationToken);
                }

                Debug.Log($"[RESPONS] [ERROR] {errorMessage}", user.ID);
            }
            else
            {
                var response = await Kernel.GetChatGPTResponse(user, prompt, true);
                var text = response.Content;

                if (response.FinishReason == "error")
                {
                    await botClient.SendTextMessageAsync(user.ID, text, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                    Debug.Log($"[RESPONS] [ERROR] {text}", user.ID);
                    return;
                }
                
                await botClient.SendTextMessageAsync(user.ID, text, parseMode: ParseMode.Markdown, replyMarkup: ScenariosKeyboard.GetKeyboard(), cancellationToken: cancellationToken);
                user.Stage = UserStage.Ready;
                Kernel.UpdateRequestBalance(user);
                Debug.Log($"[RESPONS] {text}", user.ID);
            }
            
            Debug.Log($"[REQUEST] {prompt}", user.ID);
            await Kernel.Database.AddAction(user.ID, ActionType.SocialmediaCommand);
        }
        catch (Exception e)
        {
            Debug.Log($"[RequestQuery] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram, Debug.MessageStatus.FAIL);
        }
    }
}
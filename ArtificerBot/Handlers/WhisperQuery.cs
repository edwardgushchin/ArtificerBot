using Artificer.Utility;
using Artificer.Utility.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using TMessage = Telegram.Bot.Types.Message;

namespace Artificer.Bot.Handlers;

public static class WhisperQuery
{
    public static async Task HandleAudioAsync(ITelegramBotClient botClient, User user, TMessage audio, string ext, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendChatActionAsync(user.ID, ChatAction.Typing, cancellationToken);
            
            if (Kernel.CheckBalance(user, out var errorMessage, out var errorKeyboard) == false)
            {
                if (user.Trial)
                {
                    var del = await botClient.SendTextMessageAsync(user.ID, "💬",
                        replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
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
                    Debug.Log($"[AUDIORESPONS] [ERROR] {errorMessage}", user.ID);
                }
            }
            else
            {
                var duration = (int)(audio.Audio!.Duration + 0.5d);
                var fileSize = audio.Audio.FileSize;

                var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
                var filePath = Path.Combine(tempPath, audio.Audio!.FileId);

                var textWaitMessage = Kernel.Messages.Get("whisper_request");
                
                var waitMessage = await botClient.SendTextMessageAsync(user.ID, textWaitMessage, 
                    replyToMessageId: audio.MessageId, cancellationToken: cancellationToken);
                
                Debug.Log($"[MESSAGE] {textWaitMessage}", user.ID);
                
                if (fileSize > 20000)
                {
                    await botClient.SendVoiceAsync(new ChatId(Kernel.Settings.AgentId),
                        new InputOnlineFile(audio.Audio!.FileId), $"{audio.From!.Id}:{waitMessage.MessageId}:{audio.MessageId}",
                        cancellationToken: cancellationToken);
                    return;
                }

                await using (var stream = File.OpenWrite(filePath))
                {
                    await botClient.GetInfoAndDownloadFileAsync(audio.Audio.FileId, stream, cancellationToken);
                    stream.Close(); 
                    File.Move(filePath, filePath + ext);
                    filePath += ext;
                }
                
                if (ext is ".opus" or ".ogg")
                {
                    filePath = await Kernel.ConvertToMp3(filePath);
                }

                var returnetText = await Kernel.GetTranscriptionsResponse(user.ID, filePath);

                var pathOutput = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(filePath));
                if(!Directory.Exists(pathOutput)) Directory.CreateDirectory(pathOutput);
                var transFile = Path.Combine(pathOutput, "transcription.txt");
                await using (var transcription = File.AppendText(transFile))
                {
                    await transcription.WriteAsync(returnetText);
                }

                await using (var fileStream = new FileStream(transFile, FileMode.Open))
                {
                    var inputFile = new InputOnlineFile(fileStream)
                    {
                        FileName = Path.GetFileName(transFile)
                    };

                    await botClient.DeleteMessageAsync(new ChatId(user.ID), waitMessage.MessageId, cancellationToken: cancellationToken);

                    var msg = "Я расшифровал для вас аудиозапись в текстовый файл";
                    await botClient.SendDocumentAsync(new ChatId(user.ID), inputFile,
                        caption: msg, replyToMessageId: audio.MessageId, cancellationToken: cancellationToken);
                    
                    Debug.Log($"[MESSAGE] {msg}", user.ID);
                }
                
                #pragma warning disable CS4014
                Kernel.Database.AddWhisperRequest(user.ID, user.Organization, duration);
                #pragma warning restore CS4014
                Kernel.UpdateWhisperBalance(user, duration);
                user.Stage = UserStage.Ready;
                Directory.Delete(pathOutput, true);
                File.Delete(filePath);
                Debug.Log($"[AUDIORESPONS] {returnetText}", user.ID);
            }
            
            await Kernel.Database.AddAction(user.ID, ActionType.WhisperQuery);
        }
        catch (Exception e)
        {
            if (user != null)
                await botClient.SendTextMessageAsync(user.ID, "❌ Произошла непредвиденная ошибка. Попробуйте еще раз.",
                    ParseMode.Markdown, cancellationToken: cancellationToken);
            Debug.Log($"[WhisperQuery] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram,
                Debug.MessageStatus.FAIL);
        }
    }

    public static async Task HandleDocumentAsync(ITelegramBotClient botClient, User user, TMessage document, string ext, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(user.ID, ChatAction.Typing, cancellationToken);
        
        if (Kernel.CheckBalance(user, out var errorMessage, out var errorKeyboard) == false)
        {
            if (user.Trial)
            {
                var del = await botClient.SendTextMessageAsync(user.ID, "💬",
                    replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
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
                Debug.Log($"[AUDIORESPONS] [ERROR] {errorMessage}", user.ID);
            }
        }
        else
        {
            var textWaitMessage = Kernel.Messages.Get("whisper_request");
            Debug.Log($"[MESSAGE] {textWaitMessage}", user.ID);
            var waitMessage = await botClient.SendTextMessageAsync(user.ID, textWaitMessage, 
                replyToMessageId: document.MessageId, cancellationToken: cancellationToken);
            
            await botClient.SendVoiceAsync(new ChatId(Kernel.Settings.AgentId),
                new InputOnlineFile(document.Document!.FileId), $"{document.From!.Id}:{waitMessage.MessageId}:{document.MessageId}",
                cancellationToken: cancellationToken);
        }
    }

    /*public static async Task HandleVideoAsync(ITelegramBotClient botClient, User user, TMessage document, string ext, CancellationToken cancellationToken)
    {
        
    }*/
}
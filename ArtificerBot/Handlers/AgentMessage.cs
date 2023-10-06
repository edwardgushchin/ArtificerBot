using Artificer.Utility;
using NAudio.Wave;
using Newtonsoft.Json;
using TL;
using WTelegram;

namespace Artificer.Bot.Handlers;

public static class AgentMessage
{
    public static async Task HandleAsync(Client _agent, TL.Message message)
    {
        try
        {
            if (message.Peer.ID != Kernel.Settings.BotId || message.From?.ID == Kernel.Settings.AgentId) return;
        
            var split = message.message.Split(':');
            var fromId = Convert.ToInt64(split[0]);
            var waitMessage = Convert.ToInt32(split[1]);
            var replyMessage = Convert.ToInt32(split[2]);

            var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            
            var document = (Document)((MessageMediaDocument) message.media).document;

            var filePath = document.mime_type switch
            {
                "audio/x-wav" => Path.Combine(tempPath, document.ID + ".wav"),
                "audio/ogg" => Path.Combine(tempPath, document.ID + ".oga"),
                "audio/aac" => Path.Combine(tempPath, document.ID + ".aac"),
                "video/mp4" => Path.Combine(tempPath, document.ID + ".mp4"),
                "video/mpeg" => Path.Combine(tempPath, document.ID + ".mpeg"),
                "video/webm" => Path.Combine(tempPath, document.ID + ".webm"),
                "audio/mpeg" => Path.Combine(tempPath, document.ID + ".mp3"),
                "audio/m4a" => Path.Combine(tempPath, document.ID + ".m4a"),
                "audio/mp4" => Path.Combine(tempPath, document.ID + ".mp4"),
                _ => string.Empty
            };

            await using (var stream = File.OpenWrite(filePath))
            {
                await _agent.DownloadFileAsync(document, stream);
            }

            var mp3File = document.mime_type switch
            {
                "audio/x-wav" => await Kernel.ConvertToMp3(filePath),
                "audio/ogg" => await Kernel.ConvertToMp3(filePath),
                "audio/aac" => await Kernel.ConvertToMp3(filePath),
                "audio/mpeg" => filePath,
                "video/mp4" => await Kernel.ConvertToMp3(filePath),
                "video/mpeg" => await Kernel.ConvertToMp3(filePath),
                "video/webm" => await Kernel.ConvertToMp3(filePath),
                "audio/m4a" => await Kernel.ConvertToMp3(filePath),
                "audio/mp4" => await Kernel.ConvertToMp3(filePath),
                _ => string.Empty
            };

            var reader = new Mp3FileReader(mp3File);
            var duration = reader.TotalTime.TotalSeconds;
            var lenght = reader.Length;
            reader.Close();
            
            var returnetText = string.Empty;
                
            if (lenght > 10000)
            {
                var mp3chunks = await Kernel.TrimAudioFile(mp3File, ".mp3");
                    
                foreach (var file in mp3chunks)
                {
                    var t = await Kernel.GetTranscriptionsResponse(fromId, file);
                    if (returnetText == string.Empty) returnetText += $"{t}";
                    else returnetText += $" {t}";
                    File.Delete(file);
                }
            }
            else
            {
                returnetText = await Kernel.GetTranscriptionsResponse(fromId, mp3File);
            }
            
            File.Delete(mp3File);
                
            var pathOutput = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(mp3File));
            if(!Directory.Exists(pathOutput)) Directory.CreateDirectory(pathOutput);
            var transFile = Path.Combine(pathOutput, "transcription.txt");
            await using (var transcription = File.AppendText(transFile))
            {
                await transcription.WriteAsync(returnetText);
            }

            var dialogs = await _agent.Messages_GetAllDialogs();
            var user = dialogs.users.First(pairs => pairs.Key == message.Peer.ID).Value;
                
            var inputPeer = new InputPeerUser(user.ID, user.access_hash);

            var json = JsonConvert.SerializeObject(new
            {
                from = fromId,
                path = transFile,
                seconds = (int)(duration + 0.5d),
                wait = waitMessage,
                reply = replyMessage
            });

            await _agent.SendMessageAsync(inputPeer, json, reply_to_msg_id: message.id);

            await Kernel.Database.AddAction(fromId, ActionType.AgentMessage);
        }
        catch (Exception e)
        {
            Debug.Log($"[AgentMessageHandler] {e.Message}: {e.StackTrace}", Debug.Sender.Telegram, Debug.MessageStatus.FAIL);
        }
    }
}
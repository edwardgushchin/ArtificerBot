using System.Collections.Specialized;
using Artificer.Utility;

namespace Artificer.Bot;

public class Messages
{
    private readonly ListDictionary message;

    public Messages()
    {
        Debug.Log("Initializing an instance of the Messages Subsystem...", Debug.Sender.Messages, Debug.MessageStatus.INFO);
        
        message = new ListDictionary();
        
        var messagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "messages");

        var files = Directory.GetFiles(messagesPath);

        foreach (var file in files)
        {
            message.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
        }
        
        Debug.Log("The Messages Subsystem instance has been initialized.", Debug.Sender.Messages, Debug.MessageStatus.INFO);
    }

    public string Get(string name)
    {
        return message[name]!.ToString()!
            .Replace("{%BOTNAME%}", Kernel.Settings.BotStringName)
            .Replace("{%MAX_VALUE%}", int.MaxValue.ToString());
    }
}
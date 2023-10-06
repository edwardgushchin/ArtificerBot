using System.Collections.Specialized;
using Artificer.Utility;

namespace Artificer.Bot;

public class Prompts
{
    private readonly ListDictionary _messageList;
    
    public Prompts()
    {
        Debug.Log("Initializing an instance of the Prompts Subsystem...", Debug.Sender.Messages, Debug.MessageStatus.INFO);

        _messageList = new ListDictionary();

        var messagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prompts");

        var files = Directory.GetFiles(messagesPath);

        foreach (var file in files)
        {
           // var prompt = ;
            //var tokens = Kernel.GetTokenCount(prompt).GetAwaiter().GetResult();
            //var message = new Message {role = "system", tokens = tokens, content = prompt};
            _messageList.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
        }
        
        Debug.Log("The Prompts Subsystem instance has been initialized.", Debug.Sender.Messages, Debug.MessageStatus.INFO);
    }
    
    
    public string Get(string name)
    {
        return (string)_messageList[name]!;
    }
}
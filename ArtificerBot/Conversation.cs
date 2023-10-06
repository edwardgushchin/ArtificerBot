using Artificer.Utility;
using Newtonsoft.Json;

namespace Artificer.Bot;

public class Conversation
{
    private readonly List<Message> _messageList;

    public Conversation(string conversation, long chatId)
    {
        _messageList = JsonConvert.DeserializeObject<List<Message>>(conversation);
        //user = Kernel.Users?.GetUserFromChatId(userId);
        //if (_messageList.Count != 0) return;

        //var promt = Kernel.Messages.Get("start_prompt");

        /*_messageList.Add(new Message()
        {
            role = "system",
            content = promt,
            tokens = Kernel.GetTokenCount(promt).Result
        });
        
        Kernel.Database.SetUserConversation(userId, ToString());*/
    }

    public void Add(string role, string message, int tokens)
    {
        var totalTokens = _messageList.Sum(msg => msg.tokens);

        while (totalTokens + tokens > 4096)
        {
            _messageList.RemoveAt(0);
            totalTokens = _messageList.Sum(msg => msg.tokens);
        }

        _messageList.Add(new Message()
        {
            role = role,
            content = message,
            tokens = tokens
        });
    }

    public void Decrease()
    {
        if(_messageList.Count > 0)
            _messageList.RemoveAt(0);
    }

    public Message[] GetMessagesList(UserType style, User user)
    {
        var list = new List<Message>(_messageList);

        var replace = style switch
        {
            UserType.AnonymousUser => Kernel.Prompts.Get("anonymous")!
                .Replace("{%BotStringName%}", Kernel.Settings.BotStringName)
                .Replace("{%StartBalance%}", Kernel.Settings.StartBalance.ToString()),
            UserType.RegisterUser => Kernel.Prompts.Get("user")!
                .Replace("{%BotStringName%}", Kernel.Settings.BotStringName)
                .Replace("{%UserFirstName%}", user.FirstName),
            UserType.Employer => Kernel.Prompts.Get("employer")!
                .Replace("{%BotStringName%}", Kernel.Settings.BotStringName)
                .Replace("{%UserFirstName%}", user.FirstName)
                .Replace("{%OrganizationName%}",
                    Kernel.Organizations.GetOrganizationFromManager(user.Organization)!.Name),
            UserType.ManagerBalance => Kernel.Prompts.Get("manager_balance")!
                .Replace("{%BotStringName%}", Kernel.Settings.BotStringName)
                .Replace("{%UserFirstName%}", user.FirstName)
                .Replace("{%OrganizationName%}", Kernel.Organizations.GetOrganizationFromManager(user.ID)!.Name),
            UserType.ManagerContract => Kernel.Prompts.Get("manager_contract")!
                .Replace("{%BotStringName%}", Kernel.Settings.BotStringName)
                .Replace("{%UserFirstName%}", user.FirstName)
                .Replace("{%OrganizationName%}", Kernel.Organizations.GetOrganizationFromManager(user.ID)!.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };

        var tokens = Kernel.GetTokenCount(replace).GetAwaiter().GetResult();
        list.Insert(0, new Message { content = replace, role = "system", tokens = tokens});
        
        return list.ToArray();
    }

    public sealed override string ToString()
    {
        var list = new List<Message>(_messageList);
        //list.RemoveAt(0);
        return JsonConvert.SerializeObject(list);
    }
}
using Artificer.Utility;

namespace Artificer.Bot;

public class Users
{
    private readonly List<User> _users;

    public Users()
    {
        Debug.Log("Initializing an User Subsystem Instance...", Debug.Sender.Users, Debug.MessageStatus.INFO);
        _users = Kernel.Database.GetAllUser();
        Debug.Log($"The User Subsystem instance initialized successfully. {_users.Count} users loaded.", Debug.Sender.Users, Debug.MessageStatus.INFO);
    }

    public User Create(long chatId)
    {
        var user = new User(chatId, null, null,  null, null, UserStage.None, 0,  true,"[]", DateTime.Now, true);
        _users.Add(user);
#pragma warning disable CS4014
        Kernel.Database.AddUserAsync(chatId);
#pragma warning restore CS4014
        Debug.Log($"User id{chatId} is logged into the system.", Debug.Sender.Users, Debug.MessageStatus.INFO);
        return user;
    }
    
    public User? GetUserFromChatId(long? chatId)
    {
        return _users.Find(user => user.ID == chatId);
    }

    public User[] GetUserList => _users.ToArray();
}
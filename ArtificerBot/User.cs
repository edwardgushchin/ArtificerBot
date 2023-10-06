using Artificer.Utility;

namespace Artificer.Bot;

public class User
{
    private readonly long _userId;
    private long? _organization;
    private string? _firstname;
    private string? _lastname;
    private string? _telephone;
    private UserStage _stage;
    private double _balance;
    private bool _trial;
    private bool _enabled;
    private readonly DateTime _dateOfReg;
    private readonly Conversation _conversation;
    
    public User(long userId, long? organization, string? firstname, string? lastname, string? telephone, UserStage stage, double balance, bool trial, string conversation, DateTime dateOfReg, bool enabled)
    {
        _userId = userId;
        _organization = organization;
        _firstname = firstname;
        _lastname = lastname;
        _telephone = telephone;
        _stage = stage;
        _balance = balance;
        _trial = trial;
        _conversation = new Conversation(conversation, _userId);
        _dateOfReg = dateOfReg;
        _enabled = enabled;
    }
    
    public void AddConversation(string role, string message, int tokens)
    {
        _conversation.Add(role, message, tokens);
    }

    public void DecreaseConversation()
    {
        _conversation.Decrease();
    }

    public void UpdateConversation()
    {
        #pragma warning disable CS4014
        Kernel.Database.SetUserConversation(_userId, _conversation.ToString());
        #pragma warning restore CS4014
    }

    public long ID => _userId;

    public long? Organization
    {
        get => _organization;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserOrganization(_userId, value);
            #pragma warning restore CS4014
            _organization = value;
        }
    }
    
    public string? FirstName
    {
        get => _firstname;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserFirstname(_userId, value!);
            #pragma warning restore CS4014
            _firstname = value;
        }
    }
    
    public string? LastName
    {
        get => _lastname;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserLastname(_userId, value!);
            #pragma warning restore CS4014
            _lastname = value;
        }
    }

    public string? Telephone
    {
        get => _telephone;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserTelephone(_userId, value!);
            #pragma warning restore CS4014
            _telephone = value;
        }
    }

    public UserStage Stage
    {
        get => _stage;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserStage(_userId, value);
            #pragma warning restore CS4014
            _stage = value;
        }
    }

    public double Balance
    {
        get => _balance;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserBalance(_userId, value);
            #pragma warning restore CS4014
            _balance = value;
        }
    }

    public bool Trial
    {
        get => _trial;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserTrial(_userId, value);
            #pragma warning restore CS4014
            _trial = value;
        }
    }
    
    public bool Enabled
    {
        get => _enabled;
        set
        {
            #pragma warning disable CS4014
            Kernel.Database.SetUserEnabled(_userId, value);
            #pragma warning restore CS4014
            _enabled = value;
        }
    }
    
    public string? TempData { get; set; }
    
    public bool IsManager => _organization == _userId;

    public bool IsEmployeer => _organization != null && IsManager == false;

    public DateTime DateOfReg => _dateOfReg;

    public UserType Type
    {
        get
        {
            if (_trial) return UserType.AnonymousUser; // anonymous
            if (_organization == null) return UserType.RegisterUser;
            if (_organization != _userId) return UserType.Employer;
            var organization = Kernel.Organizations.GetOrganizationFromManager(_userId);
            return organization is {Type: OrganizationType.Balance} ? UserType.ManagerBalance : UserType.ManagerContract;
        }
    }

    public Message[] GetMessageList => _conversation.GetMessagesList(Type,this);
}
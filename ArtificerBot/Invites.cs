using Artificer.Utility;

namespace Artificer.Bot;

public class Invites
{
    private readonly List<Invite> _invites;

    public Invites()
    {
        Debug.Log("Initializing an Invites Subsystem Instance...", Debug.Sender.Organizations, Debug.MessageStatus.INFO);
        _invites = Kernel.Database.GetAllInvites();
        Debug.Log($"The Invites Subsystem instance initialized successfully. {_invites.Count} invite loaded.", Debug.Sender.Organizations, Debug.MessageStatus.INFO);
    }

    public Invite[] GetInvitesFromOrganizationID(long organizationId)
    {
        return _invites.Where(i => i.OrganizationID == organizationId).ToArray();
    }

    public void Add(long organizationId, string uuid)
    {
        _invites.Add(new Invite(organizationId, uuid, DateTime.Now));
        #pragma warning disable CS4014
        Kernel.Database.AddInvite(organizationId, uuid);
        #pragma warning restore CS4014
    }

    public void Remove(Invite invite)
    {
        invite.Remove();
        _invites.Remove(invite);
    }
}

public class Invite
{
    private long _organizationId;
    private DateTime _dateTime;
    private string _uuid;
    
    public Invite(long organizationId, string uuid, DateTime dateTime)
    {
        _organizationId = organizationId;
        _uuid = uuid;
        _dateTime = dateTime;
    }

    public void Remove()
    {
        #pragma warning disable CS4014
        Kernel.Database.RemoveInvite(_uuid);
        #pragma warning restore CS4014
    }

    public long OrganizationID => _organizationId;

    public DateTime DateTime => _dateTime;

    public string UUID => _uuid;
}
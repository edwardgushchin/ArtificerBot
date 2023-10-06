using Artificer.Utility;

namespace Artificer.Bot;

public class Organizations
{
    private readonly List<Organization> _organizations;

    public Organizations()
    {
        Debug.Log("Initializing an Organization Subsystem Instance...", Debug.Sender.Organizations,
            Debug.MessageStatus.INFO); 
        _organizations = Kernel.Database.GetAllOrganization();
        Debug.Log(
            $"The Organization Subsystem instance initialized successfully. {_organizations.Count} organizations loaded.",
            Debug.Sender.Organizations, Debug.MessageStatus.INFO);
    }

    public Organization Create(long managerId)
    {
        var newOrganization = new Organization(managerId, null, OrganizationType.None, OrganizationStage.SetName,
            DateTime.Now, OrganizationRate.None);
        _organizations.Add(newOrganization);
        #pragma warning disable CS4014
        Kernel.Database.AddOrganization(managerId);
        #pragma warning restore CS4014
        Debug.Log($"Organization id{managerId} is registered in the system.", Debug.Sender.Organizations,
            Debug.MessageStatus.INFO);
        return newOrganization;
    }

    public async Task Delete(long managerId)
    {
        await Kernel.Database.DeleteOrganization(managerId);
        var org = GetOrganizationFromManager(managerId);
        if (org != null) _organizations.Remove(org);
        Debug.Log($"Organization id{managerId} is successfull removed in to system.", Debug.Sender.Organizations,
            Debug.MessageStatus.INFO);
    }

    public Organization? GetOrganizationFromManager(long? managerId)
    {
        return _organizations.Find(organization => organization.Manager == managerId);
    }

    public Organization[] OrganizationsList => _organizations.ToArray();
}


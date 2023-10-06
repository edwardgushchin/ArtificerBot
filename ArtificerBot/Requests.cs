namespace Artificer.Bot;

public class Request
{
    public int ID { get; set; }
    public long UserID { get; set; }
    public long OrganizationID { get; set; }
    public int TotalTokens { get; set; }
    public DateTime DateTime;
}
using Newtonsoft.Json;
using Artificer.Utility;

namespace Artificer.Bot;

public class Setting
{
    private long _adminId;

#pragma warning disable CS8618
    public Setting()
#pragma warning restore CS8618
    {
        Debug.Log("Reading the settings file...", Debug.Sender.Kernel, Debug.MessageStatus.INFO);

        try
        {
            var settingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            var settings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(settingPath));
            if (settings != null)
            {
                TgAccessToken = Kernel.IsDebug ? settings["DebugTgAccessToken"].ToString() : settings["TgAccessToken"].ToString();
                OAAccessToken = Kernel.IsDebug ? settings["DebugOAAccessToken"].ToString() : settings["OAAccessToken"].ToString();
                YMAccessToken = settings["YMAccessToken"].ToString();
                BotName = settings["BotName"].ToString();
                BotStringName = settings["BotStringName"].ToString();
                ShopId = Convert.ToInt32(settings["ShopId"]);
                _adminId = Convert.ToInt64(settings["AdminId"]);
                AppId = Convert.ToInt32(settings["AppId"]);
                AppHash = settings["AppHash"].ToString();
                Telephone = settings["Telephone"].ToString();
                CloudPassword = settings["CloudPassword"].ToString();
                FirstName = settings["FirstName"].ToString();
                AgentId = Convert.ToInt64(settings["AgentId"]);
                BotId = Kernel.IsDebug ? Convert.ToInt64(settings["BotDebugId"]) : Convert.ToInt64(settings["BotId"]);
                Proxy = settings["Proxy"].ToString();
                ProxyLogin = settings["ProxyLogin"].ToString();
                ProxyPass = settings["ProxyPass"].ToString();
                
                Rates = settings["Rates"].ToObject<Rate[]>();
                StartBalance = Convert.ToInt32(settings["StartBalance"].ToString());

                Debug.Log("The settings file was successfully read.", Debug.Sender.Kernel, Debug.MessageStatus.INFO);

                IsInitialized = true;
            }
            else
            {
                Debug.Log("An error has occurred with the settings file. Data not received.", Debug.Sender.Settings,
                    Debug.MessageStatus.FAIL);
                IsInitialized = false;
            }
        }
        catch (FileNotFoundException)
        {
            Debug.Log("The settings.json file was not found.", Debug.Sender.Settings, Debug.MessageStatus.FAIL);
            IsInitialized = false;
        }
        catch (Exception e)
        {
            Debug.Log($"[Init] {e.Message} {e.StackTrace}", Debug.Sender.Settings, Debug.MessageStatus.FAIL);
            IsInitialized = false;
        }
    }

    private void UpdateFile()
    {
        File.WriteAllText("settings.json", JsonConvert.SerializeObject(this));
    }


    public string TgAccessToken { get; }
    
    public string OAAccessToken { get; }

    public string YMAccessToken { get; }

    public string BotName { get; }

    public string BotStringName { get; }

    public int ShopId { get; }

    public int AppId { get; }

    public string AppHash { get; }

    public string Telephone { get; }

    public string CloudPassword { get; }
    
    public string FirstName { get; }
    
    public long AgentId { get; }
    
    public long BotId { get; }
    
    public int StartBalance { get; }

    public long AdminId
    {
        get => _adminId;
        set
        {
            Debug.Log($"The \"AdminId\" parameter is set to {value}.", Debug.Sender.Settings, Debug.MessageStatus.WARN);
            _adminId = value;
            UpdateFile();
        }
    }
    
    public string Proxy { get; }
    
    public string ProxyLogin { get; }
    
    public string ProxyPass { get; }

    public Rate[] Rates { get; }

    public bool IsInitialized { get; }
}
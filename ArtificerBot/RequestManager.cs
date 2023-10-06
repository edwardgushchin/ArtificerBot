using Artificer.Utility;
using Timer = System.Timers.Timer;

namespace Artificer.Bot;

public class RequestManager
{
    private readonly int _limiter;
    private int _position;

    public RequestManager()
    {
        Debug.Log("Initializing Request Manager...", Debug.Sender.RequestManager, Debug.MessageStatus.INFO);
        
        _limiter = 20;
        _position = 0;
        var timer = new Timer();
        timer.Elapsed += (_, _) => _position = 0;
        timer.Interval = 60000;
        timer.Enabled = true;
        timer.Start();
        
        Debug.Log("Request Manager initialization was successful.", Debug.Sender.RequestManager, Debug.MessageStatus.INFO);
    }
    
    public void ProcessRequest()
    {
        while (_position == _limiter)
        {
            //Debug.Log($"The request is being processed. Wait: 1 second", Debug.Sender.RequestManager, Debug.MessageStatus.WARN);
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
        _position++;
        //Debug.Log($"Request processed. Position: {_position}", Debug.Sender.RequestManager, Debug.MessageStatus.INFO);
    }
}
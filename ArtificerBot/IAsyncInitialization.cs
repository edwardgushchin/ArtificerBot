namespace Artificer.Bot;

public interface IAsyncInitialization
{
    Task Initialization { get; }
}
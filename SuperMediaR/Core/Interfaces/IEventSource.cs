namespace SuperMediaR.Core.Interfaces;

public interface IEventSource
{
    IEnumerable<IEvent> Events { get; }
}

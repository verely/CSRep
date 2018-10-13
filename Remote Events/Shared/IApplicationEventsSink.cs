namespace RemoteEvents
{
    /// <summary>
    /// The events sink interface known to both publisher and subscriber
    /// </summary>
    public interface IApplicationEventsSink 
    {
        void OnTime(TimeEventArgs args);
        void OnClick(ClickEventArgs args);
    }
}

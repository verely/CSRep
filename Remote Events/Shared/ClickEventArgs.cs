using System;

namespace RemoteEvents
{
    /// <summary>
    /// Describes the OnClick event
    /// </summary>
    [Serializable]
    public class ClickEventArgs : RemoteEventArgs
    {
        public ClickEventArgs(int n)
        {
            Value = string.Format("Click: {0}", n);
        }
        public string Value = "Click";
    }
}

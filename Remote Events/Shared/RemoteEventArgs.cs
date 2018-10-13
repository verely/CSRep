using System;

namespace RemoteEvents
{
    /// <summary>
    /// The remote event arguments object
    /// </summary>
    [Serializable]
    public class RemoteEventArgs : EventArgs 
    {
        public bool Enqueue = true;
    }
}

using System.Collections;

namespace Assets.Code.Interfaces
{
    /// <summary>
    /// Interface that should be applied to anything that should be serviced on each game tick.
    /// </summary>
    public interface IServiceable
    {
        /// <summary>
        /// Called on each game tick to service a given instance of a class. Done to abstract away from Unity.
        /// </summary>
        void Service();
    }
}
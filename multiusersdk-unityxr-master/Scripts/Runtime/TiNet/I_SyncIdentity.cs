namespace Ximmerse.XR
{
    using UnityEngine;


    /// <summary>
    /// Interface : sync identity.
    /// </summary>
    public interface I_SyncIdentity
    {
        /// <summary>
        /// Gets sync network ID.
        /// </summary>s
        int NetworkID
        {
            get;
        }

        /// <summary>
        /// Is the sync identity currently owned by this node ?
        /// </summary>
        bool IsOwned
        {
            get;
        }

        /// <summary>
        /// Is the sync identity currently owned by other node ?
        /// </summary>
        bool IsOwnedByOtherNode
        {
            get;
        }

        /// <summary>
        /// Is the sync identity currently not owned by any node ?
        /// </summary>
        bool HasNoOwner
        {
            get;
        }

        GameObject TargetGameObject
        {
            get;
        }
    }

}


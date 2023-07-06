using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SyncIdentity = Ximmerse.XR.SyncIdentity;
using UnityEngine.XR.Interaction.Toolkit;
namespace Ximmerse.XR
{
    /// <summary>
    /// Sync grabable:
    /// - 在 target grabable 被其他的 TiNet node claim owner的时候disable grabable;
    /// - 在 target grabable 的 权限被解除的时候， enable grabable;
    /// - 在自己抓取的时候， claim owner
    /// - 在自己释放的时候， release owner 
    /// </summary>
    [RequireComponent(typeof(SyncIdentity))]
    public class SyncGrabable : MonoBehaviour
    {
        SyncIdentity syncId;

        [SerializeField]
        XRGrabInteractable grabable;

        bool isRigidKinatmicBeforeGrabbed;

        // Start is called before the first frame update
        void Awake()
        {
            syncId = GetComponent<SyncIdentity>();
            if (!syncId)
                return;

            if (!grabable)
                grabable = syncId.TargetGameObject.GetComponent<XRGrabInteractable>();
            if (!grabable)
            {
                return;
            }

            Ximmerse.XR.SyncIdentity.OnClaimOwnershipByOtherNode += SyncIdentity_OnClaimOwnershipByOtherNode;
            /// - 在自己抓取的时候， claim owner
            /// - 在自己释放的时候， release owner 
            grabable.firstSelectEntered.AddListener(Grabable_OnGrabBegin);
            grabable.lastSelectExited.AddListener(Grabable_OnGrabEnd);
        }

        private void SyncIdentity_OnClaimOwnershipByOtherNode(Ximmerse.XR.SyncIdentity _syncId, Ximmerse.XR.UnityNetworking.I_TiNetNode node, bool isClaimed)
        {
            if (_syncId == syncId)
            {
                //被其他节点抓取:
                if (isClaimed)
                {
                    grabable.enabled = false;
                    if(grabable.GetComponent<Rigidbody>())
                    {
                        var rigid = grabable.GetComponent<Rigidbody>();
                        isRigidKinatmicBeforeGrabbed = rigid.isKinematic;
                        if(!rigid.isKinematic)
                        {
                            rigid.isKinematic = true;
                        }
                    }
                }
                //被其他节点释放:
                else
                {
                    grabable.enabled = true;
                    if (grabable.GetComponent<Rigidbody>())
                    {
                        var rigid = grabable.GetComponent<Rigidbody>();
                        if(!isRigidKinatmicBeforeGrabbed)
                        {
                            rigid.isKinematic = false;
                        }
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (grabable)
            {
                grabable.firstSelectEntered.RemoveListener(Grabable_OnGrabBegin);
                grabable.selectExited.RemoveListener(Grabable_OnGrabEnd);
            }
            SyncIdentity.OnClaimOwnershipByOtherNode -= SyncIdentity_OnClaimOwnershipByOtherNode;
        }
        /// <summary>
        /// uncliam owner when the grabble is releasing
        /// </summary>
        /// <param name="args"></param>
        private void Grabable_OnGrabEnd(SelectExitEventArgs args)
        {
            syncId.UnClaimOwner();
        }

        /// <summary>
        /// claim owner when the grabble is being grabbed
        /// </summary>
        /// <param name="args"></param>
        private void Grabable_OnGrabBegin(SelectEnterEventArgs args)
        {
            syncId.ClaimOwner();
        }


    }
}

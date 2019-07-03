using System.ComponentModel;
using Fougerite;
using UnityEngine;

namespace Wiper
{
    public class WiperHandler : MonoBehaviour
    {
        void Start()
        {
            Invoke(nameof(StartDecayBackgroundWorker), Wiper.Instance.DecayTimer * 60);
            Invoke(nameof(StartWipeBackgroundWorker), Wiper.Instance.WipeCheckTimer * 60);
        }

        private void StartWipeBackgroundWorker()
        {
            CheckWipeableObjects();
        }

        private void StartDecayBackgroundWorker()
        {
            DecayObjects();
        }
        
        private void DecayObjects()
        {
            if (Wiper.Instance.UseDecay)
            {
                Loom.QueueOnMainThread(() => { Wiper.Instance.ForceDecay(); });
                Wiper.Instance.ForceDecay();
                Invoke(nameof(StartDecayBackgroundWorker), Wiper.Instance.DecayTimer * 60);
            }
        }
        
        private void CheckWipeableObjects()
        {
            if (Wiper.Instance.UseDayLimit)
            {
                
                if (Wiper.Instance.Broadcast)
                {
                    Server.GetServer().BroadcastFrom("Wiper", "Server is checking for wipeable objects...");
                }

                Loom.QueueOnMainThread(() => { Wiper.Instance.LaunchCheck(); });

                Invoke(nameof(StartWipeBackgroundWorker), Wiper.Instance.WipeCheckTimer * 60);
            }
        }
    }
}
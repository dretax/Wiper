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
            BackgroundWorker BGW = new BackgroundWorker();
            BGW.DoWork += new DoWorkEventHandler(CheckWipeableObjects);
            BGW.RunWorkerAsync();
        }

        private void StartDecayBackgroundWorker()
        {
            BackgroundWorker BGW = new BackgroundWorker();
            BGW.DoWork += new DoWorkEventHandler(DecayObjects);
            BGW.RunWorkerAsync();
        }
        
        private void DecayObjects(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            if (Wiper.Instance.UseDecay)
            {
                //Loom.QueueOnMainThread(() => { Wiper.Instance.ForceDecay(); });
                Wiper.Instance.ForceDecay();
                Invoke(nameof(StartDecayBackgroundWorker), Wiper.Instance.DecayTimer * 60);
            }
        }
        
        private void CheckWipeableObjects(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            if (Wiper.Instance.UseDayLimit)
            {
                // Use main thread if there are problems?
                //Loom.QueueOnMainThread(() => { Wiper.Instance.LaunchCheck(); });
                if (Wiper.Instance.Broadcast)
                {
                    Server.GetServer().BroadcastFrom("Wiper", "Server is checking for wipeable objects...");
                }

                int[] obj = Wiper.Instance.LaunchCheck();
                Invoke(nameof(StartWipeBackgroundWorker), Wiper.Instance.WipeCheckTimer * 60);
                if (Wiper.Instance.Broadcast)
                {
                    Server.GetServer().BroadcastFrom("Wiper",
                        "Wiped " + obj[0] + " amount of objects, and " + obj[1] + " amount of user data.");
                }
            }
        }
    }
}
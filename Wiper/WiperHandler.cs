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
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += CheckWipeableObjects;
            backgroundWorker.RunWorkerAsync();
        }

        private void StartDecayBackgroundWorker()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += DecayObjects;
            backgroundWorker.RunWorkerAsync();
        }
        
        private void DecayObjects(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            if (Wiper.Instance.UseDecay)
            {
                Wiper.Instance.ForceDecay();
                Invoke(nameof(StartDecayBackgroundWorker), Wiper.Instance.DecayTimer * 60);
            }
        }
        
        private void CheckWipeableObjects(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            if (Wiper.Instance.UseDayLimit)
            {
                
                if (Wiper.Instance.Broadcast)
                {
                    Server.GetServer().BroadcastFrom("Wiper", "Server is checking for wipeable objects...");
                }

                Wiper.Instance.LaunchCheck();

                Invoke(nameof(StartWipeBackgroundWorker), Wiper.Instance.WipeCheckTimer * 60);
            }
        }
    }
}
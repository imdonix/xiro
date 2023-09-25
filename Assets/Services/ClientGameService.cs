using Net;
using RiptideNetworking;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services
{

    [ServiceTag("Game")]
    public class ClientGameService : ClientService
    {
        int stage = -1;
        int zombie = 0; 
        int box = 0;

        public override void Init() { }

        public override void Update()
        {
            UI.Objective(stage, zombie, box);
        }


        [RemoteCall("Stage")]
        public void Stage(Message msg) // msg not works
        {
            stage++;
        }

        [RemoteCall("ZombiKill")]
        public void ZombiKill(Message msg) // msg not works
        {
            zombie++; ;
        }

        [RemoteCall("BoxDelivered")]
        public void BoxDelivered(Message msg) // msg not works
        {
            box++;
        }



        public override void OnUserLeave(User user) { } // Client should not care about user
        public override void OnUserJoin(User user) { } // Client should not care about user
    }
}
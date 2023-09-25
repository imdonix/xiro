using Assets.Boxes;
using Net;
using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Services
{

    [ServiceTag("Game")]
    public class ServerGameService : ServerService
    {

        /*
         * 
         * Win Condition 
         * 1. Kill 50 Zombie
         * 2. Move 5 Box
         * 3. END
         * 
         * Lose condition 
         * One player dies
         *              
         */


        private int maxz = 50;
        private int zombies;
        private int boxes;
        private int killed;
        private float nexspawn;
        private bool start = false;
        private int stage = -1;

        public override void Init()
        {
            GameManager.Instance.StartCoroutine(Spawn());
        }

        public override void Update() 
        {
            nexspawn += Time.deltaTime;

            if(start)
            {
                if(stage == 0 || stage == 1)
                {
                    if (nexspawn > .25f && zombies < maxz)
                    {
                        zombies++;
                        nexspawn = 0;

                        ushort id = NetworkInstantiate<Zombie>(null);
                    }
                }
            }

        }


        public override void OnUserLeave(User user) {}

        public override void OnUserJoin(User user) {}

        IEnumerator Spawn()
        {
            while (new List<User>(GetPlayers()).Count != 2 && !Input.GetKey(KeyCode.P))
            {
                yield return new WaitForSeconds(1);
            }

            yield return new WaitForSeconds(2);

            List<User> playerlist = new List<User>(GetPlayers());
            for (int i = 0; i < playerlist.Count; i++)
            {
                NetworkInstantiate<Player>(playerlist[i]);
            }

            yield return new WaitForSeconds(3);
            start = true;

            stage = 0;
            RC r = PrepareRC("Stage");
            r.Send();

            yield return new WaitForSeconds(10);

            while (true)
            {
                maxz += 2;
                yield return new WaitForSeconds(10);
            }

        }

        [RemoteCall("DebugHitServer")]
        public void RC_DebugHit(Message msg)
        {
            _log("Hit on server service");
        }

        [RemoteCall("Destroy")]
        public void DestroyObject(Message msg)
        {
            ushort id = msg.GetUShort();
            
            if(Entity.GetEntityByID(id) is Zombie)
            {
                zombies--;
                killed++;

                if (killed > 49 && stage == 0)
                {
                    stage = 1;
                    RC r = PrepareRC("Stage");
                    r.Send();

                    NetworkInstantiate<Truck>(null);
                    for (int i = 0; i < 3; i++)
                    {
                        NetworkInstantiate<Box>(null);
                    }
                }

                RC rc = PrepareRC("ZombiKill");
                rc.Send();
            }

            NetworkDestroy(id);
        }

        [RemoteCall("SpawnAmmo")]
        public void SpawnAmmo(Message msg)
        {
            if(Random.Range(0,10) > 5)
            {
                int r = Random.Range(0, 100);
                if(r < 75)
                {
                    NetworkInstantiate<Ammobox>(null);
                }
                else
                {
                    NetworkInstantiate<ASBox>(null);
                }
            }
        }

        [RemoteCall("Pickup")]
        public void Pickup(Message msg)
        {
            ushort boxid = msg.GetUShort();

            if(Entity.HasEntityByID(boxid))
            {
                NetworkDestroy(boxid);

                boxes++;
                RC rc = PrepareRC("BoxDelivered");
                rc.Send();

                if(boxes >= 3)
                {
                    stage = 2;
                    RC r = PrepareRC("Stage");
                    r.Send();
                }

            }

        }
    }
}
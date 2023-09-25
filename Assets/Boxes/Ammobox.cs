using Net;
using RiptideNetworking;
using System.Collections;
using UnityEngine;

namespace Assets.Boxes
{
    public class Ammobox : Entity
    {
        protected override void NetworkAwake(Message args) 
        {
            if(IsMine())
            {
                transform.position = new Vector3(Random.Range(-20F, 20F), transform.position.y, Random.Range(-20F, 20F));
            }
        }

        private void Update()
        {
            if(IsMine())
            {
                RaycastHit[] hits = Physics.BoxCastAll(transform.position, Vector3.one, Vector3.up);
                foreach (var item in hits)
                {
                    if (item.collider.gameObject.TryGetComponent<Player>(out Player p))
                    {
                        p.PrepareRC("Ammo").Send();

                        gameObject.SetActive(false);

                        RC rc = Service.PrepareRC("Destroy");
                        rc.message.AddUShort(GetNetworkID());
                        rc.Send();
                    }
                }
            }
        }

    }
}
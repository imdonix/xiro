using Net;
using RiptideNetworking;
using UnityEngine;

public class Truck : Entity
{


    public Transform droppoint;

    Vector3 stop;

    protected override void NetworkAwake(Message args) 
    {
        stop = new Vector3();

        if(IsMine())
        {
            transform.position = transform.position + (transform.forward * 50);
        }
    }

    private void Update()
    {
        if(IsMine())
        {
            if(Vector3.Distance(stop, transform.position) < 1)
            {
                Box[] boxs = FindObjectsByType<Box>(FindObjectsSortMode.None);
                foreach (var item in boxs)
                {
                    if (Vector3.Distance(item.transform.position, droppoint.position) < 2)
                    {
                        RC rc = Service.PrepareRC("Pickup");
                        rc.message.AddUShort(item.GetNetworkID());
                        rc.Send();
                    }
                }
            }
            else
            {
                Vector3 dir = (stop - transform.position).normalized;
                transform.position += dir * Time.deltaTime * Mathf.Min(5, Mathf.Max(20, Vector3.Distance(stop, transform.position) / 2));
            }
        }
    }

}

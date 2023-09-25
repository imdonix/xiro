using Net;
using RiptideNetworking;
using System.Collections;
using UnityEngine;


public class Box : Entity, IStream
{

    public LineRenderer line;
    Player linked = null;
        
    protected override void NetworkAwake(Message args)
    {
        if (IsMine())
        {
            transform.position = new Vector3(Random.Range(-20F, 20F), transform.position.y, Random.Range(-20F, 20F));
        }
    }

    private void Update()
    {
        if (IsMine())
        {
            if (linked)
            {
                line.SetPositions(new Vector3[] { transform.position, linked.transform.position });
                Vector3 dir = (linked.transform.position - transform.position);
                dir.y = 0;
                dir = dir.normalized;
                float dist = Vector3.Distance(transform.position, linked.transform.position);

                if (dist < 2)
                {
                    //Do nothing
                }
                else if (dist < 5)
                {
                    transform.position += dir * Time.deltaTime * 2;
                }
                else
                {
                    linked = null;
                }
            }
            else
            {
                line.SetPositions(new Vector3[] { transform.position, transform.position });
            }
        }
    }

    public override void OnReceive(Message message)
    {
        line.SetPositions(new Vector3[] { transform.position, message.GetVector3() });
    }

    public override void OnSend(Message message)
    {
        message.AddVector3(line.GetPosition(1));
    }




    [RemoteCall("Attach")]
    public void Attach(Message msg)
    {
        ushort requester = msg.GetUShort();

        if(linked == null)
        {
            linked = (Player) Entity.GetEntityByID(requester);
            Debug.Log($"Attachd to {requester}");
        }
        else
        {
            linked = null;
        }
    }


}

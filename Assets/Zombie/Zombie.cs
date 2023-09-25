using Net;
using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Entity
{
    public float WalkSpeed = 10;
    public float RotationSpeed = 5;

    private readonly static List<Vector3> zombiespawns = new List<Vector3>() { new Vector3(1, 0, 1) * 15, new Vector3(-1, 0, 1) * 15, new Vector3(1, 0, -1) * 15, new Vector3(-1, 0, -1) * 15 };

    [SerializeField] private Animator Animator;
    private Player target;

    private float recieved;
    private Vector3 randomLoc = Vector3.zero;
    private float atcd;

    protected override void NetworkAwake(Message args)
    {
        int idx = Random.Range(0, 4);
        Teleport(zombiespawns[idx] + new Vector3(Random.Range(-3F, 3F), 0, Random.Range(-3F, 3F)));
    }

    private void Update()
    {
        if(IsMine())
        {
            recieved -= Time.deltaTime;

            Player[] player = GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None);
            if (player.Length == 0)
            {
                target = null;
            }
            else
            {
                int idx = 0;
                float dis = Vector3.Distance(player[idx].transform.position, transform.position);
                for (int i = 1; i < player.Length; i++)
                {
                    if (dis > Vector3.Distance(player[i].transform.position, transform.position))
                    {
                        idx = i;
                        dis = Vector3.Distance(player[i].transform.position, transform.position);
                    }
                }

                if(dis > 15)
                {
                    target = null;
                }
                else
                {
                    target = player[idx];
                }
            }

            var walki = false;
            var attack = false;
            if (!target)
            {
                target = null;
            }

            if (recieved < 0 && target)
            {
                if ((target.transform.position - transform.position).magnitude > 1.25F)
                {
                    var dir = MoveTowardTarget(target.transform.position);
                    RotateTowardMovementVector(dir);
                    walki = true;
                }
                else
                {
                    if(atcd > 1)
                    {
                        target.PrepareRC("Damage").Send();
                        walki = false;
                        attack = true;
                        atcd = 0;
                    }

                }
            }
            else if (recieved < 0 && !target)
            {
                var dir = MoveTowardTarget(randomLoc);
                RotateTowardMovementVector(dir);
                walki = true;

                if(Vector3.Distance(randomLoc, transform.position) < 0.15F)
                {
                    randomLoc = transform.position + new Vector3(Random.Range(-5, 5F), 0, Random.Range(-5, 5F));
                    randomLoc = new Vector3(Mathf.Clamp(randomLoc.x, -30, 30), 0, Mathf.Clamp(randomLoc.z, -30, 30));
                }
            }

            

            Animator.SetBool("damage", recieved > 0);
            Animator.SetBool("walk", walki);
            Animator.SetBool("attack", attack);

            atcd += Time.deltaTime;
        }
    }

    private Vector3 MoveTowardTarget(Vector3 targetLoc)
    {
        var speed = WalkSpeed * Time.deltaTime;

        var targetVector = (targetLoc - transform.position).normalized; 
        var targetPosition = transform.position + targetVector * speed;
        transform.position = targetPosition;
        return targetVector;
    }


    private void RotateTowardMovementVector(Vector3 movementDirection)
    {
        if (movementDirection.magnitude == 0) { return; }
        var rotation = Quaternion.LookRotation(movementDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, RotationSpeed);
    }

    private int hp = 5;

    [RemoteCall("Damage")]
    public void Damage(Message msg)
    {
        hp--;
        recieved = 0.55f;

        if(hp < 0)
        {
            RC rc = Service.PrepareRC("Destroy");
            rc.message.AddUShort(GetNetworkID());
            rc.Send();

            rc = Service.PrepareRC("SpawnAmmo");
            rc.message.AddVector3(transform.position);
            rc.Send();
        }
    }


    public void Teleport(Vector3 msg)
    {
        transform.position = msg;
        randomLoc = transform.position + new Vector3(Random.Range(-5, 5F), 0, Random.Range(-5, 5F));
    }
}

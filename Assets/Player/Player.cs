using Assets;
using Net;
using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{

    public Vector2 InputVector;
    public Vector3 MousePosition;
    public bool isAttacking;
    public bool isRunning;
    public bool isBoxing;


    [SerializeField] private float Walkpeed;
    [SerializeField] private float RunSpeed;
    [SerializeField] private float RotationSpeed;
    [SerializeField] private float FireRate;
    [SerializeField] private float StartFire;

    [SerializeField] private AudioSource M4Sound;
    [SerializeField] private Camera Camera;
    [SerializeField] private Animator Animator;
    [SerializeField] private LineRendererHelper ShootLine;
    [SerializeField] private Transform EndOfWeapon;
    [SerializeField] private NetworkShooter shooter;
    
    private int hp = 100;
    private int mag = 5;
    private int ammo = 30;


    protected override void NetworkAwake(Message args)
    {
        Camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

        List<Box> ox = new List<Box>(FindObjectsByType<Box>(FindObjectsSortMode.None));
        ox.Sort((a, b) => Vector3.Distance(a.transform.position, transform.position) > Vector3.Distance(b.transform.position, transform.position) ? 1 : -1);


        if (IsMine())
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            InputVector = new Vector2(h, v);

            isAttacking = Input.GetMouseButton(0);
            isRunning = Input.GetKey(KeyCode.LeftShift);
            isBoxing = Input.GetKeyDown(KeyCode.E);

            MousePosition = Input.mousePosition;

            var targetVector = new Vector3(InputVector.x, 0, InputVector.y);


            Animator.SetBool("walk", targetVector.normalized.magnitude > 0.005F && !isRunning);
            Animator.SetBool("run", targetVector.normalized.magnitude > 0.005F && isRunning);
            Animator.SetBool("attacking", isAttacking && ammo > 0);

            if (isAttacking)
            {
                var sway = Random.Range(-2.5f, 2.5f);

                if (cooldown < 0 & startF > StartFire & ammo > 0)
                {
                    ammo--;
                    cooldown = FireRate;
                    shooter.Shoot(sway, EndOfWeapon.position, transform.position, transform.forward);

                    var endofshoot = transform.position + (Quaternion.Euler(0, sway, 0) * transform.forward) * 14;
                    Ray ray = new Ray(EndOfWeapon.position, (endofshoot - EndOfWeapon.position).normalized);
                    var hits = Physics.RaycastAll(ray, 25);
                    foreach (var item in hits)
                    {
                        if (item.collider.TryGetComponent<Zombie>(out Zombie z))
                        {
                            z.PrepareRC("Damage").Send();
                        }
                    }
                }
                else if(cooldown < 0 && startF > StartFire && ammo <= 0 && mag > 0)
                {
                    cooldown = 1.3F;
                    ammo = 30;
                    shooter.Reload(transform.position);
                    mag--;
                }


                RotateFromMouseVector();
                startF += Time.deltaTime;
            }
            else if(isBoxing)
            { 
                if(ox.Count > 0)
                {
                    if (Vector3.Distance(ox[0].transform.position, transform.position) < 3)
                    {
                        RC rc = ox[0].PrepareRC("Attach");
                        rc.message.AddUShort(GetNetworkID());
                        rc.Send();
                    }
                }
            }
            else
            {
                var movementVector = MoveTowardTarget(targetVector);
                RotateTowardMovementVector(movementVector);
                startF = 0;
            }

            cooldown -= Time.deltaTime;


            UI.Show(hp, mag, cooldown);
        }
    }

    private void RotateFromMouseVector()
    {
        Ray ray = Camera.ScreenPointToRay(MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f))
        {
            var target = hitInfo.point;
            target.y = transform.position.y;
            transform.LookAt(target);
        }
    }

    private Vector3 MoveTowardTarget(Vector3 targetVector)
    {
        var speed = (isRunning ? RunSpeed : Walkpeed) * Time.deltaTime;

        targetVector = Quaternion.Euler(0, Camera.gameObject.transform.rotation.eulerAngles.y, 0) * targetVector;
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


    private float cooldown;
    private float startF;


    [RemoteCall("Damage")]
    public void Damage(Message msg)
    {
        hp -= 5;

        if(hp <= 0)
        {
            RC rc = Service.PrepareRC("Destroy");
            rc.message.AddUShort(GetNetworkID());
            rc.Send();
        }
    }

    [RemoteCall("Ammo")]
    public void Ammo(Message msg)
    {
        mag += 5;
    }

    [RemoteCall("AS")]
    public void AS(Message msg)
    {
        FireRate -= 0.025F;
        FireRate = Mathf.Max(0.045F, FireRate);

    }
}

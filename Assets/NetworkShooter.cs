using Net;
using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    struct ShootData
    {
        public ushort type;
        public float sway;
        public Vector3 endofweapon;
        public Vector3 position;
        public Vector3 forward;
    }

    public class NetworkShooter : NetworkComponent
    {
        private readonly List<ShootData> _queue = new List<ShootData>();
        private Entity _entity;

        private void Awake()
        {
            _entity = GetComponent<Entity>();
        }

        public override void OnReceive(Message msg)
        {
            int size = msg.GetInt();
            for (int i = 0; i < size; i++)
            {
                ushort type = msg.GetUShort();
                if(type == 0)
                {
                    float sway = msg.GetFloat();
                    Vector3 endofweapon = msg.GetVector3();
                    Vector3 position = msg.GetVector3();
                    Vector3 forward = msg.GetVector3();
                    Spawn(sway, endofweapon, position, forward);
                }
                else
                {
                    msg.GetFloat();
                    Vector3 endofweapon = msg.GetVector3();
                    msg.GetVector3();
                    msg.GetVector3();
                    GameObject.Instantiate(GameManager.Instance.ReloadSound, endofweapon, Quaternion.identity);
                }
            }
        }

        public override void OnSend(Message message)
        {
            int size = _queue.Count;
            message.AddInt(size);
            for (int i = 0; i < size; i++)
            {
                ShootData d = _queue[i];
                message.AddUShort(d.type);
                message.AddFloat(d.sway);
                message.AddVector3(d.endofweapon);
                message.AddVector3(d.position);
                message.AddVector3(d.forward);
            }

            _queue.Clear();
        }

        public void Shoot(float sway, Vector3 endofweapon, Vector3 position, Vector3 forward)
        {
            Spawn(sway, endofweapon, position, forward);
            _queue.Add(new ShootData() { type = 0, sway = sway, endofweapon = endofweapon, position = position, forward = forward });
        }

        public void Reload(Vector3 pos)
        {
            GameObject.Instantiate(GameManager.Instance.ReloadSound, pos, Quaternion.identity);
            _queue.Add(new ShootData() { type = 1, endofweapon = pos});
        }



        private void Spawn(float sway, Vector3 endofweapon, Vector3 position, Vector3 forward)
        {
            var endofshoot = position + (Quaternion.Euler(0, sway, 0) * forward) * 14;
            LineRendererHelper rendered = GameObject.Instantiate(GameManager.Instance.ShootLine).GetComponent<LineRendererHelper>();
            rendered.SetPositions(new Vector3[] { endofweapon, endofshoot });
        }

    }
}
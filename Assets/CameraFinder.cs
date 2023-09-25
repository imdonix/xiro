using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFinder : MonoBehaviour
{

    public CinemachineVirtualCamera _cam;


    void Update()
    {
        bool found = false;

        Player[] p = GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (var item in p)
        {
            if(item.IsMine())
            {
                _cam.Follow = item.transform;
                _cam.LookAt = item.transform;
                found = true;
            }
        }

        if(!found)
        {
            foreach (var item in p)
            {

                _cam.Follow = item.transform;
                _cam.LookAt = item.transform;
                found = true;

            }
        }
    }

}

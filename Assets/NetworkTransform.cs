using Net;
using RiptideNetworking;
using System.Collections;
using UnityEngine;


/// <summary>
/// A 2D networked transform component
/// </summary>
public class NetworkTransform : NetworkComponent {

  public override void OnReceive(Message message) {
    float x = message.GetFloat();
    float z = message.GetFloat();
    float r = message.GetFloat();
    transform.position = new Vector3(x, transform.position.y, z);
    transform.rotation = Quaternion.Euler(0, r, 0);
  }

  public override void OnSend(Message message) {
    message.AddFloat(transform.position.x);
    message.AddFloat(transform.position.z);
    message.AddFloat(transform.rotation.eulerAngles.y);
  }

}

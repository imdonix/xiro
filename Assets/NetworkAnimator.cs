using Net;
using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAnimator : NetworkComponent {

  private Animator _animator;
  private List<AnimatorControllerParameter> _sorted;

  #region UNITY

  private void Awake() {
    _animator = GetComponent<Animator>();
    _sorted = new List<AnimatorControllerParameter>(_animator.parameters);
  }

  #endregion

  public override void OnReceive(Message message) {
    foreach (var param in _sorted) {
      if (param.type == AnimatorControllerParameterType.Bool) {
        _animator.SetBool(param.name ,message.GetBool());
      }
      else if (param.type == AnimatorControllerParameterType.Float) {
        _animator.SetFloat(param.name, message.GetFloat());
      }
      else {
        Debug.LogError($"Networkanimator can't handle type: {param.type}");
      }
    }
  }

  public override void OnSend(Message message) {
    foreach (var param in _sorted) {
      if (param.type == AnimatorControllerParameterType.Bool) {
        message.AddBool(_animator.GetBool(param.name));
      }
      else if (param.type == AnimatorControllerParameterType.Float) {
        message.AddFloat(_animator.GetFloat(param.name));
      }
      else {
        Debug.LogError($"Networkanimator can't handle type: {param.type}");
      }
    }
  }
}

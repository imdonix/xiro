using System.Collections.Generic;
using UnityEngine;

namespace Utils {
  public static class CachedResource {
    private readonly static Dictionary<string, Object> _cache = new Dictionary<string, Object>();

    public static T Load<T>(string path) where T : Object {
      if (!_cache.ContainsKey(path)) {
        _cache[path] = Resources.Load<T>(path);
        if (ReferenceEquals(_cache[path], null)) {
          throw new System.Exception($"Resource can't be loaded: {path}");
        }
      }
      return (T)_cache[path];
    }

    public static Sprite LoadSprite(string path) {
      if (!_cache.ContainsKey(path)) {
        Texture2D tex = Resources.Load<Texture2D>(path);
        if (ReferenceEquals(tex, null)) {
          throw new System.Exception($"Resource can't be loaded: {path}");
        }

        _cache[path] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one / 2);
      }
      return (Sprite)_cache[path];
    }

    public static void Reset() {
      _cache.Clear();
    }
  }
}
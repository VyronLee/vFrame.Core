using UnityEngine;

namespace vFrame.Core.Utils.Unity
{
    public static class GameObjectUtils
    {
        public static GameObject RecursiveCreateGameObject(string fullPath) {
            Transform last = null;
            var paths = fullPath.Split('/');
            foreach (var path in paths) {
                if (string.IsNullOrEmpty(path)) {
                    continue;
                }

                GameObject root = null;
                if (null != last) {
                    var rootTransform = last.Find(path);
                    if (rootTransform) {
                        root = rootTransform.gameObject;
                    }
                }
                else {
                    root = GameObject.Find(path);
                }

                if (null == root) {
                    root = new GameObject(path);
                }
                root.transform.SetParent(last, false);
                last = root.transform;
            }

            return null != last ? last.gameObject : null;
        }

    }
}
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Extensions
{
    public static class GameObjectExtension
    {
        public static T GetOrAddComponentRecursively<T>(this GameObject gameObject)
            where T : Component
        {
            return gameObject.GetComponentInChildren<T>() ?? gameObject.AddComponent<T>();
        }

        public static T[] GetOrAddComponentsRecursively<T>(this GameObject gameObject)
            where T : Component
        {
            return gameObject.GetComponentsInChildren<T>() is { Length: > 0 } c
                ? c
                : new[] { gameObject.AddComponent<T>() };
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            var transforms = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                t.gameObject.layer = layer;
            }
        }

        public static void SetTagRecursively(this GameObject gameObject, string tag)
        {
            var transforms = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                t.gameObject.tag = tag;
            }
        }

        public static void SetTagAndLayerRecursively(
            this GameObject gameObject,
            string tag,
            int layer
        )
        {
            var transforms = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                t.gameObject.tag = tag;
                t.gameObject.layer = layer;
            }
        }

        public static void SetSortingLayerRecursively(
            this GameObject gameObject,
            string sortingLayer
        )
        {
            var transforms = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                var renderer2D = t.GetComponent<SpriteRenderer>();
                var tilemapRenderer = t.GetComponent<TilemapRenderer>();

                if (renderer2D != null)
                    renderer2D.sortingLayerName = sortingLayer;

                if (tilemapRenderer != null)
                    tilemapRenderer.sortingLayerName = sortingLayer;
            }
        }
    }
}

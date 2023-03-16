using System;
using System.Collections.Generic;
using MyBox;
using Slothsoft.UnityExtensions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace FollowYourDreams.Avatar {
    [CreateAssetMenu]
    sealed class AvatarSettings : ScriptableAsset {
        [Header("Setup")]
        [SerializeField]
        GameObject prefab;

        [Header("Movement")]
        [SerializeField, Range(0, 10)]
        public float rotationSmoothing = 0.1f;
        [SerializeField, Range(0, 10)]
        public float speedSmoothing = 0.1f;
        [SerializeField, Range(0, 10)]
        public float walkSpeed = 1;
        [SerializeField, Range(0, 10)]
        public float runSpeed = 2;

        [Space]
        [SerializeField, Range(0, 10)]
        public float jumpSpeed = 5;
        [SerializeField, Range(0, 10)]
        public float jumpStopMultiplier = 0.25f;
        [SerializeField, Range(0, 10)]
        public float jumpGravityMultiplier = 0.25f;

        [Space]
        [SerializeField, Range(0, 10)]
        public float glideVerticalBoost = 1;
        [SerializeField, Range(0, 10)]
        public float glideHorizontalBoost = 1;
        [SerializeField, Range(0, 10)]
        public float glideGravityMultiplier = 0.1f;
        [SerializeField, Range(0, 10)]
        public float glideSmoothing = 0.1f;

#if UNITY_EDITOR
        const int DIRECTION_COUNT = 5;
        [Header("Editor-only")]
        [SerializeField]
        AnimatorController controller;
        [SerializeField]
        TextAsset json;
        [SerializeField]
        Texture2D sheet;
        [SerializeField]
        Vector2 pivot = new(0.5f, 0.5f);
        [SerializeField]
        SerializableKeyValuePairs<AvatarAnimation, bool> isLoopingOverride = new();

        [Header("Auto-filled")]
        [SerializeField, ReadOnly]
        List<Sprite> sprites = new();
        Sprite GetSprite(int index, AvatarDirection direction) {
            return sprites[(index * DIRECTION_COUNT) + (int)direction];
        }
        [SerializeField, ReadOnly]
        AsepriteData data = new();
        [SerializeField, ReadOnly]
        Animator animatorPrefab;
        [SerializeField, ReadOnly]
        SpriteRenderer rendererPrefab;

        [ContextMenu(nameof(LoadPrefab))]
        void LoadPrefab() {
            prefab.TryGetComponent(out animatorPrefab);
            prefab.TryGetComponent(out rendererPrefab);
        }

        [ContextMenu(nameof(LoadData))]
        void LoadData() {
            data = AsepriteData.FromJson(json.text);
        }

        [ContextMenu(nameof(LoadSprites))]
        void LoadSprites() {
            sheet.ExtractSprites(data, pivot, this, sprites, DIRECTION_COUNT);

            if (rendererPrefab) {
                rendererPrefab.sprite = GetSprite(0, AvatarDirection.Down);
                EditorUtility.SetDirty(rendererPrefab);
            }

            AssetDatabase.SaveAssets();
        }

        [ContextMenu(nameof(LoadController))]
        void LoadController() {
            var addTransition = controller.ImportAnimations(data, isLoopingOverride, sprites, GetAnimationName, DIRECTION_COUNT);

            addTransition(AvatarAnimation.Land, AvatarAnimation.Idle);

            if (animatorPrefab) {
                animatorPrefab.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(animatorPrefab);
            }

            AssetDatabase.SaveAssets();
        }

        [ContextMenu(nameof(LoadAll))]
        void LoadAll() {
            LoadPrefab();
            LoadData();
            LoadSprites();
            LoadController();
        }
#endif

        public static string GetAnimationName(AvatarDirection direction, AvatarAnimation animation) {
            return $"{direction}_{animation}";
        }
        static string GetAnimationName(AvatarAnimation animation, int direction) {
            return GetAnimationName((AvatarDirection)direction, animation);
        }
    }
}

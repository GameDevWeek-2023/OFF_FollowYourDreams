using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace FollowYourDreams.Avatar {
    [CreateAssetMenu]
    sealed class AvatarSettings : ScriptableAsset {
        [SerializeField]
        GameObject prefab;

#if UNITY_EDITOR
        const int DIRECTION_COUNT = 5;
        const float TIME_MULTIPLIER = 0.001f;
        [Header("Animations")]
        [SerializeField]
        AnimatorController controller;
        [SerializeField]
        Texture2D sheet;
        [SerializeField]
        Sprite[] sprites = Array.Empty<Sprite>();

        Sprite GetSprite(int index, AvatarDirection direction) {
            return sprites[(index * DIRECTION_COUNT) + (int)direction];
        }

        [SerializeField]
        TextAsset json;
        [SerializeField]
        AsepriteData data = new();
        [SerializeField]
        Animator animatorPrefab;

        [ContextMenu(nameof(LoadPrefab))]
        void LoadPrefab() {
            prefab.TryGetComponent(out animatorPrefab);
        }

        [ContextMenu(nameof(LoadSheet))]
        void LoadSheet() {
            LoadPrefab();

            sprites = sheet.LoadSprites();

            data = AsepriteData.FromJson(json.text);

            string path = AssetDatabase.GetAssetPath(controller);

            var animClips = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>();

            foreach (var animClip in animClips) {
                AssetDatabase.RemoveObjectFromAsset(animClip);
            }

            var layer = controller.layers[0];
            foreach (var state in layer.stateMachine.states) {
                layer.stateMachine.RemoveState(state.state);
            }

            foreach (AvatarDirection direction in Enum.GetValues(typeof(AvatarDirection))) {
                foreach (var anim in data.meta.frameTags) {
                    if (!Enum.TryParse<AvatarAnimation>(anim.name, out var animation)) {
                        Debug.LogError($"Unknown animation '{anim.name}'!");
                        continue;
                    }

                    var animClip = new AnimationClip() {
                        name = GetAnimationName(direction, animation),
                        wrapMode = WrapMode.Loop,
                    };

                    var settings = AnimationUtility.GetAnimationClipSettings(animClip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(animClip, settings);

                    var keyframes = new List<ObjectReferenceKeyframe>();
                    int time = 0;
                    for (int i = anim.from; i <= anim.to; i++) {
                        keyframes.Add(new() {
                            time = time * TIME_MULTIPLIER,
                            value = GetSprite(i, direction)
                        });
                        time += data[i].duration;
                    }
                    keyframes.Add(new() {
                        time = time * TIME_MULTIPLIER,
                        value = GetSprite(anim.from, direction)
                    });

                    AnimationUtility.SetObjectReferenceCurve(
                        animClip,
                        EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
                        keyframes.ToArray()
                    );

                    AssetDatabase.AddObjectToAsset(animClip, path);

                    controller.AddMotion(animClip);
                }
            }

            AssetDatabase.SaveAssets();
        }

        public static string GetAnimationName(AvatarDirection direction, AvatarAnimation animation) {
            return $"{direction}_{animation}";
        }
    }
#endif
}

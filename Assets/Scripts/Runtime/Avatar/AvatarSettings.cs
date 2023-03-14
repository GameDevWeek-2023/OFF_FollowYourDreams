using System;
using Newtonsoft.Json;
using UnityEngine;

namespace FollowYourDreams.Avatar {
    [CreateAssetMenu]
    sealed class AvatarSettings : ScriptableAsset {
        [SerializeField]
        GameObject prefab;

#if UNITY_EDITOR
        [Header("Animations")]
        [SerializeField]
        Texture2D sheet;
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
            data = JsonConvert.DeserializeObject<AsepriteData>(json.text);
            Debug.Log(data.frames["S_Avatar 0.aseprite"]);
        }
#endif
    }
}

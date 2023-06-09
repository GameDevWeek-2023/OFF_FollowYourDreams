using System.Collections.Generic;
using FollowYourDreams.Graphics;
using UnityEngine;

namespace FollowYourDreams {
    sealed class AsepriteSprite : ComponentFeature<SpriteRenderer>, IImportable {
        [SerializeField]
        List<Sprite> sprites = new();
        [SerializeField]
        int spriteOffset = 0;
        public int currentSpriteId {
            get => sprites.IndexOf(observedComponent.sprite);
            set {
                observedComponent.sprite = sprites[spriteOffset + value];
            }
        }

#if UNITY_EDITOR
        [Header("Editor-only")]
        [SerializeField]
        TextAsset json = default;
        [SerializeField]
        Texture2D sheet = default;
        [SerializeField]
        Vector2 pivot = new(0.5f, 0.5f);
        public Vector2 spritePivot {
            get => pivot;
            set => pivot = value;
        }
        [SerializeField]
        AsepriteData data = new();

        [ContextMenu(nameof(LoadAll))]
        public void LoadAll() {
            if (sheet && json) {
                data = AsepriteData.FromJson(json.text);
                sheet.ExtractSprites(data, pivot, gameObject, sprites);
                currentSpriteId = 0;
            }
        }
#endif
    }
}

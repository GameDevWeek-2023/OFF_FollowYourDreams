using System;
using System.Collections;
using FMODUnity;
using Slothsoft.UnityExtensions;
using UnityEngine;

namespace FollowYourDreams.Avatar {
    sealed class BedState : MonoBehaviour, IInteractable {
        public event Action<BedAnimation> onSetAnimation;

        [Header("Config")]
        [SerializeField, Expandable]
        GameManager manager;
        [SerializeField]
        Transform interactionSpot;

        [Header("Go to sleep")]
        [SerializeField, Range(0, 10)]
        float gotoSleepStart = 1;
        [SerializeField, Range(0, 10)]
        float gotoSleepMiddle = 1;
        [SerializeField, Range(0, 10)]
        float gotoSleepEnd = 1;
        [SerializeField]
        EventReference goToSleepEvent = new();

        [Header("Wake up")]
        [SerializeField, Range(0, 10)]
        float dreamAbortStart = 1;
        [SerializeField, Range(0, 10)]
        float dreamAbortMiddle = 1;
        [SerializeField, Range(0, 10)]
        float dreamAbortEnd = 1;
        [SerializeField]
        EventReference wakeUpEvent = new();

        [Header("Runtime")]
        [SerializeField]
        bool isOccupied = false;

        Dimension currentDimension {
            get => manager.currentDimension;
            set => manager.currentDimension = value;
        }

        public int priority => 0;

        public bool isSelectable => true;

        public void Select() {
            bool gotoSleep = !isOccupied;
            onSetAnimation?.Invoke(gotoSleep ? BedAnimation.BedEmptySelected : BedAnimation.DreamSleepSelected);
        }
        public void Deselect() {
            bool gotoSleep = !isOccupied;
            onSetAnimation?.Invoke(gotoSleep ? BedAnimation.BedEmpty : BedAnimation.DreamSleep);
        }
        public IEnumerator Interact_Co(AvatarController avatar) {
            bool gotoSleep = !isOccupied;
            avatar.currentAnimation = AvatarAnimation.None;

            onSetAnimation?.Invoke(gotoSleep ? BedAnimation.GoToSleep : BedAnimation.DreamAbort);
            yield return Wait.forSeconds[gotoSleep ? gotoSleepStart : dreamAbortStart];

            if (gotoSleep) {
                goToSleepEvent.PlayOnce();
            } else {
                wakeUpEvent.PlayOnce();
            }

            currentDimension = currentDimension switch {
                Dimension.RealWorld when isOccupied => Dimension.RealWorld,
                Dimension.RealWorld => Dimension.Dreamscape,
                Dimension.Dreamscape when isOccupied => Dimension.RealWorld,
                Dimension.Dreamscape => Dimension.NightmareRealm,
                Dimension.NightmareRealm when isOccupied => Dimension.NightmareRealm,
                Dimension.NightmareRealm => Dimension.Dreamscape,
                _ => throw new NotImplementedException(),
            };
            yield return Wait.forFixedUpdate;

            avatar.AddBed(this);
            avatar.WarpTo(interactionSpot.position, Direction.UpLeft);
            isOccupied = !isOccupied;
            yield return Wait.forSeconds[gotoSleep ? gotoSleepMiddle : dreamAbortMiddle];

            onSetAnimation?.Invoke(gotoSleep ? BedAnimation.DreamUp : BedAnimation.WakeUp);
            yield return Wait.forSeconds[gotoSleep ? gotoSleepEnd : dreamAbortEnd];

            avatar.currentAnimation = AvatarAnimation.Idle;
            onSetAnimation?.Invoke(gotoSleep ? BedAnimation.DreamSleep : BedAnimation.BedEmpty);
        }

        public IEnumerator WakeUpIn_Co(AvatarController avatar) {
            currentDimension = currentDimension switch {
                Dimension.RealWorld => Dimension.RealWorld,
                Dimension.Dreamscape when isOccupied => Dimension.RealWorld,
                Dimension.Dreamscape => Dimension.Dreamscape,
                Dimension.NightmareRealm when isOccupied => Dimension.Dreamscape,
                Dimension.NightmareRealm => Dimension.NightmareRealm,
                _ => throw new NotImplementedException(),
            };
            yield return Wait.forFixedUpdate;

            avatar.currentAnimation = AvatarAnimation.None;
            avatar.WarpTo(interactionSpot.position, Direction.UpLeft);
            isOccupied = false;
            yield return Wait.forSeconds[dreamAbortMiddle];

            onSetAnimation?.Invoke(BedAnimation.WakeUp);
            yield return Wait.forSeconds[dreamAbortEnd];

            avatar.currentAnimation = AvatarAnimation.Idle;
            onSetAnimation?.Invoke(BedAnimation.BedEmpty);
        }
    }
}

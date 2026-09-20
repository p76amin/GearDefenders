using UnityEngine;
using UnityEngine.UI;

namespace GearDefenders
{
    public class UiSpriteAnimator : MonoBehaviour
    {
        Image _image;
        CombatantAnimSet _set;
        CombatantAnimState _locomotion = CombatantAnimState.Idle;
        CombatantAnimState _playing = CombatantAnimState.Idle;
        bool _oneShot;
        int _frame;
        float _accum;
        System.Action _onOneShotDone;
        bool _deathFinished;

        public bool IsPlayingDeath => _playing == CombatantAnimState.Death;

        public void Bind(Image image, CombatantAnimSet set)
        {
            _image = image;
            _set = set;
            _oneShot = false;
            _deathFinished = false;
            _playing = CombatantAnimState.Idle;
            _locomotion = CombatantAnimState.Idle;
            _frame = 0;
            _accum = 0f;
            ApplyFrame();
        }

        public void SetMoving(bool moving)
        {
            _locomotion = moving ? CombatantAnimState.Walk : CombatantAnimState.Idle;
            if (!_oneShot && !IsPlayingDeath)
            {
                var next = _locomotion;
                if (_playing != next)
                {
                    _playing = next;
                    _frame = 0;
                    _accum = 0f;
                    ApplyFrame();
                }
            }
        }

        public void PlayAttack()
        {
            PlayOneShot(CombatantAnimState.Attack, null);
        }

        public void PlayHurt()
        {
            PlayOneShot(CombatantAnimState.Hurt, null);
        }

        public void PlayDeath(System.Action onComplete)
        {
            _deathFinished = false;
            PlayOneShot(CombatantAnimState.Death, onComplete);
        }

        void PlayOneShot(CombatantAnimState state, System.Action onComplete)
        {
            if (IsPlayingDeath && state != CombatantAnimState.Death)
                return;

            var frames = _set != null ? _set.Frames(state) : null;
            if (frames == null || frames.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _onOneShotDone = onComplete;
            _oneShot = true;
            _playing = state;
            _frame = 0;
            _accum = 0f;
            ApplyFrame();
        }

        void Update()
        {
            if (_image == null || _set == null)
                return;

            var frames = _set.Frames(_playing);
            if (frames == null || frames.Length == 0)
                return;

            float fps = Mathf.Max(0.01f, _set.Fps(_playing));
            _accum += Time.deltaTime * fps;
            while (_accum >= 1f)
            {
                _accum -= 1f;
                Advance(frames);
            }

            ApplyFrame();
        }

        void Advance(Sprite[] frames)
        {
            if (_playing == CombatantAnimState.Death)
            {
                if (_frame < frames.Length - 1)
                    _frame++;
                else if (!_deathFinished)
                {
                    _deathFinished = true;
                    _oneShot = false;
                    var cb = _onOneShotDone;
                    _onOneShotDone = null;
                    cb?.Invoke();
                }

                return;
            }

            _frame++;
            if (_frame < frames.Length)
                return;

            if (_oneShot)
            {
                _oneShot = false;
                _playing = _locomotion;
                _frame = 0;
                var cb = _onOneShotDone;
                _onOneShotDone = null;
                cb?.Invoke();
                return;
            }

            _frame = 0;
        }

        void ApplyFrame()
        {
            if (_image == null || _set == null)
                return;
            var frames = _set.Frames(_playing);
            if (frames == null || frames.Length == 0)
                return;
            int i = Mathf.Clamp(_frame, 0, frames.Length - 1);
            if (frames[i] != null)
                _image.sprite = frames[i];
        }
    }
}

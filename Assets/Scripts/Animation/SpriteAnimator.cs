using System;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Code-driven sprite frame animator. No Unity Animation window dependency.
    /// Call Tick(dt) from Update() — testable with explicit time in EditMode.
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        [Header("Defaults")]
        [SerializeField] private float _defaultFrameRate = 12f;

        private SpriteRenderer _renderer;
        private Sprite[] _currentFrames;
        private int _frameIndex;
        private float _frameTimer;
        private float _frameRate;
        private bool _isPlaying;
        private bool _isLooping;
        private string _currentState = string.Empty;

        /// <summary>Fired when a non-looping animation finishes. Parameter is the state name.</summary>
        public event Action<string> OnAnimationComplete;

        // --- Public state (testable) ---
        public string CurrentState => _currentState;
        public int CurrentFrameIndex => _frameIndex;
        public bool IsPlaying => _isPlaying;
        public bool IsLooping => _isLooping;
        public float FrameRate => _frameRate;
        public int FrameCount => _currentFrames?.Length ?? 0;

        /// <summary>
        /// Initialize with an explicit SpriteRenderer. Safe to call in tests (no Awake dependency).
        /// If not called manually, Awake() will auto-resolve.
        /// </summary>
        public void Init(SpriteRenderer renderer, float defaultFrameRate = 12f)
        {
            _renderer = renderer;
            _defaultFrameRate = defaultFrameRate;
        }

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update() => Tick(Time.deltaTime);

        /// <summary>
        /// Play a named animation state with the given frames.
        /// </summary>
        /// <param name="state">Animation state name (idle, walk, attack, mine, death).</param>
        /// <param name="frames">Sprite frames to play.</param>
        /// <param name="loop">Whether to loop or play once.</param>
        /// <param name="frameRate">Frames per second. If <= 0, uses defaultFrameRate.</param>
        public void Play(string state, Sprite[] frames, bool loop, float frameRate = -1f)
        {
            if (frames == null || frames.Length == 0) return;

            // Skip if same state is already playing (avoid restart)
            if (_isPlaying && _currentState == state && _currentFrames == frames && _isLooping == loop)
                return;

            _currentState = state;
            _currentFrames = frames;
            _isLooping = loop;
            _frameRate = frameRate > 0f ? frameRate : _defaultFrameRate;
            _frameIndex = 0;
            _frameTimer = 0f;
            _isPlaying = true;

            ApplyFrame();
        }

        /// <summary>Stop playback. Holds the current frame.</summary>
        public void Stop()
        {
            _isPlaying = false;
        }

        /// <summary>
        /// Advance animation by dt seconds. Testable entry point — call from Update() or tests.
        /// </summary>
        public void Tick(float dt)
        {
            if (!_isPlaying || _currentFrames == null || _currentFrames.Length == 0)
                return;

            _frameTimer += dt;
            float frameDuration = 1f / _frameRate;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                _frameIndex++;

                if (_frameIndex >= _currentFrames.Length)
                {
                    if (_isLooping)
                    {
                        _frameIndex = 0;
                    }
                    else
                    {
                        // Non-looping: clamp to last frame, stop, fire event
                        _frameIndex = _currentFrames.Length - 1;
                        _isPlaying = false;
                        ApplyFrame();
                        OnAnimationComplete?.Invoke(_currentState);
                        return;
                    }
                }

                ApplyFrame();
            }
        }

        /// <summary>Force-set frame index (for external control / testing).</summary>
        public void SetFrame(int index)
        {
            if (_currentFrames == null || index < 0 || index >= _currentFrames.Length) return;
            _frameIndex = index;
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (_renderer == null || _currentFrames == null) return;
            if (_frameIndex >= 0 && _frameIndex < _currentFrames.Length && _currentFrames[_frameIndex] != null)
                _renderer.sprite = _currentFrames[_frameIndex];
        }
    }
}

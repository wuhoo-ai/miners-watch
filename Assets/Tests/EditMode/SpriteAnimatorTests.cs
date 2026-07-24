using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for SpriteAnimator. Uses Tick(dt) directly — no coroutines, no Awake dependency.
    /// </summary>
    [TestFixture]
    public class SpriteAnimatorTests
    {
        private GameObject _go;
        private SpriteRenderer _renderer;
        private SpriteAnimator _animator;
        private Sprite[] _frames;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestAnimator");
            _renderer = _go.AddComponent<SpriteRenderer>();
            _animator = _go.AddComponent<SpriteAnimator>();
            // Init explicitly — avoids relying on Awake() in batch mode
            _animator.Init(_renderer, defaultFrameRate: 10f);

            // Create test sprites (whiteTexture is 4x4)
            _frames = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                _frames[i] = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0, 0, 4, 4),
                    Vector2.zero);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        // --- State switching tests ---

        [Test]
        public void Play_SetsStateAndStartsPlaying()
        {
            _animator.Play("idle", _frames, loop: true);

            Assert.AreEqual("idle", _animator.CurrentState);
            Assert.IsTrue(_animator.IsPlaying);
            Assert.IsTrue(_animator.IsLooping);
            Assert.AreEqual(0, _animator.CurrentFrameIndex);
        }

        [Test]
        public void Play_SwitchesState_UpdatesCurrentState()
        {
            _animator.Play("idle", _frames, loop: true);
            _animator.Play("walk", _frames, loop: true);

            Assert.AreEqual("walk", _animator.CurrentState);
            Assert.AreEqual(0, _animator.CurrentFrameIndex); // reset on state change
        }

        [Test]
        public void Play_SameStateAndFrames_DoesNotRestart()
        {
            _animator.Play("idle", _frames, loop: true);
            _animator.Tick(0.05f); // advance partially

            int frameBefore = _animator.CurrentFrameIndex;
            _animator.Play("idle", _frames, loop: true); // same call

            // Should not reset frame index
            Assert.AreEqual(frameBefore, _animator.CurrentFrameIndex);
        }

        [Test]
        public void Play_NullFrames_DoesNothing()
        {
            _animator.Play("idle", null, loop: true);
            Assert.IsFalse(_animator.IsPlaying);
        }

        [Test]
        public void Play_EmptyFrames_DoesNothing()
        {
            _animator.Play("idle", new Sprite[0], loop: true);
            Assert.IsFalse(_animator.IsPlaying);
        }

        [Test]
        public void Stop_StopsPlayback()
        {
            _animator.Play("attack", _frames, loop: false);
            Assert.IsTrue(_animator.IsPlaying);

            _animator.Stop();
            Assert.IsFalse(_animator.IsPlaying);
        }

        // --- Frame rate tests ---

        [Test]
        public void FrameRate_DefaultApplied_WhenNotSpecified()
        {
            _animator.Play("idle", _frames, loop: true);
            Assert.AreEqual(10f, _animator.FrameRate); // defaultFrameRate = 10
        }

        [Test]
        public void FrameRate_CustomApplied()
        {
            _animator.Play("attack", _frames, loop: false, frameRate: 20f);
            Assert.AreEqual(20f, _animator.FrameRate);
        }

        [Test]
        public void Tick_AdvancesFrame_AtCorrectRate()
        {
            // 10 fps = 0.1s per frame
            _animator.Play("walk", _frames, loop: true, frameRate: 10f);

            Assert.AreEqual(0, _animator.CurrentFrameIndex);

            _animator.Tick(0.1f); // exactly one frame duration
            Assert.AreEqual(1, _animator.CurrentFrameIndex);

            _animator.Tick(0.1f);
            Assert.AreEqual(2, _animator.CurrentFrameIndex);
        }

        [Test]
        public void Tick_PartialTime_DoesNotAdvanceFrame()
        {
            _animator.Play("walk", _frames, loop: true, frameRate: 10f);

            _animator.Tick(0.05f); // half a frame duration
            Assert.AreEqual(0, _animator.CurrentFrameIndex);
        }

        [Test]
        public void Tick_LargeDelta_AdvancesMultipleFrames()
        {
            _animator.Play("walk", _frames, loop: true, frameRate: 10f);

            _animator.Tick(0.25f); // 2.5 frames at 10fps
            Assert.AreEqual(2, _animator.CurrentFrameIndex);
        }

        // --- Loop tests ---

        [Test]
        public void Loop_WrapsAround_ToFirstFrame()
        {
            _animator.Play("idle", _frames, loop: true, frameRate: 10f);

            // Advance past all 4 frames (0.4s = 4 frames at 10fps)
            _animator.Tick(0.4f);
            Assert.AreEqual(0, _animator.CurrentFrameIndex); // wrapped
            Assert.IsTrue(_animator.IsPlaying); // still playing
        }

        [Test]
        public void Loop_MultipleWraps()
        {
            _animator.Play("idle", _frames, loop: true, frameRate: 10f);

            // 0.9s = 9 frames at 10fps → 9 % 4 = 1
            _animator.Tick(0.9f);
            Assert.AreEqual(1, _animator.CurrentFrameIndex);
            Assert.IsTrue(_animator.IsPlaying);
        }

        [Test]
        public void NonLoop_StopsAtLastFrame()
        {
            _animator.Play("attack", _frames, loop: false, frameRate: 10f);

            // Advance past all frames
            _animator.Tick(0.5f); // 5 frames > 4 total

            Assert.AreEqual(3, _animator.CurrentFrameIndex); // clamped to last
            Assert.IsFalse(_animator.IsPlaying); // stopped
        }

        [Test]
        public void NonLoop_FiresOnComplete()
        {
            string completedState = null;
            _animator.OnAnimationComplete += (s) => completedState = s;

            _animator.Play("death", _frames, loop: false, frameRate: 10f);
            _animator.Tick(0.5f);

            Assert.AreEqual("death", completedState);
        }

        [Test]
        public void NonLoop_DoesNotFireComplete_BeforeFinished()
        {
            bool fired = false;
            _animator.OnAnimationComplete += (_) => fired = true;

            _animator.Play("attack", _frames, loop: false, frameRate: 10f);
            _animator.Tick(0.2f); // only 2 of 4 frames

            Assert.IsFalse(fired);
            Assert.IsTrue(_animator.IsPlaying);
        }

        // --- SpriteRenderer integration ---

        [Test]
        public void Play_AppliesFirstFrame_ToRenderer()
        {
            _animator.Play("idle", _frames, loop: true);
            Assert.AreEqual(_frames[0], _renderer.sprite);
        }

        [Test]
        public void Tick_UpdatesRendererSprite()
        {
            _animator.Play("walk", _frames, loop: true, frameRate: 10f);
            _animator.Tick(0.1f);

            Assert.AreEqual(_frames[1], _renderer.sprite);
        }

        // --- Edge cases ---

        [Test]
        public void Tick_WhenNotPlaying_DoesNothing()
        {
            _animator.Play("idle", _frames, loop: true, frameRate: 10f);
            _animator.Stop();

            int frameBefore = _animator.CurrentFrameIndex;
            _animator.Tick(1f);

            Assert.AreEqual(frameBefore, _animator.CurrentFrameIndex);
        }

        [Test]
        public void FrameCount_ReflectsCurrentFrames()
        {
            Assert.AreEqual(0, _animator.FrameCount);
            _animator.Play("idle", _frames, loop: true);
            Assert.AreEqual(4, _animator.FrameCount);
        }

        [Test]
        public void SetFrame_ChangesCurrentFrame()
        {
            _animator.Play("idle", _frames, loop: true);
            _animator.SetFrame(2);

            Assert.AreEqual(2, _animator.CurrentFrameIndex);
            Assert.AreEqual(_frames[2], _renderer.sprite);
        }

        [Test]
        public void SingleFrame_Loop_StaysOnFrame()
        {
            var single = new[] { _frames[0] };
            _animator.Play("idle", single, loop: true, frameRate: 10f);

            _animator.Tick(0.5f);
            Assert.AreEqual(0, _animator.CurrentFrameIndex);
            Assert.IsTrue(_animator.IsPlaying);
        }

        [Test]
        public void SingleFrame_NonLoop_CompletesImmediately()
        {
            var single = new[] { _frames[0] };
            bool fired = false;
            _animator.OnAnimationComplete += (_) => fired = true;

            _animator.Play("death", single, loop: false, frameRate: 10f);
            _animator.Tick(0.1f);

            Assert.IsFalse(_animator.IsPlaying);
            Assert.IsTrue(fired);
        }
    }
}

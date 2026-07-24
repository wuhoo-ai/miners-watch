using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace Tests.EditMode
{
    [TestFixture]
    public class PlayerControllerTests
    {
        private GameObject _go;
        private PlayerController _player;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _player = _go.AddComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void CoyoteTimer_WhenGrounded_ResetsToMax()
        {
            // Act - grounded for 0.02s
            _player.TryJumpTest(0.02f, grounded: true);

            // Assert
            Assert.Greater(_player.CoyoteTimer, 0.08f); // should be close to 0.1s
        }

        [Test]
        public void CoyoteTimer_WhenAirborne_Decays()
        {
            // Arrange - start grounded, then go airborne
            _player.TryJumpTest(0.02f, grounded: true);
            float timerAfterGrounded = _player.CoyoteTimer;

            // Act - simulate airborne for 0.05s
            _player.TryJumpTest(0.05f, grounded: false);

            // Assert
            Assert.Less(_player.CoyoteTimer, timerAfterGrounded);
        }

        [Test]
        public void JumpBuffer_WhenPressed_BuffersInput()
        {
            // Act - press jump while airborne
            _player.TryJumpTest(0.02f, grounded: false);

            // Assert - buffer timer should be set
            Assert.Greater(_player.JumpBufferTimer, 0f);
        }

        [Test]
        public void JumpBuffer_DecaysOverTime()
        {
            // Arrange
            _player.TryJumpTest(0.02f, grounded: false);
            float initialBuffer = _player.JumpBufferTimer;

            // Act - time passes
            _player.TryJumpTest(0.05f, grounded: false);

            // Assert
            Assert.Less(_player.JumpBufferTimer, initialBuffer);
        }

        [Test]
        public void TryJumpTest_WithinCoyoteTime_ExecutesJump()
        {
            // Arrange - grounded
            _player.TryJumpTest(0.02f, grounded: true);
            Assert.IsTrue(_player.IsGrounded);

            // Act - jump immediately
            bool jumped = _player.TryJumpTest(0.02f, grounded: true);

            // Assert
            Assert.IsTrue(jumped);
        }

        [Test]
        public void TryJumpTest_CoyoteTimeExpired_CannotJump()
        {
            // Arrange - start grounded
            _player.TryJumpTest(0.02f, grounded: true);

            // Act - go airborne for too long (beyond coyote time)
            _player.TryJumpTest(0.2f, grounded: false); // 0.2s > 0.1s coyote

            // Assert - should not jump
            bool jumped = _player.TryJumpTest(0.02f, grounded: false);
            Assert.IsFalse(jumped);
        }

        [Test]
        public void TryJumpTest_BufferedJump_ExecutesWhenGrounded()
        {
            // Arrange - airborne, press jump early
            _player.TryJumpTest(0.02f, grounded: false);
            Assert.Greater(_player.JumpBufferTimer, 0f);

            // Act - land while buffer is active
            bool jumped = _player.TryJumpTest(0.02f, grounded: true);

            // Assert
            Assert.IsTrue(jumped);
        }

        [Test]
        public void TryJumpTest_BufferExpired_CannotJump()
        {
            // Arrange - airborne, press jump early
            _player.TryJumpTest(0.02f, grounded: false);

            // Act - time passes beyond buffer window
            _player.TryJumpTest(0.3f, grounded: false); // 0.3s > 0.15s buffer

            // Assert - buffer expired
            Assert.LessOrEqual(_player.JumpBufferTimer, 0f);
        }

        [Test]
        public void IsGrounded_ReflectsParameter()
        {
            // Act & Assert
            _player.TryJumpTest(0.02f, grounded: true);
            Assert.IsTrue(_player.IsGrounded);

            _player.TryJumpTest(0.02f, grounded: false);
            Assert.IsFalse(_player.IsGrounded);
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using MinersWatch;

namespace MinersWatch.Tests.PlayMode
{
    public class PlayerControllerTests
    {
        private GameObject player;
        private PlayerController controller;
        private Rigidbody2D rb;
        private Keyboard keyboard;

        [SetUp]
        public void SetUp()
        {
            // Create test scene objects
            var scene = SceneManager.GetActiveScene();

            // Create ground
            var ground = new GameObject("Ground");
            ground.layer = 6; // Ground layer
            var groundCol = ground.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(20f, 1f);
            ground.transform.position = new Vector3(0f, -2f, 0f);

            // Create player
            player = new GameObject("Player");
            player.tag = "Player";
            controller = player.AddComponent<PlayerController>();
            rb = player.GetComponent<Rigidbody2D>();
            player.transform.position = new Vector3(0f, 0f, 0f);

            // Set up input
            keyboard = InputSystem.AddDevice<Keyboard>();
            if (keyboard == null)
                keyboard = Keyboard.current;
        }

        [TearDown]
        public void TearDown()
        {
            if (player != null)
                Object.Destroy(player);
        }

        [UnityTest]
        public IEnumerator Player_Moves_Right_WhenD_Pressed()
        {
            float initialX = player.transform.position.x;

            // Press D
            Press(keyboard.dKey);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            float newX = player.transform.position.x;
            Assert.Greater(newX, initialX, "Player should move right when D is pressed");

            Release(keyboard.dKey);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Player_Jumps_WhenSpace_Pressed()
        {
            // Wait for grounded state
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            float initialY = player.transform.position.y;

            // Press Space
            Press(keyboard.spaceKey);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            float newY = player.transform.position.y;
            Assert.Greater(newY, initialY, "Player should jump when Space is pressed");

            Release(keyboard.spaceKey);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Player_Clamped_To_CaveBounds()
        {
            // Move player beyond right bound
            player.transform.position = new Vector3(10f, 0f, 0f);
            yield return new WaitForFixedUpdate();

            // LateUpdate should clamp
            yield return null; // Wait for LateUpdate
            yield return new WaitForFixedUpdate();

            Assert.LessOrEqual(player.transform.position.x, 8f, "Player X should not exceed 8");

            // Move player beyond left bound
            player.transform.position = new Vector3(-10f, 0f, 0f);
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.GreaterOrEqual(player.transform.position.x, -8f, "Player X should not go below -8");
        }

        private void Press(KeyControl key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        }

        private void Release(KeyControl key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using MinersWatch;

namespace MinersWatch.Tests.PlayMode
{
    public class PlayerControllerTests
    {
        [UnityTest]
        public IEnumerator Player_Clamped_To_CaveBounds()
        {
            // Create minimal scene
            var ground = new GameObject("Ground");
            ground.layer = 6;
            var gc = ground.AddComponent<BoxCollider2D>();
            gc.size = new Vector2(20f, 1f);
            ground.transform.position = new Vector3(0f, -2f, 0f);

            var player = new GameObject("Player");
            player.tag = "Player";
            var controller = player.AddComponent<PlayerController>();
            player.transform.position = new Vector3(10f, 0f, 0f);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.LessOrEqual(player.transform.position.x, 8f, "Player X should not exceed 8");

            player.transform.position = new Vector3(-10f, 0f, 0f);
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.GreaterOrEqual(player.transform.position.x, -8f, "Player X should not go below -8");

            Object.Destroy(player);
        }
    }
}

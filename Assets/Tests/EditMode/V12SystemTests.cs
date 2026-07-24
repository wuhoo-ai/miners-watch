using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace MinersWatch.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for v1.2 systems: WeaponSystem, DamagePopup, ScreenShake, GameSettings.
    /// </summary>
    public class V12SystemTests
    {
        // ─── GameSettings ───────────────────────────────────────────

        [Test]
        public void GameSettings_DefaultDifficulty_IsNormal()
        {
            PlayerPrefs.DeleteKey("game_difficulty");
            Assert.AreEqual(GameSettings.Difficulty.Normal, GameSettings.Current);
        }

        [Test]
        public void GameSettings_SetDifficulty_Persists()
        {
            GameSettings.Current = GameSettings.Difficulty.Hard;
            Assert.AreEqual(GameSettings.Difficulty.Hard, GameSettings.Current);
            GameSettings.Current = GameSettings.Difficulty.Easy;
            Assert.AreEqual(GameSettings.Difficulty.Easy, GameSettings.Current);
            // cleanup
            GameSettings.Current = GameSettings.Difficulty.Normal;
        }

        [Test]
        public void GameSettings_EnemyCountMultiplier_MatchesDifficulty()
        {
            GameSettings.Current = GameSettings.Difficulty.Easy;
            Assert.AreEqual(0.6f, GameSettings.EnemyCountMultiplier, 0.01f);

            GameSettings.Current = GameSettings.Difficulty.Normal;
            Assert.AreEqual(1.0f, GameSettings.EnemyCountMultiplier, 0.01f);

            GameSettings.Current = GameSettings.Difficulty.Hard;
            Assert.AreEqual(1.5f, GameSettings.EnemyCountMultiplier, 0.01f);

            GameSettings.Current = GameSettings.Difficulty.Normal;
        }

        [Test]
        public void GameSettings_EnemyDamageMultiplier_MatchesDifficulty()
        {
            GameSettings.Current = GameSettings.Difficulty.Easy;
            Assert.AreEqual(0.7f, GameSettings.EnemyDamageMultiplier, 0.01f);

            GameSettings.Current = GameSettings.Difficulty.Normal;
            Assert.AreEqual(1.0f, GameSettings.EnemyDamageMultiplier, 0.01f);

            GameSettings.Current = GameSettings.Difficulty.Hard;
            Assert.AreEqual(1.4f, GameSettings.EnemyDamageMultiplier, 0.01f);

            GameSettings.Current = GameSettings.Difficulty.Normal;
        }

        [Test]
        public void GameSettings_DifficultyLabel_ReturnsChinese()
        {
            Assert.AreEqual("简单", GameSettings.DifficultyLabel(GameSettings.Difficulty.Easy));
            Assert.AreEqual("普通", GameSettings.DifficultyLabel(GameSettings.Difficulty.Normal));
            Assert.AreEqual("困难", GameSettings.DifficultyLabel(GameSettings.Difficulty.Hard));
        }

        // ─── WeaponSystem ───────────────────────────────────────────

        [Test]
        public void WeaponSystem_TryAttack_ReturnsTrue_FirstAttack()
        {
            var go = new GameObject("Player");
            var weapon = go.AddComponent<WeaponSystem>();
            // First attack should succeed (cooldown not active)
            bool result = weapon.TryAttack();
            Assert.IsTrue(result);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void WeaponSystem_TryAttack_CooldownBlocks()
        {
            var go = new GameObject("Player");
            var weapon = go.AddComponent<WeaponSystem>();
            weapon.TryAttack(); // first succeeds
            // Immediate second call — Time.time hasn't advanced in EditMode,
            // so cooldown (0.4s) is still active
            bool second = weapon.TryAttack();
            Assert.IsFalse(second);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void WeaponSystem_ComboCount_IncrementsOnAttack()
        {
            var go = new GameObject("Player");
            var weapon = go.AddComponent<WeaponSystem>();
            Assert.AreEqual(0, weapon.ComboCount);
            weapon.TryAttack();
            Assert.AreEqual(1, weapon.ComboCount);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void WeaponSystem_ComboCount_DoesNotIncrementOnCooldown()
        {
            var go = new GameObject("Player");
            var weapon = go.AddComponent<WeaponSystem>();
            weapon.TryAttack();
            weapon.TryAttack(); // blocked by cooldown
            Assert.AreEqual(1, weapon.ComboCount);
            Object.DestroyImmediate(go);
        }

        // ─── ScreenShake ────────────────────────────────────────────

        [Test]
        public void ScreenShake_Trigger_NoInstance_DoesNotThrow()
        {
            // No ScreenShake in scene — static Trigger should be safe
            Assert.DoesNotThrow(() => ScreenShake.Trigger(0.5f, 0.3f));
        }

        [Test]
        public void ScreenShake_Instance_SetOnAwake()
        {
            var go = new GameObject("Camera");
            var shake = go.AddComponent<ScreenShake>();
            Assert.AreEqual(shake, ScreenShake.Instance);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ScreenShake_Instance_ClearedOnDestroy()
        {
            var go = new GameObject("Camera");
            var shake = go.AddComponent<ScreenShake>();
            Assert.IsNotNull(ScreenShake.Instance);
            Object.DestroyImmediate(go);
            Assert.IsNull(ScreenShake.Instance);
        }

        [Test]
        public void ScreenShake_Shake_DoesNotThrow()
        {
            var go = new GameObject("Camera");
            var shake = go.AddComponent<ScreenShake>();
            // In EditMode, StartCoroutine won't actually run the coroutine,
            // but calling Shake should not throw
            Assert.DoesNotThrow(() => shake.Shake(0.2f, 0.5f));
            Object.DestroyImmediate(go);
        }

        // ─── DamagePopup ────────────────────────────────────────────

        [Test]
        public void DamagePopup_Show_NoInstance_DoesNotThrow()
        {
            // No DamagePopup in scene — static Show should be safe
            Assert.DoesNotThrow(() => DamagePopup.Show(Vector3.zero, "-10", Color.red));
        }

        [Test]
        public void DamagePopup_Instance_SetOnAwake()
        {
            var go = new GameObject("Popup");
            var popup = go.AddComponent<DamagePopup>();
            Assert.AreEqual(popup, DamagePopup.Instance);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DamagePopup_Instance_ClearedOnDestroy()
        {
            var go = new GameObject("Popup");
            var popup = go.AddComponent<DamagePopup>();
            Assert.IsNotNull(DamagePopup.Instance);
            Object.DestroyImmediate(go);
            Assert.IsNull(DamagePopup.Instance);
        }

        [Test]
        public void DamagePopup_Awake_DeactivatesGameObject()
        {
            var go = new GameObject("Popup");
            go.AddComponent<DamagePopup>();
            // Awake sets gameObject inactive (pool pattern)
            Assert.IsFalse(go.activeSelf);
            Object.DestroyImmediate(go);
        }

        // ─── ProceduralSprites (v1.2 enemies) ───────────────────────

        [Test]
        public void ProceduralSprites_AllEnemies_NonNull()
        {
            Assert.IsNotNull(ProceduralSprites.Rockworm);
            Assert.IsNotNull(ProceduralSprites.Shadow);
            Assert.IsNotNull(ProceduralSprites.Lavabeast);
            Assert.IsNotNull(ProceduralSprites.Guardian);
        }

        [Test]
        public void ProceduralSprites_Get_ReturnsCorrectType()
        {
            Assert.AreEqual(ProceduralSprites.Rockworm, ProceduralSprites.Get(EnemyType.Rockworm));
            Assert.AreEqual(ProceduralSprites.Shadow, ProceduralSprites.Get(EnemyType.Shadow));
            Assert.AreEqual(ProceduralSprites.Lavabeast, ProceduralSprites.Get(EnemyType.Lavabeast));
            Assert.AreEqual(ProceduralSprites.Guardian, ProceduralSprites.Get(EnemyType.Guardian));
        }

        [Test]
        public void ProceduralSprites_SpriteSize_Is48px()
        {
            var s = ProceduralSprites.Rockworm;
            Assert.AreEqual(48, s.texture.width);
            Assert.AreEqual(48, s.texture.height);
        }

        [Test]
        public void ProceduralSprites_FilterMode_IsPoint()
        {
            Assert.AreEqual(FilterMode.Point, ProceduralSprites.Rockworm.texture.filterMode);
            Assert.AreEqual(FilterMode.Point, ProceduralSprites.Shadow.texture.filterMode);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using MinersWatch;

namespace Tests.EditMode
{
    [TestFixture]
    public class WeaponSystemTests
    {
        private GameObject _go;
        private WeaponSystem _weapon;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _weapon = _go.AddComponent<WeaponSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        [Test]
        public void TryAttackTest_FirstAttack_StartsCombo()
        {
            // Act
            bool result = _weapon.TryAttackTest(0f);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _weapon.ComboCount);
            Assert.AreEqual(1f, _weapon.ComboDamageMultiplier);
        }

        [Test]
        public void TryAttackTest_AttackWithinComboWindow_IncrementsCombo()
        {
            // Arrange
            _weapon.TryAttackTest(0f);

            // Act - attack at 0.5s (within 0.8s combo window)
            bool result = _weapon.TryAttackTest(0.5f);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, _weapon.ComboCount);
            Assert.Greater(_weapon.ComboDamageMultiplier, 1f);
        }

        [Test]
        public void TryAttackTest_ThreeAttacks_ReachesMaxCombo()
        {
            // Act
            _weapon.TryAttackTest(0f);
            _weapon.TryAttackTest(0.5f);
            bool result = _weapon.TryAttackTest(1.0f);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(3, _weapon.ComboCount);
            Assert.Greater(_weapon.ComboDamageMultiplier, 1.2f);
        }

        [Test]
        public void TryAttackTest_AttackOutsideComboWindow_ResetsCombo()
        {
            // Arrange
            _weapon.TryAttackTest(0f);
            _weapon.TryAttackTest(0.5f);

            // Act - attack at 2.0s (outside 0.8s window from 0.5s)
            bool result = _weapon.TryAttackTest(2.0f);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, _weapon.ComboCount);
            Assert.AreEqual(1f, _weapon.ComboDamageMultiplier);
        }

        [Test]
        public void TryAttackTest_AttackBeforeCooldown_ReturnsFalse()
        {
            // Arrange
            _weapon.TryAttackTest(0f);

            // Act - attack at 0.2s (cooldown is 0.4s)
            bool result = _weapon.TryAttackTest(0.2f);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, _weapon.ComboCount);
        }

        [Test]
        public void UpdateComboTest_TimerDecays_CombosReset()
        {
            // Arrange
            _weapon.TryAttackTest(0f);
            _weapon.TryAttackTest(0.5f);
            Assert.AreEqual(2, _weapon.ComboCount);

            // Act - simulate time passing beyond combo window
            _weapon.UpdateComboTest(1.0f);

            // Assert
            Assert.AreEqual(0, _weapon.ComboCount);
        }

        [Test]
        public void ComboDamageMultiplier_ThreeHits_CalculatesCorrectly()
        {
            // Arrange
            _weapon.TryAttackTest(0f);
            _weapon.TryAttackTest(0.5f);
            _weapon.TryAttackTest(1.0f);

            // Assert - base 1.0 + (3-1) * 0.2 = 1.4
            float expected = 1.4f;
            Assert.AreEqual(expected, _weapon.ComboDamageMultiplier, 0.01f);
        }

        [Test]
        public void TryAttackTest_CannotExceedMaxCombo()
        {
            // Arrange - reach max combo
            _weapon.TryAttackTest(0f);
            _weapon.TryAttackTest(0.5f);
            _weapon.TryAttackTest(1.0f);
            Assert.AreEqual(3, _weapon.ComboCount);

            // Act - attack again within window
            _weapon.TryAttackTest(1.5f);

            // Assert - should still be 3
            Assert.AreEqual(3, _weapon.ComboCount);
        }
    }
}

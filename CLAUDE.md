# 矿工守夜 — Claude Code 开发指南

> 每次委派前加载此文件。本文档积累所有 CI 踩坑经验。

## 黄金法则

1. **所有 .cs 文件放 Assets/Scripts/** — 已有 MinersWatch.Game.asmdef 覆盖
2. **不要创建新 asmdef** — 除非你完全理解 Unity 程序集引用链
3. **每次委派 ≤ 3 个文件** — CI 只认增量，出错面越小定位越快
4. **写了就 push，push 就等 CI** — 不要猜测，看 CI 结果说话

## Unity 6 特定 API 踩坑清单

### MonoBehaviour 生命周期
```csharp
// ❌ EditMode 测试下 Awake/OnEnable 不可靠
// ✅ 提供公开 Init() 方法，测试手动调用
public void Init() { /* 初始化逻辑 */ }
private void Awake() => Init();

// ❌ Time.time 在 EditMode 中始终为 0
// ✅ 冷却计时器初始化为负值
private float lastActionTime = -999f;
```

### [SerializeField] 字段
```csharp
// ❌ 可变状态字段加 [SerializeField] — Unity 二次序列化会在 Awake/Init 后覆盖为 0
// ✅ 只有配置/设计时字段加 [SerializeField]（maxStamina），运行时字段不加（currentStamina）
[SerializeField] private float _maxStamina = 100f;  // 设计师可调
private float _currentStamina = 100f;                // 运行时状态，不持久化
```

### Input System
```csharp
// ❌ UnitySetUp/UnityTearDown — Unity 6 不存在
// ✅ 用 [SetUp]/[TearDown]（NUnit 标准）
// ❌ KeyControl.WriteIntoEvent / KeyControl.ApplyValue — 不存在
// ✅ PlayMode 测试不要模拟输入，太复杂。只测逻辑（如边界 clamp）
```

### Test asmdef
```csharp
// EditMode: 引用 "MinersWatch.Game"，platform=Editor
// PlayMode: 引用 "MinersWatch.Game" + "UnityEngine.TestRunner" + "Unity.InputSystem"
// EditMode 脚本中不要用 yield return / UnityTest — 用 NUnit [Test]
```

### TextMeshPro
```csharp
// TMPro 需要程序集引用 "Unity.TextMeshPro"（已在 MinersWatch.Game.asmdef 中）
using TMPro;  // 不是 UnityEngine.UI.Text
```

### Rigidbody2D API (Unity 6)
```csharp
// ✅ rb.linearVelocity (Unity 6)
// ❌ rb.velocity (已弃用)
```

## 编辑测试的铁律

```csharp
// EditMode 测试模板:
[Test]
public void Test_Xxx() {
    var go = new GameObject("Test");
    var comp = go.AddComponent<YourComponent>();
    comp.Init(); // ← 手动初始化！
    // ... 测试逻辑 ...
    Object.DestroyImmediate(go);
}
```

## CI 约定

- push 后自动触发 quality-gates.yml（EditMode+PlayMode）和 build.yml（Windows+Android）
- 不要用 `restore-keys` 做 Library 缓存（会导致旧 DLL 复用）
- 查看 CI: `gh run list -R wuhoo-ai/miners-watch --limit 2`
- 查看日志: `gh run view <id> -R wuhoo-ai/miners-watch --log-failed | grep "error CS"`

## 已实现系统

| Task | 系统 | 关键文件 |
|------|------|---------|
| T001 | 基础设施 | EditorBuildSettings, MainMenu.unity |
| T002 | 玩家系统 | PlayerController, StaminaSystem, PlayerControls.inputactions |

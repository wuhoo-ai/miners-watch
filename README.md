# 矿工守夜 (Miner's Watch)

> Unity 6 URP | 2D像素+3D混合 | Dave the Diver风格

白天挖矿攒资源，夜晚建防御活下来，层层深入直到击败 Boss。

## 技术栈

- Unity 6000.0.33f1 (LTS)
- Universal Render Pipeline (URP)
- Cinemachine + Input System
- 2D Sprite + 3D Mesh 混合渲染
- GameCI + GitHub Actions 每日构建

## 项目结构

```
Assets/
├── Scenes/          # 场景文件
├── Scripts/         # C# 代码
│   ├── Player/      # 玩家控制器/体力
│   ├── Systems/     # 采矿/商店/存档/日夜循环
│   ├── Data/        # ScriptableObject 数据
│   ├── Entities/    # 敌人/防御工事
│   └── UI/          # UI 控制器
├── Prefabs/         # 预制体
├── Sprites/         # 2D 像素素材
│   ├── Character/   # 主角
│   ├── Enemies/     # 怪物/Boss
│   ├── Environment/ # 场景背景
│   ├── Defenses/    # 防御工事
│   ├── Items/       # 矿物图标
│   └── UI/          # UI 元素
├── Audio/           # 音频
│   ├── SFX/         # 音效
│   └── BGM/         # 背景音乐
├── Models/          # 3D 模型 (Blender)
├── Editor/          # Editor 脚本
└── Resources/       # 动态加载资源
```

## 开发管线

```
GDD → tasks.json → 3路Agent并行(coding/pixel-art/music)
→ review门禁 → GameCI构建 → WeChat通知
```

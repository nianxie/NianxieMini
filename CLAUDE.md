# CLAUDE.md

此文件为 Claude Code (claude.ai/code) 提供在此代码仓库中工作的指导。

## 项目概述

NianxieMini 是一个用于创建互动叙事短游戏的 Unity 插件，支持中文文本。它通过 XLua 提供了与 Unity 集成的 Lua 脚本系统。

**Unity 版本**: 2022.3.62f2c1
**主要语言**: C#，支持 Lua 脚本

## 开发命令

### 无命令行构建
这是一个 Unity 插件项目，没有传统的构建脚本。所有编译都由 Unity 编辑器处理。

## 架构

### 核心结构
```
Runtime/
├── Core/          # 主要框架 (GameManager, LuaBehaviour, UIManager)
├── Craft/         # 可修改游戏参数系统
├── Preview/       # 预览功能
└── Riff/          # 自定义二进制序列化格式

Editor/
├── XLua/          # Lua 代码生成和热修复工具
└── Misc/          # 自定义检查器和创建工具

Plugins/
├── DOTween/       # 动画库
├── TextMesh Pro/  # 高级文本渲染
├── XLua/          # Lua 集成
└── ...           # 其他依赖项
```

### 关键组件

1. **LuaBehaviour**: Lua 脚本游戏对象的基类
2. **GameManager**: 中央游戏状态管理
3. **UIManager**: 带有 Lua 集成的 UI 系统
4. **AssetUsageCenter**: 资源加载和管理
5. **RiffSystem**: 用于游戏存档/配置的自定义二进制数据格式

### 程序集定义
项目使用 Unity 程序集定义实现模块化：
- `NianxieMini.Runtime.asmdef` - 核心运行时程序集
- `NianxieMini.Editor.asmdef` - 编辑器工具
- `NianxieMini.Craft.asmdef` - 制作模式功能
- `NianxieMini.Preview.asmdef` - 预览系统

## Lua 脚本使用

通过 XLua 集成的 Lua 脚本可在以下位置找到：
- `Templates/` 下的模板项目
- 运行时脚本使用 `LuaBehaviour` 组件
- 支持使用 `HOTFIX_ENABLE` 标志的热修复


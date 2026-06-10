# 校园广播接收器插件

一个为 [ClassIsland](https://github.com/ClassIsland/ClassIsland) 设计的校园广播接收器插件，配合PocketBase实现跨校园内网的精准班级通知下发。

## 功能特性

- **实时监听** - 通过SSE连接PocketBase，实时接收广播消息
- **班级过滤** - 只接收指定班级的通知，避免干扰
- **智能防打扰** - 上课时自动静默排队，下课后集中弹出
- **拖堂支持** - 支持最多3次拖堂隐藏，每次20秒
- **强制全屏** - 通知无法通过Alt+F4关闭，必须手动点击关闭按钮
- **TTS语音** - 使用ClassIsland的Edge TTS服务，播放3遍
- **条纹风UI** - 冷峻严肃的全屏遮罩设计
- **设置页面** - 在ClassIsland设置中配置本机名称、服务器地址、班级

## 系统架构

```
Web控制端(H5) → PocketBase(8091) → ClassIsland插件 → 全屏通知+TTS
```

## 安装方法

### 方式一：从插件市场安装（推荐）

1. 在ClassIsland中打开「插件」→「插件市场」
2. 搜索「校园广播接收器」
3. 点击安装

### 方式二：手动安装

1. 下载最新的 `.cipx` 插件包
2. 在ClassIsland中打开「插件」→「安装插件」→ 选择文件
3. 或者手动解压到 `data\Plugins\com.school.broadcast\`

## 配置说明

安装后在ClassIsland设置中找到「校园广播设置」：

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| 本机电脑名称 | 用于标识当前电脑 | 当前电脑名 |
| PocketBase服务器地址 | 广播系统数据中枢地址 | http://127.0.0.1:8091 |
| 目标班级 | 当前电脑所属班级 | 7-1 |

## 使用方法

1. 启动PocketBase服务
2. 启动ClassIsland
3. 在设置中配置班级和服务器地址
4. 通过Web控制端或Python脚本发送广播

## 开发

### 环境要求

- .NET 8 SDK
- ClassIsland 2.0.1+

### 构建

```bash
dotnet build -c Release
```

### 部署到本地ClassIsland

```bash
# 复制DLL到插件目录
copy bin\Debug\net8.0-windows\BroadcastPlugin.dll "D:\ClassIsland_app_windows_x64_selfContained_folder\data\Plugins\com.school.broadcast\"
```

## 项目结构

```
ClassIsland.BroadcastPlugin/
├── Models/
│   ├── BroadcastMessage.cs    # 消息模型
│   └── PluginSettings.cs      # 设置模型
├── Services/
│   └── BroadcastService.cs    # 核心服务（SSE监听）
├── Views/
│   ├── BroadcastWindow.cs     # 全屏弹窗
│   └── PluginSettingsPage.cs  # 设置页面
├── Plugin.cs                  # 插件入口
├── manifest.yml               # 插件清单
└── icon.png                   # 插件图标
```

## 依赖

- ClassIsland.PluginSdk 2.0.0.2
- PocketBase (数据中枢)
- ClassIsland Edge TTS (语音合成)

## 许可证

MIT License

## 致谢

- [ClassIsland](https://github.com/ClassIsland/ClassIsland) - 课表信息显示工具
- [PocketBase](https://pocketbase.io/) - 开源后端服务
- [EntertainingIsland](https://github.com/BSOD-MEMZ/EntertainingIsland) - 插件开发参考

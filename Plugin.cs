using System.IO;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.BroadcastPlugin.Models;
using ClassIsland.BroadcastPlugin.Services;
using ClassIsland.BroadcastPlugin.Views;
using ClassIsland.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClassIsland.BroadcastPlugin;

[PluginEntrance]
public class Plugin : PluginBase
{
    public PluginSettings Settings { get; set; } = new();

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 加载设置
        Settings = ConfigureFileHelper.LoadConfig<PluginSettings>(
            Path.Combine(PluginConfigFolder, "Settings.json"));
        
        // 设置变更时自动保存
        Settings.PropertyChanged += (sender, args) =>
        {
            ConfigureFileHelper.SaveConfig(
                Path.Combine(PluginConfigFolder, "Settings.json"), Settings);
        };

        // 注册设置页面
        services.AddSingleton(Settings);
        services.AddSettingsPage<PluginSettingsPage>();

        // 注册广播接收服务
        services.AddSingleton<BroadcastService>();
        services.AddHostedService(provider => provider.GetRequiredService<BroadcastService>());
    }
}

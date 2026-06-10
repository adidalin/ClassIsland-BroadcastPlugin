using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.BroadcastPlugin.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ClassIsland.BroadcastPlugin.Views;

[SettingsPageInfo(
    "com.school.broadcast.settings",
    "校园广播设置",
    "\uE713",
    "\uE713",
    SettingsPageCategory.External
)]
public class PluginSettingsPage : SettingsPageBase
{
    private readonly PluginSettings _settings;

    public PluginSettingsPage(PluginSettings settings)
    {
        _settings = settings;

        // 创建主滚动视图
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };

        // 创建主容器
        var mainStack = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 16
        };

        // === 基本设置部分 ===
        AddSectionHeader(mainStack, "\uE713", "基本设置");

        // 本机电脑名称
        AddSettingItem(mainStack,
            "\uE77B",
            "本机电脑名称",
            "用于标识当前电脑，方便在多台设备时区分",
            CreateTextBox(_settings.ComputerName, "例如：教室电脑-7-2", 300,
                (text) => _settings.ComputerName = text));

        // PocketBase服务器地址
        AddSettingItem(mainStack,
            "\uE774",
            "PocketBase服务器地址",
            "广播系统的数据中枢地址，需要包含端口号",
            CreateTextBox(_settings.PocketBaseUrl, "例如：http://192.168.1.100:8091", 400,
                (text) => _settings.PocketBaseUrl = text));

        // 目标班级
        AddSettingItem(mainStack,
            "\uE716",
            "目标班级",
            "当前电脑所属班级，需要与发送端一致",
            CreateTextBox(_settings.TargetClass, "例如：7-2", 200,
                (text) => _settings.TargetClass = text));

        // === 说明部分 ===
        AddSectionHeader(mainStack, "\uE946", "使用说明");

        var helpText = new TextBlock
        {
            Text = "1. 确保PocketBase服务已启动\n" +
                   "2. 本机电脑名称建议使用「学校名-年级-班级」格式\n" +
                   "3. 服务器地址需要包含http://前缀和端口号\n" +
                   "4. 班级名称必须与Web控制端选择的班级一致\n" +
                   "5. 修改设置后自动保存，无需手动操作",
            Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
            TextWrapping = TextWrapping.Wrap,
            LineSpacing = 8,
            Margin = new Thickness(8, 0, 0, 0)
        };
        mainStack.Children.Add(helpText);

        // === 插件信息 ===
        AddSectionHeader(mainStack, "\uE946", "插件信息");

        var infoText = new TextBlock
        {
            Text = "名称: 校园广播接收器\n" +
                   "版本: 1.0.0\n" +
                   "功能: 接收PocketBase广播消息，全屏通知+TTS语音播报\n" +
                   "支持: 上课静默、拖堂隐藏、强制全屏",
            Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
            TextWrapping = TextWrapping.Wrap,
            LineSpacing = 8,
            Margin = new Thickness(8, 0, 0, 0)
        };
        mainStack.Children.Add(infoText);

        scrollViewer.Content = mainStack;
        Content = scrollViewer;
    }

    /// <summary>
    /// 添加分组标题
    /// </summary>
    private void AddSectionHeader(StackPanel parent, string icon, string text)
    {
        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 8, 0, 4)
        };

        header.Children.Add(new TextBlock
        {
            Text = icon,
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        });

        header.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });

        parent.Children.Add(header);
    }

    /// <summary>
    /// 添加设置项
    /// </summary>
    private void AddSettingItem(StackPanel parent, string icon, string title, string description, Control control)
    {
        // 创建设置项容器
        var itemBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 2, 0, 2)
        };

        var itemGrid = new Grid();
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        // 图标
        var iconText = new TextBlock
        {
            Text = icon,
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 20,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 212)),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 12, 0)
        };
        Grid.SetColumn(iconText, 0);

        // 文字区域
        var textStack = new StackPanel
        {
            Spacing = 4
        };

        textStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold
        });

        textStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
            TextWrapping = TextWrapping.Wrap
        });
        Grid.SetColumn(textStack, 1);

        // 控件
        control.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(control, 2);

        itemGrid.Children.Add(iconText);
        itemGrid.Children.Add(textStack);
        itemGrid.Children.Add(control);

        itemBorder.Child = itemGrid;
        parent.Children.Add(itemBorder);
    }

    /// <summary>
    /// 创建文本输入框
    /// </summary>
    private TextBox CreateTextBox(string initialValue, string placeholder, int width, Action<string> onChanged)
    {
        var textBox = new TextBox
        {
            Text = initialValue,
            Watermark = placeholder,
            Width = width,
            HorizontalContentAlignment = HorizontalAlignment.Left
        };

        textBox.TextChanged += (s, e) => onChanged(textBox.Text ?? "");
        return textBox;
    }
}

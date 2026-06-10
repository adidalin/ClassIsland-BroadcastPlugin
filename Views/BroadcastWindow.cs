using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace ClassIsland.BroadcastPlugin.Views;

public class BroadcastWindow : Window
{
    private readonly string _content;
    private readonly TextBlock _contentTextBlock;
    private readonly TextBlock _infoTextBlock;
    private readonly Button _delayButton;
    private readonly Button _closeButton;

    // 配置
    private const int DelaySeconds = 20;
    private const int MaxDelayCount = 3;
    private const int DisplaySeconds = 10;

    private int _delayCount = 0;
    private bool _delayClicked = false;
    private bool _allowClose = false;

    public BroadcastWindow(string content)
    {
        _content = content;

        // 窗口设置 - 强制全屏
        Title = "校园广播";
        WindowState = WindowState.Maximized;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Topmost = true;
        ShowInTaskbar = false;
        Background = CreateStripedBackground();
        CanResize = false;

        // 禁用Alt+F4和其他关闭方式
        Closing += (s, e) =>
        {
            if (!_allowClose)
            {
                e.Cancel = true;
            }
        };

        // 创建UI
        var mainGrid = new Grid
        {
            Margin = new Thickness(40)
        };
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // 标题
        var titleText = new TextBlock
        {
            Text = "校园广播通知",
            Foreground = Brushes.White,
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 40, 0, 20)
        };
        Grid.SetRow(titleText, 0);

        // 通知内容
        _contentTextBlock = new TextBlock
        {
            Text = content,
            Foreground = Brushes.White,
            FontSize = 72,
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(_contentTextBlock, 1);

        // 状态信息
        _infoTextBlock = new TextBlock
        {
            Text = "",
            Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
            FontSize = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 10)
        };
        Grid.SetRow(_infoTextBlock, 2);

        // 按钮容器
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 20
        };
        Grid.SetRow(buttonPanel, 3);

        // 拖堂按钮（立即可点击）
        _delayButton = new Button
        {
            Content = "拖堂（隐藏20秒）",
            FontSize = 24,
            Padding = new Thickness(30, 15, 30, 15),
            Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
            BorderThickness = new Thickness(1)
        };
        _delayButton.Click += (s, e) => { _delayClicked = true; };

        // 关闭按钮（初始禁用，灰色）
        _closeButton = new Button
        {
            Content = "关闭通知",
            FontSize = 24,
            Padding = new Thickness(30, 15, 30, 15),
            Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
            BorderThickness = new Thickness(1),
            IsEnabled = false
        };
        _closeButton.Click += (s, e) => { CloseWindow(); };

        buttonPanel.Children.Add(_delayButton);
        buttonPanel.Children.Add(_closeButton);

        mainGrid.Children.Add(titleText);
        mainGrid.Children.Add(_contentTextBlock);
        mainGrid.Children.Add(_infoTextBlock);
        mainGrid.Children.Add(buttonPanel);

        Content = mainGrid;
    }

    private void CloseWindow()
    {
        _allowClose = true;
        Close();
    }

    /// <summary>
    /// 启用关闭按钮
    /// </summary>
    private void EnableCloseButton()
    {
        _closeButton.IsEnabled = true;
        _closeButton.Background = new SolidColorBrush(Color.FromRgb(180, 30, 30));
        _closeButton.Foreground = Brushes.White;
    }

    /// <summary>
    /// 创建条纹背景
    /// </summary>
    private IBrush CreateStripedBackground()
    {
        var visualBrush = new VisualBrush
        {
            TileMode = TileMode.Tile,
            DestinationRect = new RelativeRect(0, 0, 20, 20, RelativeUnit.Absolute)
        };

        var canvas = new Canvas
        {
            Width = 20,
            Height = 20
        };

        var background = new Avalonia.Controls.Shapes.Rectangle
        {
            Width = 20,
            Height = 20,
            Fill = new SolidColorBrush(Color.FromArgb(224, 0, 0, 0))
        };
        canvas.Children.Add(background);

        var line = new Avalonia.Controls.Shapes.Line
        {
            StartPoint = new Point(0, 20),
            EndPoint = new Point(20, 0),
            Stroke = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255)),
            StrokeThickness = 0.5
        };
        canvas.Children.Add(line);

        visualBrush.Visual = canvas;
        return visualBrush;
    }

    /// <summary>
    /// 等待拖堂点击或超时
    /// </summary>
    private async Task<bool> WaitForDelayOrTimeoutAsync(int timeoutSeconds)
    {
        _delayClicked = false;
        var startTime = DateTime.Now;
        while (!_delayClicked && (DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
        {
            await Task.Delay(100);
        }
        return _delayClicked;
    }

    /// <summary>
    /// 显示窗口（不处理TTS，TTS由BroadcastService处理）
    /// </summary>
    public async Task ShowAndSpeakAsync()
    {
        Show();

        // 初始状态：显示拖堂按钮，关闭按钮禁用
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _delayButton.IsVisible = true;
            _delayButton.Content = $"拖堂（隐藏{DelaySeconds}秒）";
            _closeButton.IsEnabled = false;
            _closeButton.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            _closeButton.Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150));
            _infoTextBlock.Text = $"可拖堂 {MaxDelayCount} 次 | 关闭按钮 {DisplaySeconds} 秒后可用";
        });

        // 拖堂循环
        for (int i = 0; i < MaxDelayCount; i++)
        {
            var clicked = await WaitForDelayOrTimeoutAsync(DisplaySeconds);

            if (clicked)
            {
                // 点击了拖堂 → 隐藏窗口
                _delayCount++;
                await Dispatcher.UIThread.InvokeAsync(() => { Hide(); });

                // 等待DelaySeconds
                await Task.Delay(DelaySeconds * 1000);

                // 重新显示
                await Dispatcher.UIThread.InvokeAsync(() => { Show(); });

                // 更新提示
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _delayButton.Content = $"拖堂（隐藏{DelaySeconds}秒）";
                    _infoTextBlock.Text = $"已拖堂 {_delayCount} 次 | 可拖堂 {MaxDelayCount - _delayCount} 次";
                });
            }
            else
            {
                // 超时没点击 → 跳出拖堂循环
                break;
            }
        }

        // 拖堂结束，启用关闭按钮
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _delayButton.IsVisible = false;
            EnableCloseButton();
            _infoTextBlock.Text = "请点击红色按钮关闭通知";
        });
    }
}

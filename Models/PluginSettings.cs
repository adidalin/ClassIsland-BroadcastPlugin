using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClassIsland.BroadcastPlugin.Models;

/// <summary>
/// 插件设置模型
/// </summary>
public class PluginSettings : INotifyPropertyChanged
{
    private string _computerName = Environment.MachineName;
    private string _pocketBaseUrl = "http://127.0.0.1:8091";
    private string _targetClass = "7-1";
    private int _displaySeconds = 10;

    /// <summary>
    /// 本机电脑名称
    /// </summary>
    public string ComputerName
    {
        get => _computerName;
        set
        {
            if (value == _computerName) return;
            _computerName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// PocketBase服务器地址
    /// </summary>
    public string PocketBaseUrl
    {
        get => _pocketBaseUrl;
        set
        {
            if (value == _pocketBaseUrl) return;
            _pocketBaseUrl = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 目标班级
    /// </summary>
    public string TargetClass
    {
        get => _targetClass;
        set
        {
            if (value == _targetClass) return;
            _targetClass = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 通知显示时长（秒）
    /// </summary>
    public int DisplaySeconds
    {
        get => _displaySeconds;
        set
        {
            if (value == _displaySeconds) return;
            _displaySeconds = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

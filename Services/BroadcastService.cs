using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Abstractions.Services.SpeechService;
using ClassIsland.BroadcastPlugin.Models;
using ClassIsland.BroadcastPlugin.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ClassIsland.BroadcastPlugin.Services;

public class BroadcastService : BackgroundService, IDisposable
{
    private readonly ILessonsService _lessonsService;
    private readonly PluginSettings _settings;
    private readonly ILogger<BroadcastService> _logger;
    private readonly ISpeechService _speechService;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentQueue<BroadcastMessage> _messageQueue = new();
    
    private CancellationTokenSource? _cts;
    private string? _clientId;

    public BroadcastService(ILessonsService lessonsService, PluginSettings settings, ILogger<BroadcastService> logger, ISpeechService speechService)
    {
        _lessonsService = lessonsService;
        _settings = settings;
        _logger = logger;
        _speechService = speechService;
        _httpClient = new HttpClient();
        
        _lessonsService.OnBreakingTime += OnBreakingTime;
        _lessonsService.OnClass += OnClass;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _logger.LogInformation("[广播服务] 启动中...");
        _logger.LogInformation("[广播服务] PocketBase地址: {Url}", _settings.PocketBaseUrl);
        _logger.LogInformation("[广播服务] 目标班级: {Class}", _settings.TargetClass);
        
        await StartListening(_cts.Token);
    }

    private async Task StartListening(CancellationToken stoppingToken)
    {
        try
        {
            var sseUrl = $"{_settings.PocketBaseUrl}/api/realtime";
            _logger.LogInformation("[广播服务] 正在连接: {Url}", sseUrl);
            
            var request = new HttpRequestMessage(HttpMethod.Get, sseUrl);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
            
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, stoppingToken);
            response.EnsureSuccessStatusCode();
            
            using var stream = await response.Content.ReadAsStreamAsync(stoppingToken);
            using var reader = new System.IO.StreamReader(stream);
            
            _logger.LogInformation("[广播服务] SSE连接已建立，等待消息...");
            
            string? eventType = null;
            string? data = null;
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                
                if (line == null)
                {
                    _logger.LogWarning("[广播服务] 连接断开，5秒后重连...");
                    await Task.Delay(5000, stoppingToken);
                    await StartListening(stoppingToken);
                    return;
                }
                
                if (line.StartsWith("event:"))
                {
                    eventType = line.Substring(6).Trim();
                }
                else if (line.StartsWith("data:"))
                {
                    data = line.Substring(5).Trim();
                }
                else if (line == "" && eventType != null && data != null)
                {
                    await HandleSseEvent(eventType, data);
                    eventType = null;
                    data = null;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[广播服务] 连接错误");
            if (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("[广播服务] 5秒后重连...");
                await Task.Delay(5000, stoppingToken);
                await StartListening(stoppingToken);
            }
        }
    }

    private async Task HandleSseEvent(string eventType, string data)
    {
        try
        {
            _logger.LogInformation("[广播服务] 收到事件: {Event}", eventType);
            _logger.LogInformation("[广播服务] 事件数据: {Data}", data);
            
            // 处理连接事件，获取clientId并订阅
            if (eventType == "PB_CONNECT")
            {
                var jsonDoc = JsonDocument.Parse(data);
                if (jsonDoc.RootElement.TryGetProperty("clientId", out var clientIdElement))
                {
                    _clientId = clientIdElement.GetString();
                    _logger.LogInformation("[广播服务] 获取到客户端ID: {ClientId}", _clientId);
                    await SubscribeToCollection();
                }
                return;
            }
            
            // 处理记录变更事件（事件类型可能是broadcast_messages/*或PB_UPDATE/PB_CREATE）
            if (eventType == "PB_UPDATE" || eventType == "PB_CREATE" || eventType.StartsWith("broadcast_messages"))
            {
                var jsonDoc = JsonDocument.Parse(data);
                var root = jsonDoc.RootElement;
                
                if (!root.TryGetProperty("action", out var action))
                    return;
                
                var actionStr = action.GetString();
                if (actionStr != "create" && actionStr != "update")
                    return;
                
                if (!root.TryGetProperty("record", out var record))
                    return;
                
                var message = new BroadcastMessage
                {
                    Id = record.GetProperty("id").GetString() ?? "",
                    TargetClass = record.GetProperty("target_class").GetString() ?? "",
                    Content = record.GetProperty("content").GetString() ?? "",
                    AlertType = record.GetProperty("alert_type").GetString() ?? "fullscreen"
                };
                
                _logger.LogInformation("[广播服务] 收到消息: 班级={Class}, 类型={Type}, 内容={Content}", 
                    message.TargetClass, message.AlertType, message.Content);
                
                if (message.TargetClass != _settings.TargetClass)
                {
                    _logger.LogInformation("[广播服务] 忽略非目标班级消息: {Class}", message.TargetClass);
                    return;
                }
                
                await ProcessMessage(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[广播服务] 处理事件错误");
        }
    }

    private async Task SubscribeToCollection()
    {
        if (string.IsNullOrEmpty(_clientId))
        {
            _logger.LogError("[广播服务] 客户端ID为空，无法订阅");
            return;
        }

        try
        {
            var subscribeUrl = $"{_settings.PocketBaseUrl}/api/realtime";
            var body = new
            {
                clientId = _clientId,
                subscriptions = new[] { "broadcast_messages/*" }
            };
            
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(subscribeUrl, content);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("[广播服务] 已订阅broadcast_messages集合");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[广播服务] 订阅失败");
        }
    }

    private async Task ProcessMessage(BroadcastMessage message)
    {
        if (IsInClass())
        {
            _messageQueue.Enqueue(message);
            _logger.LogInformation("[广播服务] 上课中，消息已入队，队列长度: {Count}", _messageQueue.Count);
        }
        else
        {
            _logger.LogInformation("[广播服务] 下课状态，立即显示消息");
            await ShowBroadcast(message);
        }
    }

    private bool IsInClass()
    {
        return _lessonsService.CurrentState == 0;
    }

    private void OnBreakingTime(object? sender, EventArgs e)
    {
        _logger.LogInformation("[广播服务] 检测到下课，处理队列消息...");
        ProcessQueue();
    }

    private void OnClass(object? sender, EventArgs e)
    {
        _logger.LogInformation("[广播服务] 检测到上课，进入静默模式");
    }

    private void ProcessQueue()
    {
        while (_messageQueue.TryDequeue(out var message))
        {
            _logger.LogInformation("[广播服务] 从队列取出消息: {Content}", message.Content);
            ShowBroadcast(message).Wait();
        }
    }

    private async Task ShowBroadcast(BroadcastMessage message)
    {
        _logger.LogInformation("[广播服务] 显示广播: {Content}", message.Content);
        
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                // 使用ClassIsland的TTS服务朗读3遍（带间隔）
                for (int i = 0; i < 3; i++)
                {
                    _speechService.EnqueueSpeechQueue(message.Content);
                    _logger.LogInformation("[广播服务] TTS第 {Count} 遍", i + 1);
                    if (i < 2)
                    {
                        await Task.Delay(2000); // 每遍间隔2秒
                    }
                }
                
                // 显示弹窗
                var window = new BroadcastWindow(message.Content);
                await window.ShowAndSpeakAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[广播服务] 显示窗口错误");
        }
    }

    public override void Dispose()
    {
        _cts?.Cancel();
        _lessonsService.OnBreakingTime -= OnBreakingTime;
        _lessonsService.OnClass -= OnClass;
        _httpClient?.Dispose();
        base.Dispose();
    }
}

using System;
using TMPro;
using UnityEngine;
using Zenject;

public class LLMInputHandler: IInitializable, IDisposable
{
    [Inject(Id = "InputField")]
    private readonly TMP_InputField _inputField;
    [Inject(Id = "OutputField")]
    private readonly TMP_Text _outputField;

    private EventHandler _eventHandler;

    private ILLMService _geminiSevice;
    private ILLMService _groqSevice;
    private ILLMService _openRouterService;
    
    [Inject]
    public void Construct(EventHandler eventHandler)
    {
        _eventHandler = eventHandler;
        _eventHandler.OnSendRequest += SendRequest;
    }

    public void Initialize()
    {
        _geminiSevice = new GeminiService();
        _groqSevice = new GroqService();
        _openRouterService = new OpenRouterService();
    }

    private async void SendRequest()
    {
        string request = _inputField.text;

        _eventHandler.SaveMessageInDB("Player", "System", request);

        _inputField.text = "";
        // string response = await _geminiSevice.GetResponseAsync(request);
        // string response = await _groqSevice.GetResponseAsync(request);
        string response = await _openRouterService.GetResponseAsync(request);

        // 4. Выводим результат
        if (!string.IsNullOrEmpty(response))
        {
            // Debug.Log($"[{_geminiSevice.ModelName}] ответил: {response}");
            // Debug.Log($"[{_groqSevice.ModelName}] ответил: {response}");
            Debug.Log($"[{_openRouterService.ModelName}] ответил: {response}");


            _outputField.text += request + '\n' + response + '\n';

            _eventHandler.SaveMessageInDB(_openRouterService.ModelName, "System", response);
        }
        else
        {
            Debug.LogError("Не удалось получить ответ от нейросети.");
        }
    }

    public void Dispose()
    {
        _eventHandler.OnSendRequest -= SendRequest;
    }
}

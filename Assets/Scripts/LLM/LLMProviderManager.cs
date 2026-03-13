using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LLMProviderManager : MonoBehaviour
{
    private Dictionary<string, ILLMService> _activeModels = new();

    void Awake()
    {
        // Регистрируем нужные модели
        RegisterModel("OpenAI", new ChatGPTService());
        RegisterModel("Google", new GeminiService());
    }

    public void RegisterModel(string id, ILLMService service) => _activeModels[id] = service;

    public async Task<string> RequestFromModel(string id, string prompt)
    {
        if (_activeModels.TryGetValue(id, out var service))
        {
            return await service.GetResponseAsync(prompt);
        }
        return null;
    }
}


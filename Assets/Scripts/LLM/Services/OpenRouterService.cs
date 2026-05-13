using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;

public class OpenRouterService : ILLMService
{
    // public string ModelName => "nvidia/nemotron-3-super-120b-a12b:free";
    public string ModelName => "openrouter/owl-alpha";
    private string _apiKey;
    // URL включает название модели и API ключ как параметр
    private string _baseUrl = "https://openrouter.ai/api/v1/chat/completions";

    public OpenRouterService()
    {
        _ = LoadApiKey();
    }

    private async Task LoadApiKey()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "openrouter_key.txt");
        if (File.Exists(filePath))
        {
            _apiKey = (await File.ReadAllTextAsync(filePath)).Trim();
        }
        else
        {
            Debug.LogError("OpenRouter API Key не найден в StreamingAssets/openrouter_key.txt");
        }
    }

    public async Task<string> GetResponseAsync(List<Message> prompt)
    {
        if (string.IsNullOrEmpty(_apiKey)) await LoadApiKey();

        // Формируем запрос
        var requestData = new OpenRouterRequest
        {
            messages = prompt,
            model = ModelName
        };

        string jsonPayload = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(_baseUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Debug.LogWarning(request.downloadHandler.text);
                var response = JsonUtility.FromJson<GroqResponse>(request.downloadHandler.text);
                if (response.choices != null && response.choices.Count > 0)
                {
                    return response.choices[0].message.content;
                }
                return "OpenRouter вернул пустой ответ.";
            }

            Debug.LogError($"OpenRouter Error: {request.error}\n{request.downloadHandler.text}");
            return null;
        }
    }
}



[Serializable]
public class OpenRouterRequest
{
    public string model;
    public List<Message> messages;
}

// Классы для десериализации ответа
[Serializable]
public class OpenRouterResponse { public List<Choice> choices; }



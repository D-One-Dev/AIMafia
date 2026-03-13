using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;

public class GeminiService : ILLMService
{
    public string ModelName => "gemini-3-flash-preview";
    private string _apiKey;
    // URL включает название модели и API ключ как параметр
    private string _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

    public GeminiService()
    {
        _ = LoadApiKey();
    }

    private async Task LoadApiKey()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "gemini_key.txt");
        if (File.Exists(filePath))
        {
            _apiKey = (await File.ReadAllTextAsync(filePath)).Trim();
        }
        else
        {
            Debug.LogError("Gemini API Key не найден в StreamingAssets/gemini_key.txt");
        }
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey)) await LoadApiKey();

        // Формируем запрос
        var requestData = new GeminiRequest
        {
            contents = new List<Content>
            {
                new Content {
                    role = "user",
                    parts = new List<Part> { new Part { text = prompt } }
                }
            },
            generationConfig = new GenerationConfig()
        };

        string jsonPayload = JsonUtility.ToJson(requestData);
        string fullUrl = $"{_baseUrl}{ModelName}:generateContent?key={_apiKey}";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                if (response.candidates != null && response.candidates.Length > 0)
                {
                    return response.candidates[0].content.parts[0].text;
                }
                return "Gemini вернул пустой ответ.";
            }

            Debug.LogError($"Gemini Error: {request.error}\n{request.downloadHandler.text}");
            return null;
        }
    }
}



[Serializable]
public class GeminiRequest
{
    public List<Content> contents;
    public GenerationConfig generationConfig;
}

[Serializable]
public class Content
{
    public string role; // "user" или "model"
    public List<Part> parts;
}

[Serializable]
public class Part
{
    public string text;
}

[Serializable]
public class GenerationConfig
{
    public float temperature = 0.7f;
    public int maxOutputTokens = 1000;
}

// Классы для десериализации ответа
[Serializable]
public class GeminiResponse { public Candidate[] candidates; }
[Serializable]
public class Candidate { public Content content; }
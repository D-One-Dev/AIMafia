using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;

public class GroqService : ILLMService
{
    public string ModelName => "llama-3.3-70b-versatile";
    private string _apiKey;
    // URL включает название модели и API ключ как параметр
    private string _baseUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GroqService()
    {
        _ = LoadApiKey();
    }

    private async Task LoadApiKey()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "groq_key.txt");
        if (File.Exists(filePath))
        {
            _apiKey = (await File.ReadAllTextAsync(filePath)).Trim();
        }
        else
        {
            Debug.LogError("Groq API Key не найден в StreamingAssets/groq_key.txt");
        }
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey)) await LoadApiKey();

        // Формируем запрос
        var requestData = new GroqRequest
        {
            messages = new List<Message>
            {
                new Message
                {
                    role = "user",
                    content = prompt
                }
            },
            model = ModelName
        };

        string jsonPayload = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(_baseUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(request.downloadHandler.text);
                var response = JsonUtility.FromJson<GroqResponse>(request.downloadHandler.text);
                if (response.choices != null && response.choices.Count > 0)
                {
                    return response.choices[0].message.content;
                }
                return "Groq вернул пустой ответ.";
            }

            Debug.LogError($"Groq Error: {request.error}\n{request.downloadHandler.text}");
            return null;
        }
    }
}



[Serializable]
public class GroqRequest
{
    public List<Message> messages;
    public string model;
}

// Классы для десериализации ответа
[Serializable]
public class GroqResponse { public List<Choice> choices; }



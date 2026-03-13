using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;

public class ChatGPTService : ILLMService
{
    public string ModelName => "gpt-4o-mini";
    private string _apiKey;
    private string _url = "https://api.openai.com/v1/chat/completions";

    public ChatGPTService()
    {
        // Ключ загрузится при создании экземпляра сервиса
        _ = LoadApiKey();
    }

    private async Task LoadApiKey()
    {
        // Формируем путь к файлу
        string filePath = Path.Combine(Application.streamingAssetsPath, "openai_key.txt");

        if (Application.platform == RuntimePlatform.Android)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(filePath))
            {
                await request.SendWebRequest();
                _apiKey = request.downloadHandler.text.Trim();
            }
        }
        else
        {
            // Для ПК/Редактора можно использовать обычный File.ReadAllText
            if (File.Exists(filePath))
            {
                _apiKey = File.ReadAllText(filePath).Trim();
            }
            else
            {
                Debug.LogError($"Ключ не найден по пути: {filePath}");
            }
        }
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        // Проверка: загрузился ли ключ
        if (string.IsNullOrEmpty(_apiKey))
        {
            await LoadApiKey();
            if (string.IsNullOrEmpty(_apiKey)) return "Ошибка: API Key не загружен.";
        }

        var requestData = new OpenAIRequest
        {
            model = ModelName,
            messages = new System.Collections.Generic.List<Message> {
                new Message { role = "system", content = "Ты участник игры Мафия." },
                new Message { role = "user", content = prompt }
            }
        };

        string jsonPayload = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(_url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + _apiKey);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                return response.choices[0].message.content;
            }

            return $"Error: {request.error}";
        }
    }
}

[Serializable]
public class OpenAIRequest
{
    public string model;
    public List<Message> messages;
    public float temperature = 0.7f;
}

[Serializable]
public class Message
{
    public string role; // "system", "user" или "assistant"
    public string content;
}

// Классы для получения ответа
[Serializable]
public class OpenAIResponse { public Choice[] choices; }
[Serializable]
public class Choice { public Message message; }
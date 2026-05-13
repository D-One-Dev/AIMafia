// using UnityEngine;
// using UnityEngine.Networking;
// using System.Threading.Tasks;
// using System.Text;
// using System.IO;
// using System;

// public class ClaudeService : ILLMService
// {
//     public string ModelName => "claude-opus-4-6";
//     private string _apiKey;

//     private string _baseUrl = "https://api.anthropic.com/v1/messages";

//     public ClaudeService()
//     {
//         _ = LoadApiKey();
//     }

//     private async Task LoadApiKey()
//     {
//         string filePath = Path.Combine(Application.streamingAssetsPath, "claude_key.txt");
//         if (File.Exists(filePath))
//         {
//             _apiKey = (await File.ReadAllTextAsync(filePath)).Trim();
//         }
//         else
//         {
//             Debug.LogError("Claude API Key не найден в StreamingAssets/claude_key.txt");
//         }
//     }

//     public async Task<string> GetResponseAsync(string prompt)
//     {
//         if (string.IsNullOrEmpty(_apiKey)) await LoadApiKey();

//         var requestData = new ClaudeRequest
//         {
//             model = ModelName,
//             max_tokens = 1000,
//             messages = new ClaudeRequestMessage[]
//             {
//                 new ClaudeRequestMessage{role = "user", content = prompt}
//             }
//         };

//         string jsonPayload = JsonUtility.ToJson(requestData);

//         using (UnityWebRequest request = new UnityWebRequest(_baseUrl, "POST"))
//         {
//             byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("x-api-key", _apiKey);
//             request.SetRequestHeader("anthropic-version", "2023-06-01");

//             await request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 string jsonResponse = request.downloadHandler.text;
//                 Debug.LogWarning(jsonResponse);
//                 ClaudeResponse responseData = JsonUtility.FromJson<ClaudeResponse>(jsonResponse);
//                 if(responseData.content != null)
//                 {
//                     foreach (var item in responseData.content)
//                     {
//                         if(item.type == "text")
//                         {
//                             return item.text;
//                         }
//                     }
//                 }
//                 Debug.LogError("Error: empty response");
//                 return null;
//             }

//             Debug.LogError($"Claude Error: {request.error}\n{request.downloadHandler.text}");
//             return null;
//         }
//     }
// }



// [Serializable]
// public class ClaudeRequest
// {
//     public string model;
//     public int max_tokens;
//     public ClaudeRequestMessage[] messages;
// }
// [Serializable]
// public class ClaudeRequestMessage { public string role; public string content;}
// [Serializable]
// public class ClaudeResponse { public ClaudeOutputItem[] content; }
// [Serializable]
// public class ClaudeOutputItem { public string type; public string text; }
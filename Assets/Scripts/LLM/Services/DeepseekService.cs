// using UnityEngine;
// using UnityEngine.Networking;
// using System.Threading.Tasks;
// using System.Text;
// using System.IO;
// using System;

// public class DeepseekService : ILLMService
// {
//     public string ModelName => "deepseek-chat";
//     private string _apiKey;

//     private string _baseUrl = "https://api.deepseek.com/chat/completions";

//     public DeepseekService()
//     {
//         _ = LoadApiKey();
//     }

//     private async Task LoadApiKey()
//     {
//         string filePath = Path.Combine(Application.streamingAssetsPath, "deepseek_key.txt");
//         if (File.Exists(filePath))
//         {
//             _apiKey = (await File.ReadAllTextAsync(filePath)).Trim();
//         }
//         else
//         {
//             Debug.LogError("Deepseek API Key не найден в StreamingAssets/deepseek_key.txt");
//         }
//     }

//     public async Task<string> GetResponseAsync(string prompt)
//     {
//         if (string.IsNullOrEmpty(_apiKey)) await LoadApiKey();

//         var requestData = new DeepseekRequest
//         {
//             model = ModelName,
//             messages = new DeepseekRequestMessage[]
//             {
//                 new DeepseekRequestMessage{role = "user", content = prompt}
//             },
//             stream = false
//         };

//         string jsonPayload = JsonUtility.ToJson(requestData);

//         using (UnityWebRequest request = new UnityWebRequest(_baseUrl, "POST"))
//         {
//             byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

//             await request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 string jsonResponse = request.downloadHandler.text;
//                 Debug.LogWarning(jsonResponse);
//                 DeepseekResponse responseData = JsonUtility.FromJson<DeepseekResponse>(jsonResponse);
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

//             Debug.LogError($"Deepseek Error: {request.error}\n{request.downloadHandler.text}");
//             return null;
//         }
//     }
// }



// [Serializable]
// public class DeepseekRequest
// {
//     public string model;
//     public DeepseekRequestMessage[] messages;
//     public bool stream;
// }
// [Serializable]
// public class DeepseekRequestMessage { public string role; public string content;}
// [Serializable]
// public class DeepseekResponse { public DeepseekOutputItem[] content; }
// [Serializable]
// public class DeepseekOutputItem { public string type; public string text; }
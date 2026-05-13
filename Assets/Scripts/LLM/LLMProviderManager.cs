// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Zenject;

// public class LLMProviderManager: IInitializable, IDisposable
// {
//     private EventHandler _eventHandler;
//     private Dictionary<string, ILLMService> _activeModels;

//     [Inject]
//     public void Construct(EventHandler eventHandler)
//     {
//         _eventHandler = eventHandler;
//         _eventHandler.OnRegisterModel += RegisterModel;

//         _activeModels = new();
//     }

//     public void Initialize()
//     {
//         // Регистрируем нужные модели
//         RegisterModel("OpenAI", new ChatGPTService());
//         RegisterModel("Google", new GeminiService());
//     }

//     private void RegisterModel(string id, ILLMService service) => _activeModels[id] = service;

//     public async Task<string> RequestFromModel(string id, string prompt)
//     {
//         if (_activeModels.TryGetValue(id, out var service))
//         {
//             return await service.GetResponseAsync(prompt);
//         }
//         return null;
//     }

//     public void Dispose()
//     {
//         _eventHandler.OnRegisterModel -= RegisterModel;
//     }
// }


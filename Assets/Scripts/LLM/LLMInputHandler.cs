using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public enum LLMProvider
{
    Groq = 0,
    OpenRouter = 1
}

public class LLMInputHandler
{
    // [Inject(Id = "InputField")]
    // private readonly TMP_Text _inputField;
    // [Inject(Id = "OutputField")]
    // private readonly TMP_Text _outputField;

    private EventHandler _eventHandler;
    private DatabaseManager _databaseManager;

    [Inject]
    public void Construct(EventHandler eventHandler, DatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
        _eventHandler = eventHandler;
    }

    public async Task<string> SendRequest(ILLMService llmService, string playerName, string request, string basePrompt, bool hidden)
    {
        // _eventHandler.SaveMessageInDB("System", role, playerName, request);

        // _inputField.text = "";

        List<Message> prompt = new List<Message>();
        prompt.Add(new Message
        {
            role = "assistant",
            content = basePrompt
        });

        List<GameMessage> messages = _databaseManager.ReadFromDB($"SELECT * FROM ChatHistory WHERE Target IN ('{playerName}', 'System') AND Hidden = false");

        foreach (GameMessage message in messages)
        {
            prompt.Add(new Message
            {
                role = "assistant",
                content = $"{message.Author}: {message.Message}"
            });
        }

        prompt.Add(new Message
        {
            role = "user",
            content = request
        });

        foreach (Message gameMessage in prompt) Debug.Log($"{gameMessage.role}: {gameMessage.content}");

        string response = await llmService.GetResponseAsync(prompt);

        if (!string.IsNullOrEmpty(response))
        {
            Debug.Log($"[{llmService.ModelName}] ответил: {response}");

            // _eventHandler.SayPhrase(response);
            // _outputField.text += request + '\n' + response + '\n';

            _eventHandler.SaveMessageInDB(playerName, "System", response, hidden);
            return response;
        }
        else
        {
            Debug.LogError("Не удалось получить ответ от нейросети.");
            return null;
        }
    }

    public ILLMService GetLLM(LLMProvider llmProvider)
    {
        return llmProvider switch
        {
            LLMProvider.Groq => new GroqService(),
            LLMProvider.OpenRouter => new OpenRouterService(),
            _ => new OpenRouterService(),
        };
    }
}

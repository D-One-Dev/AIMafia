using System;
using UnityEngine;

public class EventHandler : MonoBehaviour
{
    public event Action<string, string, string, bool> OnSaveMessageInDB;
    public event Action<string, ILLMService> OnRegisterModel;
    public event Action OnSendRequestToLLM;
    public event Action<String> OnSayPhrase;
    public event Action<AudioClip> OnRecognizePhrase;
    public event Action OnStartRecordingPhrase;
    public event Action OnEndRecordingPhrase;
    public event Action OnSendPhrase;

    public void SaveMessageInDB(string author, string target, string message, bool hidden)
    {
        OnSaveMessageInDB?.Invoke(author, target, message, hidden);
    }

    public void RegisterModel(string id, ILLMService service)
    {
        OnRegisterModel?.Invoke(id, service);
    }

    public void SendRequestToLLM()
    {
        OnSendRequestToLLM?.Invoke();
    }

    public void SayPhrase(string phrase)
    {
        OnSayPhrase?.Invoke(phrase);
    }

    public void RecognizePhrase(AudioClip phrase)
    {
        OnRecognizePhrase?.Invoke(phrase);
    }
    public void StartRecordingPhrase()
    {
        OnStartRecordingPhrase?.Invoke();
    }
    public void EndRecordingPhrase()
    {
        OnEndRecordingPhrase?.Invoke();
    }

    public void SendPhrase()
    {
        OnSendPhrase?.Invoke();
    }
}
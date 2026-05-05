using System;
using UnityEngine;

public class EventHandler: MonoBehaviour
{
    public event Action<string, string, string> OnSaveMessageInDB;
    public event Action<string, ILLMService> OnRegisterModel;
    public event Action OnSendRequest;

    public void SaveMessageInDB(string author, string target, string message)
    {
        OnSaveMessageInDB?.Invoke(author, target, message);
    }

    public void RegisterModel(string id, ILLMService service)
    {
        OnRegisterModel?.Invoke(id, service);       
    }

    public void SendRequest()
    {
        OnSendRequest?.Invoke();
    }
}
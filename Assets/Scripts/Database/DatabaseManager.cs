using System;
using System.IO;
using SQLite;
using UnityEngine;
using Zenject;

[Table("ChatHistory")]
public class LLMMessage
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Author { get; set; }
    public string Target { get; set; }
    public string Message {get; set; }
}

public class DatabaseManager : IInitializable, IDisposable
{
    private EventHandler _eventHandler;    
    private SQLiteConnection db;

    [Inject]
    public void Construct(EventHandler eventHandler)
    {
        _eventHandler = eventHandler;
        _eventHandler.OnSaveMessageInDB += SaveMessageInDB;
    }

    public void Initialize()
    {
        InitializeDB();
    }

    private void InitializeDB()
    {
        Debug.Log(Application.persistentDataPath);

        if(File.Exists($"{Application.persistentDataPath}/ChatHistory.db"))
        {
            Debug.Log("Database already exists, dropping old DB and creating new");
            File.Delete($"{Application.persistentDataPath}/ChatHistory.db");
        }

        db = new SQLiteConnection($"{Application.persistentDataPath}/ChatHistory.db");

        db.CreateTable<LLMMessage>();
    }

    private void SaveMessageInDB(string author, string target, string message)
    {
        LLMMessage llmMessage = new LLMMessage
        {
            Author = author,
            Target = target,
            Message = message
        };

        db.Insert(llmMessage);
    }

    public void Dispose()
    {
        _eventHandler.OnSaveMessageInDB -= SaveMessageInDB;
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILLMService
{
    string ModelName { get; }
    // Метод для отправки запроса
    Task<string> GetResponseAsync(List<Message> prompt);
}
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LLMInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text outputField;
    private ILLMService _geminiSevice;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _geminiSevice = new GeminiService();
    }

    // Update is called once per frame
    public async void SendRequest()
    {
        string request = inputField.text;
        inputField.text = "";
        string response = await _geminiSevice.GetResponseAsync(request);

        // 4. Выводим результат
        if (!string.IsNullOrEmpty(response))
        {
            Debug.Log($"[{_geminiSevice.ModelName}] ответил: {response}");

            outputField.text += request + '\n' + response + '\n';
        }
        else
        {
            Debug.LogError("Не удалось получить ответ от нейросети.");
        }
    }
}

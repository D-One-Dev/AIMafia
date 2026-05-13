using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PonyuDev.SherpaOnnx.Tts;
using PonyuDev.SherpaOnnx.Tts.Engine;
using TMPro;
using UnityEngine;
using Zenject;

public enum PlayerType
{
    Human = 0,
    AI = 1
}

public enum PlayerRole
{
    Мафия = 0,
    Шериф = 1,
    Доктор = 2,
    Мирный = 3,
    NotSet = 100
}

public class Player
{
    public string Name;
    public PlayerType Type;
    public PlayerRole Role;
    public ILLMService LLMService;
    public string BasePrompt;
    public bool IsDead;
}

public enum GameState
{
    Night = 0,
    Discussion = 1,
    Vote = 2
}

public class GameMaster : IInitializable, IDisposable
{
    private readonly string _defaultPrompt = @"Ты — игрок в настольную игру «Мафия».
Основные правила: - Всегда отвечай ТОЛЬКО на русском языке.
- Не используй эмодзи, markdown, спецсимволы и декоративное оформление.
- Твое имя: SELF_NAME
- Имена игроков: Михаил, Кристина, Дмитрий, Егор, Агамир
- Твоя роль: ROLE
- Всего игроков: 5
- Роли в игре: 1 мафия, 1 шериф, 1 доктор, 2 мирных жителя.
- Никогда не раскрывай свою роль напрямую, даже если тебя обвиняют.
- Учитывай только живых игроков.
- Нельзя голосовать или применять ночное действие на себя или мертвых игроков.
- Если игрок выбыл, его роль становится известна всем.

Фазы игры:
1. Ночь:
- Мафия выбирает игрока для убийства.
- Шериф проверяет роль одного игрока.
- Доктор лечит одного игрока.
- Все ночные действия выполняются скрытно.

2. День:
- Система сообщает, кто умер ночью.
- Игроки обсуждают, кто может быть мафией.
- Можно обвинять, оправдываться, анализировать поведение и голоса других игроков.
- Не выдавай скрытую информацию, которой не можешь знать.

3. Голосование:
- Каждый игрок выбирает одного игрока на выбывание.
- Игрок с большинством голосов выбывает и раскрывает роль.

Поведение по ролям:
- Мафия:
  - Цель: устранить всех мирных.
  - В обсуждении пытайся выглядеть мирным, подозревать других и избегать подозрений.
  - Ночью выбирай наиболее опасного игрока.

- Шериф:
  - Цель: найти мафию и помочь мирным.
  - Ночью проверяй подозрительных игроков.
  - Не раскрывай себя без необходимости.

- Доктор:
  - Цель: сохранить мирных и помочь найти мафию.
  - Ночью лечи наиболее вероятную цель мафии или важного игрока.

- Мирный житель:
  - Анализируй поведение, обвинения и голосования.
  - Пытайся вычислить мафию логически.

Формат ответов:
- Во время обсуждения отвечай обычным текстом.
- Во время голосования отвечай ТОЛЬКО одним словом - именем выбранного игрока без дополнительного текста:

- Во время ночной фазы отвечай ТОЛЬКО одним словом - именем выбранного игрока без дополнительного текста:

Правила речи:
- Всегда говори только от первого лица.
- Никогда не начинай ответ с имени игрока, двоеточия или обращения.
- Не используй формат:
  ИМЯ:
  ИМЯ -
  (ИМЯ)
- Не имитируй сообщения других игроков.
- Не пиши реплики за других игроков.
- Не вставляй свое имя в начало сообщения.
- Не оформляй ответ как диалог или стенограмму.
- Во время обсуждения ответ должен содержать только твою собственную реплику без префиксов и подписей.
- Имена игроков можно упоминать только внутри предложения.

Запрещено:
- Нарушать формат ответа в голосовании и ночью.
- Добавлять пояснения к ответу в голосовании и ночью.
- Придумывать несуществующие правила или роли.
- Управлять действиями других игроков.
- Сообщать скрытую информацию без игровых оснований.

Если входные данные противоречат правилам — следуй правилам этого промпта.";

    [Inject(Id = "InputField")]
    private readonly TMP_Text _inputField;

    private List<Player> _players;

    private LLMInputHandler _llmInputHandler;
    private EventHandler _eventHandler;

    private GameState _currentState;
    private TTSManager _ttsManager;
    private DatabaseManager _databaseManager;
    private bool _canPlayerSubmit;

    [Inject]
    public void Construct(LLMInputHandler llmInputHandler, EventHandler eventHandler, TTSManager ttsManager, DatabaseManager databaseManager)
    {
        _llmInputHandler = llmInputHandler;
        _ttsManager = ttsManager;
        _databaseManager = databaseManager;

        _eventHandler = eventHandler;
        _eventHandler.OnSendPhrase += SendPhrase;
    }

    public async void Initialize()
    {
        SetPlayers();
        SetRoles();
        SetPrompts();

        _currentState = GameState.Night;
        await GameRound();
    }

    private void SetPlayers()
    {
        _players = new List<Player>
        {
            new Player
            {
                Name = "Михаил",
                Type = PlayerType.Human,
                Role = PlayerRole.Мирный,
                LLMService = null,
                IsDead = false
            },
            new Player
            {
                Name = "Кристина",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.OpenRouter),
                IsDead = false
            },
            new Player
            {
                Name = "Дмитрий",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.Groq),
                IsDead = false
            },
            new Player
            {
                Name = "Егор",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.OpenRouter),
                IsDead = false
            },
            new Player
            {
                Name = "Агамир",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.Groq),
                IsDead = false
            }
        };
    }

    private void SetRoles()
    {
        List<PlayerRole> roles = new List<PlayerRole>
        {
            PlayerRole.Мафия,
            PlayerRole.Шериф,
            PlayerRole.Доктор,
            PlayerRole.Мирный
        };

        for (int i = 1; i < _players.Count; i++)
        {
            int choise = UnityEngine.Random.Range(0, roles.Count);
            _players[i].Role = roles[choise];
            roles.RemoveAt(choise);
        }

        foreach (Player player in _players) Debug.Log($"Player: {player.Name}, role: {player.Role}");
    }

    private void SetPrompts()
    {
        foreach (Player player in _players)
        {
            string prompt = _defaultPrompt;
            prompt = prompt.Replace("SELF_NAME", player.Name);
            switch (player.Role)
            {
                case PlayerRole.Мафия:
                    prompt = prompt.Replace("ROLE", "Мафия");
                    break;
                case PlayerRole.Шериф:
                    prompt = prompt.Replace("ROLE", "Шериф");
                    break;
                case PlayerRole.Доктор:
                    prompt = prompt.Replace("ROLE", "Доктор");
                    break;
                case PlayerRole.Мирный:
                    prompt = prompt.Replace("ROLE", "Мирный житель");
                    break;
            }
            player.BasePrompt = prompt;
        }
    }

    private async Task GameRound()
    {
        _canPlayerSubmit = false;
        await PerformNightActions();
        _currentState = GameState.Discussion;
        _databaseManager.SaveMessageInDB("System", "System", "Начинается фаза обсуждения", false);
        _canPlayerSubmit = true;
    }

    private async Task PerformNightActions()
    {
        List<Player> nightActivePlayers = _players.Where(p => p.Role != PlayerRole.Мирный).OrderBy(p => p.Role).ToList();
        Player killedPlayer = null;
        string log = null;

        foreach (Player player in nightActivePlayers)
        {
            if (player.Type == PlayerType.Human) continue;
            string answer = await _llmInputHandler.SendRequest(player.LLMService, player.Name, "Ночная фаза", player.BasePrompt, true);
            switch (player.Role)
            {
                case PlayerRole.Мафия:
                    killedPlayer = _players.First(x => x.Name == answer);
                    killedPlayer.IsDead = true;

                    log = $"Мафия убила игрока {answer} ({killedPlayer.Role})";
                    break;
                case PlayerRole.Доктор:
                    if (answer == killedPlayer.Name)
                    {
                        killedPlayer.IsDead = false;
                        log = "Благодаря работе Доктора Мафия не смогла никого убить этой ночью";
                    }
                    break;
                case PlayerRole.Шериф:
                    _databaseManager.SaveMessageInDB("System", player.Name, $"Роль игрока {answer}: {_players.First(x => x.Name == answer).Role}");
                    break;
            }

        }
        if (log != null)
        {
            _databaseManager.SaveMessageInDB("System", "System", log, false);
            ShowLog(log);
        }
    }

    private void SendPhrase()
    {
        if (_canPlayerSubmit)
        {
            string phrase = _inputField.text;
            _inputField.text = "";
            _databaseManager.SaveMessageInDB("Михаил", "System", phrase, false);
        }

        TryNext();
    }

    private void TryNext()
    {
        if (_currentState == GameState.Discussion)
        {
            PerformDiscussion();
        }
    }

    private async Task PerformDiscussion()
    {
        List<Player> aliveAIs = _players
            .Where(p => !p.IsDead && p.Type == PlayerType.AI)
            .ToList();

        if (aliveAIs.Count == 0)
            return;

        // Запускаем первый запрос
        Task<string> currentRequestTask = SendPlayerRequest(aliveAIs[0]);

        // Текущая озвучка
        Task currentSpeechTask = Task.CompletedTask;

        for (int i = 0; i < aliveAIs.Count; i++)
        {
            // Ждём ответ текущего AI
            string response = await currentRequestTask;

            response = Regex.Replace(response, @"^[^:]+:\s*", "");

            // Пока идёт озвучка —
            // запускаем следующий запрос
            Task<string> nextRequestTask = null;

            if (i + 1 < aliveAIs.Count)
            {
                nextRequestTask = SendPlayerRequest(aliveAIs[i + 1]);
            }

            // Ждём завершения предыдущей озвучки
            await currentSpeechTask;

            // Запускаем новую озвучку
            currentSpeechTask = SpeakResponse(response);

            // Переходим к следующему запросу
            currentRequestTask = nextRequestTask;
        }

        // Дожидаемся последней озвучки
        await currentSpeechTask;
    }

    private Task<string> SendPlayerRequest(Player player)
    {
        return _llmInputHandler.SendRequest(
            player.LLMService,
            player.Name,
            "Обсуждение",
            player.BasePrompt,
            false
        );
    }

    private async Task SpeakResponse(string response)
    {
        TtsResult result = await _ttsManager.SayPhrase(response);

        await Task.Delay(TimeSpan.FromSeconds(result.DurationSeconds));
    }

    private void ShowLog(string log)
    {
        _inputField.text = log;
    }

    public void Dispose()
    {
        _eventHandler.OnSendPhrase -= SendPhrase;
    }
}

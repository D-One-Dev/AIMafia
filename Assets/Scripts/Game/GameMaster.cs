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
    public int ID;
    public PlayerType Type;
    public PlayerRole Role;
    public ILLMService LLMService;
    public string BasePrompt;
    public bool IsDead;
    public int TTSVoiceID;
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

    [Inject(Id = "BotOutputFields")]
    private readonly TMP_Text[] _botOutputFields;

    private List<Player> _players;

    private LLMInputHandler _llmInputHandler;
    private EventHandler _eventHandler;

    private GameState _currentState;
    private TTSManager _ttsManager;
    private DatabaseManager _databaseManager;
    private bool _canPlayerSubmit;
    private bool _canPlayerVote;

    [Inject]
    public void Construct(LLMInputHandler llmInputHandler, EventHandler eventHandler, TTSManager ttsManager, DatabaseManager databaseManager)
    {
        _llmInputHandler = llmInputHandler;
        _ttsManager = ttsManager;
        _databaseManager = databaseManager;

        _eventHandler = eventHandler;
        _eventHandler.OnSendPhrase += SendPhrase;
        _eventHandler.OnPlayerVote += PlayerVote;
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
                ID = 0,
                Type = PlayerType.Human,
                Role = PlayerRole.Мирный,
                LLMService = null,
                IsDead = false
            },
            new Player
            {
                Name = "Кристина",
                ID = 1,
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.OpenRouter),
                IsDead = false,
                TTSVoiceID = 2,
            },
            new Player
            {
                Name = "Дмитрий",
                ID = 2,
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.Groq),
                IsDead = false,
                TTSVoiceID = 0,
            },
            new Player
            {
                Name = "Егор",
                ID = 3,
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.OpenRouter),
                IsDead = false,
                TTSVoiceID = 1,
            },
            new Player
            {
                Name = "Агамир",
                ID = 4,
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.Groq),
                IsDead = false,
                TTSVoiceID = 3,
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
        if (_players.Count(p => !p.IsDead && p.Type == PlayerType.Human) > 0) _canPlayerSubmit = true;
        else await PerformDiscussion();
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
            _canPlayerSubmit = false;
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

            response = RemoveLeadingName(response);

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
            currentSpeechTask = SpeakResponse(response, aliveAIs[i]);

            // Переходим к следующему запросу
            currentRequestTask = nextRequestTask;
        }

        // Дожидаемся последней озвучки
        await currentSpeechTask;

        _databaseManager.SaveMessageInDB("System", "System", "Начинается фаза голосования", false);
        ShowLog("Начинается фаза голосования");
        _currentState = GameState.Vote;
        if (_players.Count(p => !p.IsDead && p.Type == PlayerType.Human) <= 0) await PerformVote("");
        else _canPlayerVote = true;
    }

    private async void PlayerVote(string vote)
    {
        if (!_canPlayerVote) return;
        if (_players.Count(p => !p.IsDead && p.Type == PlayerType.Human) <= 0) return;
        if (_players.First(p => p.Name == vote).IsDead) return;

        _canPlayerVote = false;
        await PerformVote(vote);
    }

    private async Task PerformVote(string playerVote)
    {
        List<Player> aliveAIs = _players
            .Where(p => !p.IsDead && p.Type == PlayerType.AI)
            .ToList();

        Dictionary<string, int> votes = new Dictionary<string, int>
        {
            { "Михаил", 0 },
            { "Кристина", 0 },
            { "Дмитрий", 0 },
            { "Егор", 0 },
            { "Агамир", 0 }
        };

        if (votes.ContainsKey(playerVote) && !_players.First(p => p.Name == playerVote).IsDead) votes[playerVote] += 1;

        foreach (Player player in aliveAIs)
        {
            if (player.Type == PlayerType.Human) continue;
            string answer = await _llmInputHandler.SendRequest(player.LLMService, player.Name, "Голосование", player.BasePrompt, true);

            if (votes.ContainsKey(answer) && !_players.First(p => p.Name == answer).IsDead) votes[answer] += 1;
        }

        KeyValuePair<string, int> mostVotedPlayer = votes.OrderByDescending(v => v.Value).First();
        Player killedPlayer = _players.First(p => p.Name == mostVotedPlayer.Key);
        killedPlayer.IsDead = true;

        string log = "Голосование:\n";
        foreach (KeyValuePair<string, int> pair in votes)
        {
            log += $"{pair.Key}: {pair.Value}\n";
        }
        string logEnd = "";
        switch (killedPlayer.Role)
        {
            case PlayerRole.Мафия:
                logEnd = $"{mostVotedPlayer.Key} выбывает, его роль - Мафия";
                break;
            case PlayerRole.Шериф:
                logEnd = $"{mostVotedPlayer.Key} выбывает, его роль - Шериф";
                break;
            case PlayerRole.Доктор:
                logEnd = $"{mostVotedPlayer.Key} выбывает, его роль - Доктор";
                break;
            case PlayerRole.Мирный:
                logEnd = $"{mostVotedPlayer.Key} выбывает, его роль - Мирный житель";
                break;
        }
        log += logEnd;

        _databaseManager.SaveMessageInDB("System", "System", logEnd, false);
        ShowLog(log);
    }

    public string RemoveLeadingName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        int colonIndex = input.IndexOf(':');

        // Двоеточия нет или оно в начале строки
        if (colonIndex <= 0)
            return input;

        // Слишком длинная "шапка" — скорее всего не имя
        if (colonIndex > 10)
            return input;

        string beforeColon = input[..colonIndex].Trim();

        // Пусто после Trim
        if (beforeColon.Length == 0)
            return input;

        // Если до двоеточия есть знаки препинания — не удаляем
        // Разрешаем дефис и пробелы, т.к. имена могут быть двойными
        bool hasInvalidPunctuation = beforeColon.Any(c =>
            char.IsPunctuation(c) &&
            c != '-' &&
            c != '—');

        if (hasInvalidPunctuation)
            return input;

        // Удаляем имя и пробелы после двоеточия
        return input[(colonIndex + 1)..].TrimStart();
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

    private async Task SpeakResponse(string response, Player player)
    {
        TtsResult result = await _ttsManager.SayPhrase(response, player.TTSVoiceID);
        _botOutputFields[player.ID].text = response;
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

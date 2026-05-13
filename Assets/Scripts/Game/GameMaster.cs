using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum PlayerType
{
    Human = 0,
    AI = 1
}

public enum PlayerRole
{
    Mafia = 0,
    Sherif = 1,
    Doctor = 2,
    Citizen = 3,
    NotSet = 100
}

public class Player
{
    public string Name;
    public PlayerType Type;
    public PlayerRole Role;
    public ILLMService LLMService;
}

public class GameMaster : IInitializable
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
- Во время голосования отвечай ТОЛЬКО JSON без дополнительного текста:
{""choise\"":""ИМЯ""}

- Во время ночной фазы отвечай ТОЛЬКО JSON без дополнительного текста:
{""choise"":""ИМЯ""}

Запрещено:
- Нарушать JSON-формат в голосовании и ночью.
- Добавлять пояснения к JSON.
- Придумывать несуществующие правила или роли.
- Управлять действиями других игроков.
- Сообщать скрытую информацию без игровых оснований.

Если входные данные противоречат правилам — следуй правилам этого промпта.";

    private List<Player> _players;

    private LLMInputHandler _llmInputHandler;

    [Inject]
    public void Construct(LLMInputHandler llmInputHandler)
    {
        _llmInputHandler = llmInputHandler;
    }

    public void Initialize()
    {
        SetPlayers();
        SetRoles();
    }

    private void SetPlayers()
    {
        _players = new List<Player>
        {
            new Player
            {
                Name = "Михаил",
                Type = PlayerType.Human,
                Role = PlayerRole.NotSet,
                LLMService = null
            },
            new Player
            {
                Name = "Кристина",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.OpenRouter)
            },
            new Player
            {
                Name = "Дмитрий",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.Groq)
            },
            new Player
            {
                Name = "Егор",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.OpenRouter)
            },
            new Player
            {
                Name = "Агамир",
                Type = PlayerType.AI,
                Role = PlayerRole.NotSet,
                LLMService = _llmInputHandler.GetLLM(LLMProvider.Groq)
            }
        };
    }

    private void SetRoles()
    {
        List<PlayerRole> roles = new List<PlayerRole>
        {
            PlayerRole.Mafia,
            PlayerRole.Sherif,
            PlayerRole.Doctor,
            PlayerRole.Citizen,
            PlayerRole.Citizen
        };

        for(int i = 0; i < _players.Count; i++)
        {
            int choise = Random.Range(0, roles.Count);
            _players[i].Role = roles[choise];
            roles.RemoveAt(choise);
        }

        foreach(Player player in _players) Debug.Log($"Player: {player.Name}, role: {player.Role}");
    }
}

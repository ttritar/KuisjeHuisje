using SFB;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class FileLogger : ISingleton<FileLogger>
{

    public enum LogCategory { Info, NPC, World, Emotion, Player, Error }


    // File
    private bool _firstTimeCreation = false;
    [SerializeField] private bool _appendToExisting = true;
    [SerializeField] private string _outputFileName = "OutputLog";
    [SerializeField] private string _defaultLogDirectory = "C:/CartoonKapoen";

    private string _filePath;
    private string FilePath
    {
        get
        {
            if (_filePath == null)
            {
                _filePath = Path.Combine(LogDirectory, _outputFileName + $"{DateTime.Now:dd_MM_yyyy_HHmmss}.txt");
                WriteStartMessage();
            }
            return _filePath;
        }
    }
    private static readonly object _fileLock = new();
    public string LogDirectory { get; private set; } = null;


    // HELPER
    //--------------------------------------------------
    public void PickLogDirectory()
    {
        var result = StandaloneFileBrowser.OpenFolderPanel("Select Log Directory", "", false);
        if (result.Length > 0 && !string.IsNullOrEmpty(result[0]))
        {
            string path = result[0];
            PlayerPrefs.SetString("LogDirectory", path);
            PlayerPrefs.Save();

            SetLogDirectory(path);
        }
    }
    public void SetLogDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if(_firstTimeCreation)
            WriteEndMessage();
        LogDirectory = directory;
        _filePath = null;
        _firstTimeCreation = true;
    }
    private void WriteEndMessage()
    {
        lock (_fileLock)
        {
            if (FilePath == "")
                return;
            using StreamWriter writer = new(FilePath, true, Encoding.UTF8);
            writer.WriteLine();
            writer.WriteLine("---------- Einde Sessie ----------");
            writer.WriteLine();
        }
    }
    private void WriteStartMessage()
    {
        lock (_fileLock)
        {
            if (FilePath == "")
                return;
            using StreamWriter writer = new(FilePath, _appendToExisting, Encoding.UTF8);
            writer.WriteLine("----------");
            writer.WriteLine($"| Game Session: {DateTime.Now:G} |");
            writer.WriteLine("----------");
        }
    }
    private string LogCategoryPrefix(LogCategory type)
    {
        string prefix = type switch
        {
            LogCategory.Info => "[Info]",
            LogCategory.NPC => "[Karakter]",
            LogCategory.World => "[Wereld]",
            LogCategory.Emotion => "[Emotie]",
            LogCategory.Player => "[Speler]",
            LogCategory.Error => "[ERROR]",
            _ => "[]"
        };
        return prefix;
    }

    // START / END
    //--------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        LogDirectory = _defaultLogDirectory + "/Logs";
        if (PlayerPrefs.HasKey("LogDirectory"))
        {
            string path = PlayerPrefs.GetString("LogDirectory");
            LogDirectory = path;
        }
        SetLogDirectory(LogDirectory);
    }
    private void OnApplicationQuit()
    {
        WriteEndMessage();
    }

    // FUNCTIONALITY
    //--------------------------------------------------
    public void LogCustom(string message, LogCategory type)
    {
        string prefix = LogCategoryPrefix(type);
        string line = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {prefix} {message}";

        lock (_fileLock)
        {
            if (FilePath == "")
                return;
            using StreamWriter writer = new(FilePath, true, Encoding.UTF8);
            writer.WriteLine(line);
        }
    }
    private string TranslateEmotionAdjective(Emotion e)
    {
        return e switch
        {
            Emotion.Happy => "blije",
            Emotion.Angry => "boze",
            Emotion.Sad => "verdrietige",
            Emotion.Scared => "bange",
            _ => e.ToString()
        };
    }
    private string TranslateEmotionNoun(Emotion e)
    {
        return e switch
        {
            Emotion.Happy => "blij",
            Emotion.Angry => "boos",
            Emotion.Sad => "verdrietig",
            Emotion.Scared => "bang",
            _ => e.ToString()
        };
    }
    private string TranslateAnimal(Animal a)
    {
        return a switch
        {
            Animal.Cat => "kat",
            Animal.Dog => "hond",
            Animal.Frog => "kikker",
            Animal.Sheep => "schaap",
            Animal.Bunny => "konijn",
            Animal.Bear => "beer",
            Animal.Lion => "leeuw",
            _ => a.ToString()
        };
    }
    private string TranslateItem(ItemBehaviour.Type i)
    {
        return i switch
        {
            ItemBehaviour.Type.DeadFish => "vissen geraamte",
            ItemBehaviour.Type.Flower => "bloem",
            ItemBehaviour.Type.WiltedFlower => "verwelkte bloem",
            _ => i.ToString()
        };
    }
    private string TranslateHair(Hair h)
    {
        return h switch
        {
            Hair.Bald => "kaal",
            Hair.Short => "kort",
            Hair.Ponytail => "paardenstaart",
            Hair.WizardHat => "tovernaarshoed",
            _ => h.ToString()
        };
    }
    private string TranslateHairColor(HairColor h)
    {
        return h switch
        {
            HairColor.Black => "zwart",
            HairColor.Brown => "bruin",
            HairColor.Blonde => "blond",
            HairColor.Red => "rood",
            _ => h.ToString()
        };
    }
    private string TranslateTop(Top t)
    {
        return t switch
        {
            Top.TShirt => "korte mouwen",
            Top.Longsleeve => "lange mouwen",
            Top.WizardCloak => "tovernaars jas",
            _ => t.ToString()
        };
    }
    private string TranslateBottom(Bottom b)
    {
        return b switch
        {
            Bottom.Shorts => "korte broek",
            Bottom.Pants => "lange broek",
            Bottom.SkirtRuffles => "rokje",
            Bottom.SkirtStraight => "rokje",
            _ => b.ToString()
        };
    }

    // Logging
    //--------------------------------------------------
    private string FormatCharacter(CharacterData data)
    {
        return $"{data.Relationship} ({TranslateAnimal(data.Animal)}, {TranslateEmotionNoun(data.Emotion)})";
    }
    private string FormatStyle(PlayerData data)
    {
        return $"een {TranslateHair(data.Hair)} kapsel, {TranslateTop(data.Top)}, en een {TranslateBottom(data.Bottom)}. De speler voelt zich {TranslateEmotionNoun(data.Emotion)}.";
    }

    public void LogPlayerCustomization(PlayerData data)
    {
        LogCustom(
            $"De speler heeft hun uiterlijk aangepast: {FormatStyle(data)}",
            LogCategory.Player
        );
    }

    public void LogItemGifting(ItemBehaviour item, CharacterData data)
    {
        LogCustom($"De speler geeft een {TranslateItem(item.ItemType)} aan {FormatCharacter(data)}.", LogCategory.NPC);
    }
    public void LogItemPickup(ItemBehaviour item)
    {
        LogCustom($"De speler heeft een {TranslateItem(item.ItemType)} opgeraapt.", LogCategory.Player);
    }
    public void LogEmotion(CharacterData data, Emotion emotion)
    {
        LogCustom($"De speler voelt zich {TranslateEmotionNoun(emotion)} naar {FormatCharacter(data)}.", LogCategory.Emotion);
    }
    public void LogNPCCreation(CharacterData data)
    {
        LogCustom($"De speler maakte {data.Relationship} als een {TranslateEmotionAdjective(data.Emotion)} {TranslateAnimal(data.Animal)}.", LogCategory.NPC);
    }

    public void LogNPCUnassignment(CharacterData data)
    {
        LogCustom($"De speler heeft {FormatCharacter(data)} uit een huis gezet.", LogCategory.NPC);
    }
    public void LogNPCAssignment(CharacterData data, HouseBehaviour house)
    {
        string msg = $"De speler heeft {FormatCharacter(data)} een huis gegeven.";

        var assignedCount = house.AssignedCharacters.Count;
        if (assignedCount > 0)
        {
            int idx = 0;
            foreach (var c in house.AssignedCharacters)
            {
                msg += $" {FormatCharacter(c)}";
                ++idx;
                if (idx < assignedCount)
                    msg += ",";
            }

            if (assignedCount > 1) msg += " wonen hier ook al.";
            else msg += " woont hier ook al.";
        }

        LogCustom(msg, LogCategory.NPC);
    }
    public void LogNPCWorldSwitch(Emotion oldWorld, Emotion newWorld, CharacterData data)
    {
        LogCustom($"Stuurt {FormatCharacter(data)} van de {TranslateEmotionAdjective(oldWorld)} wereld naar de {TranslateEmotionAdjective(newWorld)} wereld.", LogCategory.NPC);
    }
    public void LogWorldSwitch(Emotion oldWorld, Emotion newWorld)
    {
        LogCustom($"De speler gaat naar van de {TranslateEmotionAdjective(oldWorld)} wereld naar de {TranslateEmotionAdjective(newWorld)} wereld.", LogCategory.Player);
    }
    public void LogHouseCleaned(Emotion world)
    {
        LogCustom($"De speler heeft een huis schoon gemaakt in de {TranslateEmotionAdjective(world)} wereld!", LogCategory.World);
    }
}

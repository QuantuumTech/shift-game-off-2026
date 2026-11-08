using System.Collections;
using UnityEngine;

/// <summary>
/// Game flow: Title -&gt; Level Select (unlocks kept in memory only — no save
/// system, per the jam scope) -&gt; Levels 1-8 -&gt; Win screen.
/// R restarts the current level, Esc returns to the title.
/// Spikes respawn the player at the level start and reset the state to A.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPlaying { get; private set; }

    public TextAsset[] levelFiles;

    public readonly string[] levelNames =
    {
        "First Flip", "Now You See It", "Leap of Faith", "Crossfire",
        "The Shaft", "Double Shift", "Both Sides Now", "Finale"
    };

    public readonly string[] levelHints =
    {
        "SHIFT flips the world. Some blocks only exist in one state.",
        "Walls can vanish too. Flip back before the violet spikes.",
        "Flip mid-jump to land on the other state's platform.",
        "Flip between the spike patches. Timing is everything.",
        "Flip while falling to pass through the floor.",
        "Two platforms, two states. Chain your flips mid-jump.",
        "No jumping out of this one. Flip, flip, flip.",
        "Everything you know. Good luck."
    };

    private GameUI _ui;
    private FlipManager _flip;
    private CameraFit _camFit;
    private Camera _cam;
    private LevelBuilder _level;
    private GameObject _levelRoot;
    private PlayerController _player;

    private int _currentLevel;
    private int _unlocked;
    private int _deaths;
    private float _lastRespawn = -10f;
    private bool _clearing;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(TextAsset[] files, Camera cam)
    {
        levelFiles = files;
        _cam = cam;
        _camFit = cam.GetComponent<CameraFit>();
        if (_camFit == null) _camFit = cam.gameObject.AddComponent<CameraFit>();

        _flip = gameObject.AddComponent<FlipManager>();
        _ui = gameObject.AddComponent<GameUI>();
        _ui.Init(StartGame, OpenSelect, PickLevel, BackToTitle, PlayAgain, levelNames);
        _ui.ShowTitle();
        _ui.SetHudVisible(false);

        if (levelFiles == null || levelFiles.Length == 0)
        {
            _ui.ShowToast("No levels assigned - run SHIFT > Setup Project (see SETUP.md)");
            Debug.LogError("SHIFT: Bootstrap has no level files assigned. Run SHIFT > Setup Project in the editor.");
        }
    }

    private void Update()
    {
        if (!IsPlaying) return;
        if (Input.GetKeyDown(KeyCode.R)) LoadLevel(_currentLevel);
        else if (Input.GetKeyDown(KeyCode.Escape)) BackToTitle();
    }

    // ---------- flow ----------

    public void StartGame()
    {
        LoadLevel(0);
    }

    public void OpenSelect()
    {
        _ui.ShowSelect(_unlocked);
    }

    public void PickLevel(int index)
    {
        LoadLevel(index);
    }

    public void PlayAgain()
    {
        _unlocked = 0;
        _deaths = 0;
        LoadLevel(0);
    }

    public void BackToTitle()
    {
        IsPlaying = false;
        _clearing = false;
        ClearLevel();
        if (_player != null)
        {
            _player.ControlsEnabled = false;
            _player.gameObject.SetActive(false);
        }
        _ui.ShowTitle();
        _ui.SetHudVisible(false);
    }

    public void LoadLevel(int index)
    {
        if (levelFiles == null || index < 0 || index >= levelFiles.Length)
        {
            Debug.LogError("SHIFT: cannot load level " + index + " (missing or out of range).");
            return;
        }

        ClearLevel();
        _currentLevel = index;
        _clearing = false;

        _levelRoot = new GameObject("LevelRoot");
        _level = _levelRoot.AddComponent<LevelBuilder>();
        _level.Build(levelFiles[index]);

        _camFit.Fit(_level.Data.Width, _level.Data.Height);

        if (_player == null)
        {
            _player = PlayerController.Create(_level.SpawnWorld);
        }
        _player.gameObject.SetActive(true);
        _player.Respawn(_level.SpawnWorld);
        _player.ControlsEnabled = true;

        _flip.Init(_cam, _player);
        _flip.SetLevel(_level);
        _flip.ResetState();
        _level.ApplyState(WorldState.A);

        IsPlaying = true;
        _ui.HideAll();
        _ui.SetHudVisible(true);
        _ui.SetLevelText("Level " + (index + 1) + " - " + NameFor(index));
        _ui.SetHint(HintFor(index));
        _ui.SetStateText(WorldState.A);
        _ui.SetDeathsText(_deaths);
    }

    public void RespawnPlayer()
    {
        if (!IsPlaying || _clearing || _level == null || _player == null) return;
        if (Time.time - _lastRespawn < 0.3f) return; // avoid double-counting one death
        _lastRespawn = Time.time;

        _deaths++;
        _flip.ResetState();
        _level.ApplyState(WorldState.A);
        _player.Respawn(_level.SpawnWorld);
        _ui.SetStateText(WorldState.A);
        _ui.SetDeathsText(_deaths);
    }

    public void LevelComplete()
    {
        if (!IsPlaying || _clearing) return;
        _clearing = true;
        _unlocked = Mathf.Max(_unlocked, _currentLevel + 1);
        if (_player != null) _player.ControlsEnabled = false;

        if (_currentLevel + 1 >= levelFiles.Length)
        {
            IsPlaying = false;
            _ui.SetHudVisible(false);
            _ui.ShowWin(_deaths);
        }
        else
        {
            _ui.ShowToast("Level Clear!");
            StartCoroutine(LoadNextAfterDelay());
        }
    }

    private IEnumerator LoadNextAfterDelay()
    {
        yield return new WaitForSeconds(0.9f);
        // BackToTitle clears _clearing; don't load if the player bailed out.
        if (_clearing) LoadLevel(_currentLevel + 1);
    }

    public void OnFlipped(WorldState state)
    {
        _ui.SetStateText(state);
    }

    // ---------- helpers ----------

    private void ClearLevel()
    {
        if (_levelRoot != null) Destroy(_levelRoot);
        _levelRoot = null;
        _level = null;
        if (_flip != null) _flip.SetLevel(null);
    }

    private string NameFor(int index)
    {
        return index >= 0 && index < levelNames.Length ? levelNames[index] : "Level " + (index + 1);
    }

    private string HintFor(int index)
    {
        return index >= 0 && index < levelHints.Length ? levelHints[index] : "";
    }
}

using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private UIDocument _document;
    private Button _startNewGameButton;
    private Button _continueButton;
    private Button _loadGameButton;
    private Button _settingsButton;
    private Button _quitButton;

    private void OnEnable()
    {
        _document = GetComponent<UIDocument>();
        if (_document == null)
            return;

        var root = _document.rootVisualElement;

        _startNewGameButton = root.Q<Button>("start-new-game-button");
        _continueButton = root.Q<Button>("continue-button");
        _loadGameButton = root.Q<Button>("load-game-button");
        _settingsButton = root.Q<Button>("settings-button");
        _quitButton = root.Q<Button>("quit-button");

        _startNewGameButton?.RegisterCallback<ClickEvent>(evt => OnStartNewGameClicked());
        _continueButton?.RegisterCallback<ClickEvent>(evt => OnContinueClicked());
        _loadGameButton?.RegisterCallback<ClickEvent>(evt => OnLoadGameClicked());
        _settingsButton?.RegisterCallback<ClickEvent>(evt => OnSettingsClicked());
        _quitButton?.RegisterCallback<ClickEvent>(evt => OnQuitClicked());
    }

    private void OnStartNewGameClicked()
    {
        var command = new GameMain.RunTime.GameFlowCommands.StartGameCommand("Chapter1", "level0");
        command.Execute();
    }

    private void OnContinueClicked()
    {
        Debug.Log("Continue Clicked");
    }

    private void OnLoadGameClicked()
    {
        Debug.Log("Load Game Clicked");
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings Clicked");
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit Clicked");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

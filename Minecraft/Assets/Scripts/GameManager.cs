using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("States & Modes")]
    public ePlatform platform = ePlatform.STANDALONE;
    public eNetworkMode networkMode = eNetworkMode.SINGLEPLAYER;
    public eGameMode gameMode = eGameMode.CREATIVE;
    public eGameState gameState = eGameState.GAMEPLAY;
    [Header("References")]
    public Chat chat;
    public Menu menu;
    public GameUI gameUI;
    public CameraController cameraController;

    public static GameManager singleton;


    private void Awake ()
    {
        singleton = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update ()
    {
        switch (gameState)
        {
            case eGameState.GAMEPLAY:
                GameplayStateMachine();
                break;            
            case eGameState.CHAT:
                ChatStateMachine();
                break;            
            case eGameState.INVENTORY:
                InventoryStateMachine();
                break;            
            case eGameState.MENU:
                MenuStateMachine();
                break;
        }
    }

    private void GameplayStateMachine ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameState = eGameState.MENU;
            Cursor.lockState = CursorLockMode.None;

            gameUI.Toggle();
            menu.Toggle();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            gameState = eGameState.CHAT;

            chat.ToggleInput();
        }
        else if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E))
        {
            gameState = eGameState.INVENTORY;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            cameraController.SwitchCameraMode();
        }
    }

    private void ChatStateMachine ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameState = eGameState.GAMEPLAY;
            Cursor.lockState = CursorLockMode.Locked;

            chat.ToggleInput();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            gameState = eGameState.GAMEPLAY;
            Cursor.lockState = CursorLockMode.Locked;

            chat.SendChatMessage();
        }
    }

    private void InventoryStateMachine ()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E))
        {
            gameState = eGameState.GAMEPLAY;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void MenuStateMachine ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameState = eGameState.GAMEPLAY;
            Cursor.lockState = CursorLockMode.Locked;

            menu.Toggle();
            gameUI.Toggle();
        }
    }

    public void CollectGarbage ()
    {
        System.GC.Collect();
    }

}

public enum eNetworkMode
{
    SINGLEPLAYER,
    MULTIPLAYER
}

public enum eGameMode
{
    CREATIVE,
    SURVIVAL,
    SPECTATOR
}

public enum eGameState
{
    GAMEPLAY,
    CHAT,
    INVENTORY,
    MENU
}

public enum ePlatform
{
    STANDALONE,
    MOBILE
}


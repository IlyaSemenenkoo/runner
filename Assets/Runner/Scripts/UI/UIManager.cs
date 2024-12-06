using UnityEngine;


public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _leaderBoardScreen;
    [SerializeField] private GameObject _mainMenuScreen;
    [SerializeField] private GameObject _scoreScreen;
    [SerializeField] private PlayerMove playerMove;
    private FirebaseAuthManager firebaseAuthManager;

    private void Awake()
    {
        _mainMenuScreen.SetActive(true);
        _scoreScreen.SetActive(false);
        firebaseAuthManager = GetComponent<FirebaseAuthManager>();
    }

    public void StartMove()
    {
        _mainMenuScreen.SetActive(false);
        _scoreScreen.SetActive(true);
        playerMove.StartMove();
    }

    public void Close()
    {
        Application.Quit();
    }

    public void LeaderBoard()
    {
        _leaderBoardScreen.SetActive(true);
        _mainMenuScreen.SetActive(false);
    }

    public void CloseLeaderBoard()
    {
        _leaderBoardScreen.SetActive(false);
        _mainMenuScreen.SetActive(true);
    }

    public void LogOut()
    {
        firebaseAuthManager.Logout();
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}

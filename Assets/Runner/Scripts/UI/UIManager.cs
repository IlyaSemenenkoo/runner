using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _leaderBoardScreen;
    [SerializeField] private GameObject _mainMenuScreen;
    [SerializeField] private GameObject _scoreScreen;
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private Text[] scoreArray;
    [SerializeField] private Text[] nameArray;
    [SerializeField] private Text _yourBestScore;
    [SerializeField] private FirebaseAuthManager _firebaseAuthManager;
    [SerializeField] private FirestoreManager _firestoreManager;
    [SerializeField] private GameObject _register;
    private QuerySnapshot _snapshot;
    

    private void Awake()
    {
        _mainMenuScreen.SetActive(true);
        _scoreScreen.SetActive(false);
    }

    public void StartMove()
    {
        _mainMenuScreen.SetActive(false);
        _scoreScreen.SetActive(true);
        _playerMove.StartMove();
    }

    public void Close()
    {
        Application.Quit();
    }

    public void LeaderBoard()
    {
        StartCoroutine(_firestoreManager.GetTop10Leaderboard());
        StartCoroutine(_firestoreManager.GetCurrentUserHighScore());
        _leaderBoardScreen.SetActive(true);
        _mainMenuScreen.SetActive(false); 
    }

    public void CloseLeaderBoard()
    {
        for(int i = 0; i < scoreArray.Length; i++)
        {
            nameArray[i].text = " ";
            scoreArray[i].text = " ";
        }
        _leaderBoardScreen.SetActive(false);
        _mainMenuScreen.SetActive(true);
    }

    public void LogOut()
    {
        _firebaseAuthManager.Logout();
    }

    public void End()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void WatchAds()
    {

    }
}

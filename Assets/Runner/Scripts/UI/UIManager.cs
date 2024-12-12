using TMPro;
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
    [SerializeField] private TextMeshProUGUI _yourBestScore;
    [SerializeField] private GameObject _deadScreen;
    [SerializeField] private AdMobReward _reward;
    

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
        FirestoreManager.Instance.GetTop(nameArray, scoreArray);
        FirestoreManager.Instance.GetUserScore(_yourBestScore);
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
        FirebaseAuthManager.Instance.Logout();
    }

    public void End()
    {
        _deadScreen.SetActive(false);
        SceneManager.LoadScene("GameScene");
    }

    public void WatchAds()
    {
        _reward.ShowRewardedAd();
        _playerMove.Continue();
        _deadScreen.SetActive(false);
    }
    
    public void Hit()
    {
        _deadScreen.SetActive(true);
    }
}

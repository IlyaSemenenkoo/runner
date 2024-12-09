using Firebase.Firestore;
using UnityEngine;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.UI;



public class FirestoreManager : MonoBehaviour
{
    private FirebaseAuth _auth;
    private FirebaseFirestore _firestore;
    private QuerySnapshot _snapshot;
    private int _highScore;
    [SerializeField] private Text[] scoreArray;
    [SerializeField] private Text[] nameArray;
    [SerializeField] private Text _yourBestScore;

    void Awake()
    {
        // ������������� Firestore
        _firestore = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;
    }

    public IEnumerator SaveUserData(string userId, string email, string login, int highScore)
    {
        DocumentReference userRef = _firestore.Collection("users").Document(userId);

        // �������� ������� ������
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "email", email },
            { "login", login },
            { "highScore", highScore }
        };

        var regTask = userRef.SetAsync(user);

        yield return new WaitUntil(() => regTask.IsCompleted);
        if (regTask.IsCompletedSuccessfully)
        {
            Debug.Log("User data saved to Firestore.");
        }
        if (regTask.Exception != null)
        {
            Debug.LogError("Failed to save user data: " + regTask.Exception?.Message);
        }
    }
  
    public IEnumerator LoadUserData(string userId)
    {
        var usersRef = _firestore.Collection("users");
        var userDoc = usersRef.Document(userId);

        var docTask = userDoc.GetSnapshotAsync();

        yield return new WaitUntil(() => docTask.IsCompleted);

        if (docTask.IsCanceled || docTask.IsFaulted)
        {
            Debug.LogError("Failed to load user data: " + docTask.Exception?.Message);
            StopCoroutine(LoadUserData(userId));
        }

        DocumentSnapshot doc = docTask.Result;
        if (doc.Exists)
        {
            string email = doc.GetValue<string>("email");
            string login = doc.GetValue<string>("login");
            int highscore = doc.GetValue<int>("highscore");

            // ����������� ���������� ������
            Debug.Log($"User data loaded: Email = {email}, Login = {login}, Highscore = {highscore}");
        }
        else
        {
            Debug.LogError("User document not found.");
        }
    }

    public IEnumerator UpdateHighScore(int newHighScore)
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null)
        {
           StopCoroutine(UpdateHighScore(newHighScore));
        }
        string userId = user.UserId;
        DocumentReference userRef = _firestore.Collection("users").Document(userId);

        var snapTask = userRef.GetSnapshotAsync();

        yield return new WaitUntil(() => snapTask.IsCompleted);

        if (snapTask.IsCompletedSuccessfully)
        {
            DocumentSnapshot snapshot = snapTask.Result;
            if (snapshot.Exists)
            {
                int currentHighScore = snapshot.GetValue<int>("highScore");

                // ���� ����� ������ ���� ��������, ��������� ���
                if (newHighScore > currentHighScore)
                {
                    Dictionary<string, object> updatedData = new Dictionary<string, object>
                    {
                        { "highScore", newHighScore }
                    };

                    var updateTask = userRef.UpdateAsync(updatedData);

                    yield return new WaitUntil(() => updateTask.IsCompleted);
                    
                        if (updateTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("High score updated successfully.");
                        }
                        else
                        {
                            Debug.LogError("Failed to update high score: " + updateTask.Exception?.Message);
                        }
                  
                }
                else
                {
                    Debug.Log("New high score is not higher than the current one.");
                }
            }
            else
            {
                Debug.LogError("User data not found.");
            }
        }
        else
        {
            Debug.LogError("Failed to fetch user data: " + snapTask.Exception?.Message);
        }

    }

    public async Task<String> GetUserByLogin(string login)
    {
        try
        {
            // ��� 1: ����� ������������ �� ������
            var usersRef = _firestore.Collection("users");
            var query = usersRef.WhereEqualTo("login", login).Limit(1);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                // ���� ������ �������� �� ����������
                DocumentSnapshot doc = snapshot.Documents.FirstOrDefault();
                if (doc != null)
                {
                    return doc.GetValue<string>("email");                
                }
            }
            else
            {
                Debug.LogError("User not found with this login.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in GetUserByLogin: " + ex.Message);
        }

        return null; // ������� null � ������ ������
    }

    public IEnumerator GetTop10Leaderboard()
    {
        // ������ �� ���������� �� highScore � ������� ��������, ����������� �� 10 �������
        Query leaderboardQuery = _firestore.Collection("users")
            .WhereGreaterThan("highScore", 0)  // ����������, ����� highScore ��� ������ 0
            .OrderByDescending("highScore")  // ���������� �� �������� highScore
            .Limit(10);  // ����������� �� 10 �������

        // ���������� �������
        var snapshotTask = leaderboardQuery.GetSnapshotAsync();

        yield return new WaitUntil(() => snapshotTask.IsCompleted); // ���� ���������� �������

        if (snapshotTask.IsFaulted || snapshotTask.IsCanceled)
        {
            Debug.LogError("Error fetching leaderboard data");
        }
        else
        {
            int index = 0;
            _snapshot = snapshotTask.Result;
            foreach (DocumentSnapshot doc in _snapshot.Documents)
            {
                if (doc != null)
                {
                    nameArray[index].text = doc.GetValue<string>("login");
                    scoreArray[index].text = doc.GetValue<int>("highScore").ToString();
                    index++;
                }
            }
        }
    }

    public IEnumerator GetCurrentUserHighScore()
    {
        // �������� �������� ������������ �� Firebase Auth
        FirebaseUser user = _auth.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;

            // ������ ��� ��������� ��������� ������������ �� userId
            DocumentReference userRef = _firestore.Collection("users").Document(userId);
            var snapshotTask = userRef.GetSnapshotAsync();

            // ���� ���������� �������
            yield return new WaitUntil(() => snapshotTask.IsCompleted);

            if (snapshotTask.IsFaulted || snapshotTask.IsCanceled)
            {
                Debug.LogError("Error fetching user data");
            }
            else
            {
                DocumentSnapshot snapshot = snapshotTask.Result;

                // �������� �� ������������� ���������
                if (snapshot.Exists)
                {
                    _highScore = snapshot.GetValue<int>("highScore");
                    _yourBestScore.text = "Your best score : " + _highScore;
                }
                else
                {
                    Debug.LogError("User document not found.");
                }
            }
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
        }
    }
}

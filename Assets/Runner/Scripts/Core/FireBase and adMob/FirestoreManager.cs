using Firebase.Firestore;
using UnityEngine;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using TMPro;



public class FirestoreManager : MonoBehaviour
{
    private FirebaseAuth _auth;
    private FirebaseFirestore _firestore;
    
    private QuerySnapshot _snapshot;
    private int _highScore;
    
    public static FirestoreManager Instance;

    private void CreateInstance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Awake()
    {
        _firestore = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;
        DontDestroyOnLoad(this.gameObject);
        CreateInstance();
    }

    public IEnumerator SaveUserData(string userId, string email, string login, int highScore)
    {
        DocumentReference userRef = _firestore.Collection("users").Document(userId);
        
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

    public async Task<string> GetUserByLoginAsync(string login)
    {
        Query query = _firestore.Collection("users").WhereEqualTo("login", login).Limit(1);
        QuerySnapshot snapshots = await query.GetSnapshotAsync();

        if (snapshots.Count != 0)
        {
            foreach (var doc in snapshots)
            {
                if (doc != null && doc.GetValue<string>("login") == login)
                {
                    return doc.GetValue<string>("email");
                }
            }
        }

        Debug.LogWarning("User not found with this login.");
        return null;
    }

    public void GetTop(Text[] name, Text[] score)
    {
        StartCoroutine(GetTop10Leaderboard(name, score));
    }

    private IEnumerator GetTop10Leaderboard(Text[] nameArray, Text[] scoreArray)
    {
        Query leaderboardQuery = _firestore.Collection("users")
            .WhereGreaterThan("highScore", 0)
            .OrderByDescending("highScore")
            .Limit(10);
        
        var snapshotTask = leaderboardQuery.GetSnapshotAsync();

        yield return new WaitUntil(() => snapshotTask.IsCompleted);

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

    public void GetUserScore(TextMeshProUGUI score)
    {
        StartCoroutine(GetCurrentUserHighScore(score));
    }

    private IEnumerator GetCurrentUserHighScore(TextMeshProUGUI _yourBestScore)
    {
        FirebaseUser user = _auth.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;

            DocumentReference userRef = _firestore.Collection("users").Document(userId);
            var snapshotTask = userRef.GetSnapshotAsync();

            yield return new WaitUntil(() => snapshotTask.IsCompleted);

            if (snapshotTask.IsFaulted || snapshotTask.IsCanceled)
            {
                Debug.LogError("Error fetching user data");
            }
            else
            {
                DocumentSnapshot snapshot = snapshotTask.Result;

                if (snapshot.Exists)
                {
                    _highScore = snapshot.GetValue<int>("highScore");
                    _yourBestScore.text = "YOUR BEST SCORE : " + _highScore;
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

using Firebase.Firestore;
using UnityEngine;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections;



public class FirestoreManager : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;

    void Awake()
    {
        // Инициализация Firestore
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    public IEnumerator SaveUserData(string userId, string email, string login, int highScore)
    {
        DocumentReference userRef = firestore.Collection("users").Document(userId);

        // Создание объекта данных
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
        var usersRef = firestore.Collection("users");
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

            // Используйте полученные данные
            Debug.Log($"User data loaded: Email = {email}, Login = {login}, Highscore = {highscore}");
        }
        else
        {
            Debug.LogError("User document not found.");
        }
    }


    public IEnumerator SignInWithEmail(string email, string password)
    {
        var authTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => authTask.IsCompleted);

        if(authTask.IsCanceled || authTask.IsFaulted)
        {
            Debug.LogError("Login Failed: " + authTask.Exception?.Message);
            StopCoroutine(SignInWithEmail(email, password));
        }
        AuthResult authResult = authTask.Result;
        FirebaseUser user = authResult.User;

        Debug.Log("User logged in successfully: " + user.Email);

        // Загружаем данные пользователя из Firestore
        StartCoroutine(LoadUserData(user.UserId));

    }

    // Метод для обновления рекорда

    public IEnumerator UpdateHighScore(int newHighScore)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
           StopCoroutine(UpdateHighScore(newHighScore));
        }
        string userId = user.UserId;
        DocumentReference userRef = firestore.Collection("users").Document(userId);

        var snapTask = userRef.GetSnapshotAsync();

        yield return new WaitUntil(() => snapTask.IsCompleted);

        if (snapTask.IsCompletedSuccessfully)
        {
            DocumentSnapshot snapshot = snapTask.Result;
            if (snapshot.Exists)
            {
                int currentHighScore = snapshot.GetValue<int>("highScore");

                // Если новый рекорд выше текущего, обновляем его
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
            // Шаг 1: Поиск пользователя по логину
            var usersRef = firestore.Collection("users");
            var query = usersRef.WhereEqualTo("login", login).Limit(1);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                // Берём первый документ из результата
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

        return null; // Возврат null в случае ошибки
    }



}

using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


public class FirestoreManager : MonoBehaviour
{
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;

    void Start()
    {
        // Инициализация Firestore
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    // Метод для сохранения данных пользователя
    public void SaveUserData(string userId, string email, string login, string name, int highScore)
    {
        DocumentReference userRef = firestore.Collection("users").Document(userId);

        // Создание объекта данных
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "email", email },
            { "login", login },
            { "name", name },
            { "highScore", highScore }
        };

        // Запись данных в Firestore
        userRef.SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("User data saved to Firestore.");
            }
            else
            {
                Debug.LogError("Failed to save user data: " + task.Exception?.Message);
            }
        });
    }

    // Метод для загрузки данных пользователя
    public void LoadUserData(string userId)
    {
        DocumentReference userRef = firestore.Collection("users").Document(userId);

        // Получаем документ пользователя
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string email = snapshot.GetValue<string>("email");
                    string login = snapshot.GetValue<string>("login");
                    string name = snapshot.GetValue<string>("name");
                    int highScore = snapshot.GetValue<int>("highScore");

                    Debug.Log($"Loaded User Data: Email={email}, Login={login}, Name={name}, HighScore={highScore}");
                }
                else
                {
                    Debug.LogError("User data not found.");
                }
            }
            else
            {
                Debug.LogError("Failed to load user data: " + task.Exception?.Message);
            }
        });
    }

    // Метод для входа через email и пароль
    public void SignInWithEmail(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Login Failed: " + task.Exception?.Message);
                return;
            }

            // Получаем результат авторизации
            AuthResult authResult = task.Result;
            FirebaseUser user = authResult.User;

            Debug.Log("User logged in successfully: " + user.Email);

            // Загружаем данные пользователя из Firestore
            LoadUserData(user.UserId);
        });
    }

    // Метод для обновления рекорда
    public void UpdateHighScore(int newHighScore)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            return;
        }

        string userId = user.UserId;
        // Получаем ссылку на документ пользователя в Firestore
        DocumentReference userRef = firestore.Collection("users").Document(userId);

        // Получаем текущие данные пользователя
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
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

                        // Обновляем в Firestore
                        userRef.UpdateAsync(updatedData).ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                Debug.Log("High score updated successfully.");
                            }
                            else
                            {
                                Debug.LogError("Failed to update high score: " + updateTask.Exception?.Message);
                            }
                        });
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
                Debug.LogError("Failed to fetch user data: " + task.Exception?.Message);
            }
        });
    }


    public Task<FirebaseUser> GetUserByLogin(string login, string password)
    {
        var usersRef = firestore.Collection("users");
        var query = usersRef.WhereEqualTo("login", login).Limit(1); // Поиск по логину

        return query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count > 0)
                {
                    // Используем FirstOrDefault для получения первого документа
                    DocumentSnapshot doc = snapshot.Documents.FirstOrDefault();
                    if (doc != null)
                    {
                        string email = doc.GetValue<string>("email");

                        // Пытаемся войти с найденным email и переданным паролем
                        return auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(authTask =>
                        {
                            if (authTask.IsCompletedSuccessfully)
                            {
                                // Извлекаем FirebaseUser из AuthResult
                                FirebaseUser user = authTask.Result.User;
                                return user; // Возвращаем FirebaseUser
                            }
                            else
                            {
                                Debug.LogError("Failed to sign in: " + authTask.Exception?.Message);
                                return null;
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("Failed to retrieve document.");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError("User not found with this login.");
                    return null; // Если пользователь не найден
                }
            }
            else
            {
                Debug.LogError("Failed to fetch user data: " + task.Exception?.Message);
                return null; // Ошибка при поиске пользователя
            }
        }).Unwrap(); // Unwrap для правильной работы с Task<Task<FirebaseUser>>
    }




}

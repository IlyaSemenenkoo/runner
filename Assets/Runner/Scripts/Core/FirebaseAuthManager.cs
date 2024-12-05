using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using System;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirestoreManager firestore;

    public event Action<string> AuthError;
    private string _passwordError = "Passwords do not match!";
    private string _registrFaild = "Registration Failed.";
    private string _loginError = "Login Failed";

    public event Action<string> AuthSuccess;
    private string _registSuccess = "Registration Success";
    private string _loginSuccess = "Login Success";

    void Start()
    {
        firestore = GetComponent<FirestoreManager>();
        // Инициализация Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;

                // Инициализация Firebase Auth
                auth = FirebaseAuth.DefaultInstance;
                

                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    // Регистрация пользователя с подтверждением пароля
    public void RegisterUser(string email, string password, string confirmPassword, string login, string name)
    {
        if (password.Length < 6)
        {
            Debug.LogError("Password must be at least 6 characters long.");
            AuthError.Invoke(_passwordError);
            return;
        }

        // Проверка совпадения паролей
        if (password != confirmPassword)
        {
            Debug.LogError(_passwordError);
            AuthError.Invoke(_passwordError);
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Registration Failed: " + task.Exception);
                AuthError.Invoke(_registrFaild);
                return;
            }

            // Получаем результат регистрации
            AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;

            Debug.Log("User registered with UID: " + newUser.UserId);

            // Сохраняем данные пользователя в Firestore
            firestore.SaveUserData(newUser.UserId, email, login, name, 0); // 0 - начальный рекорд
            AuthSuccess.Invoke(_registSuccess);
        });
    }

    // Вход с email и паролем
    public void LoginUserWithEmail(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Login Failed: " + task.Exception);
                AuthError.Invoke(_loginError);
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log("User logged in successfully: " + user.Email);

            // Загружаем данные пользователя из Firestore
            firestore.LoadUserData(user.UserId);
            AuthSuccess.Invoke(_loginSuccess);
        });
    }

    // Вход через логин
    public void LoginUserWithUsername(string login, string password)
    {
        // Ищем пользователя по логину в Firestore
        firestore.GetUserByLogin(login, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error fetching user by login: " + task.Exception);
                AuthError.Invoke(_loginError);
                return;
            }

            FirebaseUser user = task.Result;
            if (user != null)
            {
                Debug.Log("User found by login: " + user.Email);
                // Пытаемся войти с найденным email и паролем
                LoginUserWithEmail(user.Email, password);
            }
            else
            {
                Debug.LogError("User not found with this login.");
                AuthError.Invoke(_loginError);
            }
        });
    }

    // Выход пользователя
    public void Logout()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("User logged out successfully.");
    }
}

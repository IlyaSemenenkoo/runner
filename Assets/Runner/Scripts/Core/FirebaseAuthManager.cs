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
        // ������������� Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;

                // ������������� Firebase Auth
                auth = FirebaseAuth.DefaultInstance;
                

                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    // ����������� ������������ � �������������� ������
    public void RegisterUser(string email, string password, string confirmPassword, string login, string name)
    {
        if (password.Length < 6)
        {
            Debug.LogError("Password must be at least 6 characters long.");
            AuthError.Invoke(_passwordError);
            return;
        }

        // �������� ���������� �������
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

            // �������� ��������� �����������
            AuthResult authResult = task.Result;
            FirebaseUser newUser = authResult.User;

            Debug.Log("User registered with UID: " + newUser.UserId);

            // ��������� ������ ������������ � Firestore
            firestore.SaveUserData(newUser.UserId, email, login, name, 0); // 0 - ��������� ������
            AuthSuccess.Invoke(_registSuccess);
        });
    }

    // ���� � email � �������
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

            // ��������� ������ ������������ �� Firestore
            firestore.LoadUserData(user.UserId);
            AuthSuccess.Invoke(_loginSuccess);
        });
    }

    // ���� ����� �����
    public void LoginUserWithUsername(string login, string password)
    {
        // ���� ������������ �� ������ � Firestore
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
                // �������� ����� � ��������� email � �������
                LoginUserWithEmail(user.Email, password);
            }
            else
            {
                Debug.LogError("User not found with this login.");
                AuthError.Invoke(_loginError);
            }
        });
    }

    // ����� ������������
    public void Logout()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("User logged out successfully.");
    }
}

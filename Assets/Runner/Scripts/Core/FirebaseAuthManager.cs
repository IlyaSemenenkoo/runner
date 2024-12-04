using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore firestore;

    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase successfully initialized.");

                // Attempt silent login
                TrySilentLogin();
            }
            else
            {
                Debug.LogError($"Firebase initialization failed: {task.Result}");
            }
        });
    }

    // Registration method
    public void RegisterUser(string email, string password, string confirmPassword, string name)
    {
        if (password != confirmPassword)
        {
            Debug.LogError("Passwords do not match.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Registration failed: " + task.Exception);
                return;
            }

            // Get the FirebaseUser
            user = task.Result.User;
            Debug.Log("User registered: " + user.Email);

            // Save user data (if needed)
            SaveUserData(user.UserId, name);
        });
    }

    // Login method
    public void LoginUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Login failed: " + task.Exception);
                return;
            }

            user = task.Result.User;
            Debug.Log("User logged in: " + user.Email);

            // Save credentials locally for silent login
            PlayerPrefs.SetString("FirebaseEmail", email);
            PlayerPrefs.SetString("FirebasePassword", password);
            PlayerPrefs.Save();
        });
    }

    // Silent login
    private void TrySilentLogin()
    {
        string savedEmail = PlayerPrefs.GetString("FirebaseEmail", null);
        string savedPassword = PlayerPrefs.GetString("FirebasePassword", null);

        if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
        {
            LoginUser(savedEmail, savedPassword);
        }
        else
        {
            Debug.Log("No saved credentials. User needs to log in.");
        }
    }

    // Logout method
    public void LogoutUser()
    {
        auth.SignOut();
        Debug.Log("User logged out.");

        // Clear saved credentials
        PlayerPrefs.DeleteKey("FirebaseEmail");
        PlayerPrefs.DeleteKey("FirebasePassword");
    }

    // Save additional user data
    private void SaveUserData(string userId, string name)
    {
        DocumentReference docRef = firestore.Collection("users").Document(userId);
        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "name", name },
            { "email", user.Email }
        };

        docRef.SetAsync(userData).ContinueWith(task => {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("User data successfully saved.");
            }
            else
            {
                Debug.LogError("Failed to save user data: " + task.Exception);
            }
        });
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Registration : MonoBehaviour
{
    [SerializeField] private GameObject registerWindow;
    [SerializeField] private GameObject loginWindow;
    [SerializeField] private Button backButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;
    public static Registration instance;

    private void Awake()
    {
        registerWindow.SetActive(false);
        loginWindow.SetActive(false);
        backButton.gameObject.SetActive(false);
        CreateInstance();
    }

    private void CreateInstance()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void Back()
    {
        registerWindow.SetActive(false);
        loginWindow.SetActive(false);
        backButton.gameObject.SetActive(false);
        registerButton.gameObject.SetActive(true);
        loginButton.gameObject.SetActive(true);
    }

    public void Login()
    {
        loginWindow.SetActive(true);
        backButton.gameObject.SetActive(true);
        registerButton.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);
    }

    public void Register()
    {
        registerWindow.SetActive(true);
        backButton.gameObject.SetActive(true);
        registerButton.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);
    }
}

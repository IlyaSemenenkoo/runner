using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Registration : MonoBehaviour
{
    [SerializeField] private GameObject registerWindow;
    [SerializeField] private GameObject loginWindow;
    [SerializeField] private Button backButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;
    public static Registration instance;
    [Header("Login")]
    [SerializeField] private TMP_InputField loginLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;
    [SerializeField] private TMP_Text warmingLoginText;
    [SerializeField] private TMP_Text confirmLoginText;

    [Header("Register")]
    [SerializeField] private TMP_InputField loginRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterVerifyField;
    [SerializeField] private TMP_Text warningRegisterText;

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

    public void LoginButton()
    {
        FirebaseAuthManager.Instance.LoginButton(loginLoginField.text, passwordLoginField.text);
    }

    public void RegisterButton()
    {
        FirebaseAuthManager.Instance.RegisterButton(emailRegisterField.text, passwordRegisterField.text, loginRegisterField.text, passwordRegisterVerifyField.text);
    }

    public void Register()
    {
        registerWindow.SetActive(true);
        backButton.gameObject.SetActive(true);
        registerButton.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);
    }

    public void AuthComplite()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void WarmingLogin(string warming)
    {
        warmingLoginText.text = warming;
    }

    public void ConfirmLogin(string confirm)
    {
        warmingLoginText.text = confirm;
    }

    public void WarmingReg(string warming)
    {
        warningRegisterText.text = warming;
    }
}

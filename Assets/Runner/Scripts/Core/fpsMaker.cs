using UnityEngine;

public class fpsMaker : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}

using UnityEngine;

public class FpsMaker : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}

using UnityEngine;

public class SectionDestroy : MonoBehaviour
{
    private string _playerTag = "Player";
    private LevelGenerate _levelGenerate;

    private void Awake()
    {
        _levelGenerate = FindAnyObjectByType<LevelGenerate>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            _levelGenerate.DestroySection();
        }
    }
}

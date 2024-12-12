using UnityEngine;

public class SectionDestroy : MonoBehaviour
{
    private string _playerTag = "Player";
    [SerializeField] private LevelGenerate _levelGenerate;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            _levelGenerate.DestroySection();
        }
    }
}

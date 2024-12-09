using UnityEngine;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    [SerializeField] private FirebaseAuthManager _authManager;
    [SerializeField] private FirestoreManager _fireStoreManager;
    public override void InstallBindings()
    {
        Container.BindInstance(_authManager).AsSingle().NonLazy();
        Container.BindInstance(_fireStoreManager).AsSingle().NonLazy();
    }
}

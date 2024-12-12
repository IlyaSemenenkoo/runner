using UnityEngine;

public class SingleObstacleFactory : ObstacleFactory
{
    [SerializeField] private GameObject[] _passableObstacles;  
    [SerializeField] private GameObject[] _singleObstacles;   
    private GameObject _selectedObstacle;
    private float _emptySpawnChance = 0.3f;
    private float _passableChance = 0.5f;
    private int _countSingleObstacles;

    public override void CreateObstacle(GameObject obstacle, Vector3 position, Transform parent)
    {
        Instantiate(obstacle, position, Quaternion.identity, parent);
    }

    public void SelecteObstacle(Transform[] spawnPoints)
    {
        _countSingleObstacles = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (Random.value < _passableChance || _countSingleObstacles == 2)
            {
                if (Random.value < _emptySpawnChance)  
                {
                    continue;  
                }
                else
                {
                    int passableObstacleIndex = Random.Range(0, _passableObstacles.Length);
                    _selectedObstacle = _passableObstacles[passableObstacleIndex];
                }
            }
            else
            {
                _countSingleObstacles++;
                int singleObstacleIndex = Random.Range(0, _singleObstacles.Length);
                _selectedObstacle = _singleObstacles[singleObstacleIndex];
            }

            CreateObstacle(_selectedObstacle, spawnPoints[i].position, spawnPoints[i]);
        }
    }
}

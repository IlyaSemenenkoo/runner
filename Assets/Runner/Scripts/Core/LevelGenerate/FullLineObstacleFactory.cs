using UnityEngine;

public class FullLineObstacleFactory : ObstacleFactory
{
    [SerializeField] private GameObject[] _fullLineObstacles;
    private int _center = 1;

    public void SelecteObstacle(Transform[] spawnPoints)
    {
        int index = Random.Range(0, _fullLineObstacles.Length);
        CreateObstacle(_fullLineObstacles[index], spawnPoints[_center].position, spawnPoints[_center]);
    }

    public override void CreateObstacle(GameObject obstacle, Vector3 position, Transform parent)
    {
        Instantiate(obstacle, position, Quaternion.identity, parent);
    }
}

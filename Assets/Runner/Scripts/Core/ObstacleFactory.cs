using UnityEngine;

public abstract class ObstacleFactory : MonoBehaviour
{
    public abstract void CreateObstacle(GameObject obstacle, Vector3 position, Transform parent);
}

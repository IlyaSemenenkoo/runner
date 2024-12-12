using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerate : MonoBehaviour
{
    [SerializeField] private GameObject[] _sections; // Prefabs of level sections
    [SerializeField] private FullLineObstacleFactory _fullLineObstacleFactory;
    [SerializeField] private SingleObstacleFactory _singleObstacleFactory;// Reference to the obstacle factory
    private int _zPos = 50; // Z-coordinate for the next section
    private float _fullLineObstacleChance = 0.3f;

    private Queue<GameObject> _sectionQueue = new Queue<GameObject>(); // Queue to manage active sections
    private int _queueLimit = 15;

    private void Start()
    {
        for (int i = 0; i < _queueLimit; i++)
        {
            StartCoroutine(GenerateSection());
        }
    }

    public void DestroySection()
    {
        GameObject oldSection = _sectionQueue.Dequeue();
        Destroy(oldSection); // Remove the old section from the game world
        StartCoroutine(GenerateSection());
    }

    IEnumerator GenerateSection()
    {
        // Create a random section
        int secNum = Random.Range(0, _sections.Length);
        GameObject tempSection = Instantiate(_sections[secNum], new Vector3(0, 0, _zPos), Quaternion.identity);

        // Add the section to the queue
        _sectionQueue.Enqueue(tempSection);

        // Find all lines within the section
        foreach (Transform line in tempSection.transform.Find("Lines"))
        {
            // Get spawn points for the current line
            Transform[] spawnPoints = new Transform[line.childCount];
            for (int i = 0; i < line.childCount; i++)
            {
                spawnPoints[i] = line.GetChild(i);
            }

            if (Random.value < _fullLineObstacleChance)
            {
                _fullLineObstacleFactory.SelecteObstacle(spawnPoints);
            }
            else
            {
                _singleObstacleFactory.SelecteObstacle(spawnPoints);
            }
                
        }

        // Increment the Z-coordinate for the next section
        _zPos += 50;

        // Delay before creating the next section
        yield return new WaitForSeconds(1);
    }
}

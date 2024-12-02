using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float forwardSpeed = 5f; // Скорость движения вперед
    public float laneChangeSpeed = 7f; // Скорость смены дорожки

    private int currentLane = 1; // Текущая дорожка: 0 = левая, 1 = средняя, 2 = правая
    private float[] lanes = { -2f, 0f, 2f }; // Координаты дорожек по оси X
    private Vector3 targetPosition; // Целевая позиция персонажа для смены дорожки

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        // Постоянное движение вперед
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // Обработка ввода
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }

        // Плавное перемещение между дорожками
        Vector3 desiredPosition = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * laneChangeSpeed);
    }

    private void MoveLeft()
    {
        if (currentLane > 0)
        {
            currentLane--;
            targetPosition.x = lanes[currentLane];
        }
    }

    private void MoveRight()
    {
        if (currentLane < lanes.Length - 1)
        {
            currentLane++;
            targetPosition.x = lanes[currentLane];
        }
    }
}

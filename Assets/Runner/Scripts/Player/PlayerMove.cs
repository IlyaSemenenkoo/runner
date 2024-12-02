using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float forwardSpeed = 5f; // �������� �������� ������
    public float laneChangeSpeed = 7f; // �������� ����� �������

    private int currentLane = 1; // ������� �������: 0 = �����, 1 = �������, 2 = ������
    private float[] lanes = { -2f, 0f, 2f }; // ���������� ������� �� ��� X
    private Vector3 targetPosition; // ������� ������� ��������� ��� ����� �������

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        // ���������� �������� ������
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // ��������� �����
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }

        // ������� ����������� ����� ���������
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

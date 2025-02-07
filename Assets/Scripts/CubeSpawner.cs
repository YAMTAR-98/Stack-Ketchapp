using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public MovingCube cubePrefab;

    [SerializeField] private MoveDirection moveDirection;


    internal void SpawnCube()
    {
        MovingCube spawnedCube = Instantiate(cubePrefab);

        if (MovingCube.LastCube != null && !MovingCube.LastCube.isStartCube)
        {
            float x = moveDirection == MoveDirection.X ? transform.position.x : MovingCube.LastCube.transform.position.x;
            float z = moveDirection == MoveDirection.Z ? transform.position.z : MovingCube.LastCube.transform.position.z;

            spawnedCube.transform.position = new Vector3(x, MovingCube.LastCube.transform.position.y + cubePrefab.transform.localScale.y, z);
        }
        else
        {
            spawnedCube.transform.position = transform.position;
            Debug.Log("asdasdasd");
        }


        spawnedCube.MoveDirection = moveDirection;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, cubePrefab.transform.localScale);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Transform targetPlatform;
    public LevelManager levelManager;
    public float moveSpeed = 2f;
    public float rotationSpeed = 10f;
    public float targetThreshold = 0.1f;
    internal Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        levelManager.OpenOrCloseStartUI(true);
        GameManager.Instance.FinisSession();

    }

    void Update()
    {
        if (transform.position.y < -10)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (targetPlatform != null)
        {

            Vector3 currentPos = transform.position;
            Vector3 targetPos = new Vector3(targetPlatform.position.x, currentPos.y, targetPlatform.position.z);
            float distance = Vector3.Distance(currentPos, targetPos);

            if (distance > targetThreshold)
            {
                if (animator != null)
                    animator.SetBool("Dance", false);

                Vector3 localTarget = transform.InverseTransformPoint(targetPos);

                Vector3 localMove = Vector3.ClampMagnitude(localTarget, moveSpeed * Time.deltaTime);

                Vector3 worldMove = transform.TransformDirection(localMove);

                transform.position += worldMove;
            }
            else
            {
                if (animator != null)
                    animator.SetBool("Dance", true);
            }
        }
    }
    public void SetTarget(Transform newTarget)
    {
        targetPlatform = newTarget;

        if (animator != null && newTarget != null)
            animator.SetBool("Run", true);
    }
}




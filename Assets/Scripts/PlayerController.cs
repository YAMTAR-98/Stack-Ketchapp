using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Hedef platformun Transform'u. Inspector üzerinden veya SetTarget metodu ile atanabilir.
    public Transform targetPlatform;

    // Hareket ve dönme hızları
    public float moveSpeed = 2f;
    public float rotationSpeed = 10f;

    // Hedefe ulaşma mesafe eşiği
    public float targetThreshold = 0.1f;

    // Animator referansı
    private Animator animator;

    void Start()
    {
        // Bu scriptin bağlı olduğu GameObject'deki Animator bileşenini alıyoruz.
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (targetPlatform != null)
        {
            float distance = Vector3.Distance(transform.position, targetPlatform.position);

            if (distance > targetThreshold)
            {
                if (animator != null)
                    animator.SetBool("Dance", false);

                transform.position = Vector3.MoveTowards(transform.position, targetPlatform.position, moveSpeed * Time.deltaTime);

                Vector3 direction = targetPlatform.position - transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
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

        // Yeni hedefe geçişte dansı kapat.
        if (animator != null)
            animator.SetBool("Dance", false);
    }
}

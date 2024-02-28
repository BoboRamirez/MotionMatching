using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private Vector3 m_Movement;
    [SerializeField] private Quaternion m_Rotation = Quaternion.identity;
    [SerializeField] bool hasHorizontalInput;
    [SerializeField] bool hasVerticalInput;
    Animator m_Animator;
    Rigidbody m_Rigidbody;
    public float turnSpeed = 20f;
    private Vector3 desiredForward;
    bool IsWalking
    {
        get
        {
            return hasHorizontalInput || hasVerticalInput;
        }
    }
    float horizontal, vertical;
    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        //Time.timeScale = 0.1f;
    }

    private void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();
        hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        m_Animator.SetBool("IsWalking", IsWalking);
        desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
    }

    private void OnAnimatorMove()
    {
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
        m_Rigidbody.MoveRotation(m_Rotation);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    public bool IsTarget { get; set; }

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        _animator.SetBool("IsTarget", IsTarget);
    }
}

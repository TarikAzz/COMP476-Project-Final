using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An indicator for character movement
/// </summary>
public class Indicator : MonoBehaviour
{
    #region Public properties

    /// <summary>
    /// Whether the indicator is the current target of a character
    /// </summary>
    public bool IsTarget { get; set; }

    #endregion
    
    #region Private variables

    /// <summary>
    /// The animator component
    /// </summary>
    private Animator _animator;

    #endregion

    /// <summary>
    /// Initializes the components
    /// </summary>
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Updates the animator variables 
    /// </summary>
    void Update()
    {
        _animator.SetBool("IsTarget", IsTarget);
    }
}

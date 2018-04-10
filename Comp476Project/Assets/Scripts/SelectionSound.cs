using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSound : MonoBehaviour
{
    public AudioClip defenderSelect;
    public AudioClip defenderOk;

    public AudioClip infiltratorSelect;
    public AudioClip infiltratorOk;

    public void PlayDefenderSelect()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(defenderSelect);
    }

    public void PlayDefenderOk()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(defenderOk);
    }

    public void PlayInfiltratorSelect()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(infiltratorOk);
    }

    public void PlayInfiltratorOk()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(infiltratorOk);
    }

}

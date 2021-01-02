using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        [SerializeField] private bool playAtStart = false;
        private bool alreadyTriggered = false;

        private void Start()
        {
            if (playAtStart)
            {
                GetComponent<PlayableDirector>().Play();
            }
        }

        IEnumerator PlayCinematics()
        {
            yield return new WaitForSeconds(0.01f);
            if (playAtStart)
            {
                GetComponent<PlayableDirector>().Play();
            }
        }



        // used to trigger cinematic with trigger collider when player enters
        // dont forget to make layer to "ignore raycast"
        // automatically disabling posibility to play it again by entering same collider
        private void OnTriggerEnter(Collider other)
        {
            if (!alreadyTriggered && other.gameObject.tag == "Player")
            {
               GetComponent<PlayableDirector>().Play();
               alreadyTriggered = true;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RPG.Saving;

namespace RPG.SceneManagement
{
    enum DestinationIdentifier
    {
        A, B, C, D, E
    }

    public class Portal : MonoBehaviour
    {
        [SerializeField] private int sceneToLoad = -1; // to get error when we forget to put correct scene to load
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private DestinationIdentifier destination;

        [Header("Fading In and Out")]
        [SerializeField] private float fadeIn = 2f;
        [SerializeField] private float fadeOut = 2f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.parent.gameObject.tag == "Player")
            {
                StartCoroutine(Transition());
            }
        }        

        private IEnumerator Transition()
        {
            if (sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set!");
                yield break;
            }

            Fader fader = FindObjectOfType<Fader>();
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            DontDestroyOnLoad(gameObject);

            yield return fader.FadeOut(fadeOut);

            savingWrapper.Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoad);

            savingWrapper.Load();

            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);

            savingWrapper.Save();

            yield return fader.FadeIn(fadeIn);

            Destroy(gameObject);
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnPoint.position); // to prevent conflicts between NavMeshAgent
            player.transform.rotation = otherPortal.spawnPoint.rotation;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue;
                if (this.destination != portal.destination) continue;
                
                return portal;                
            }
            return null;
        }
    }
}


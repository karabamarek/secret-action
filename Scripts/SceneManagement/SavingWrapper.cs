using RPG.Saving;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";

        [SerializeField] float fadeInTime = 2f;

        int currentSceneIndex;
        int savedSceneIndex;

        // if I want to start where I finished //
        //private IEnumerator Start()
        //{
        //    Fader fader = FindObjectOfType<Fader>();
        //    fader.FadeOutImmediate();
        //    yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
        //    yield return fader.FadeIn(fadeInTime);
        //}
      

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                savedSceneIndex = SceneManager.GetActiveScene().buildIndex;
                Save();
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                Load();
            }
        }        
       

        public void Load()
        {
            GetComponent<SavingSystem>().Load(defaultSaveFile);
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }
    }
}

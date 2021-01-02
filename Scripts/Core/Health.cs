using RPG.Saving;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] private float healthPoints = 20f;
        [SerializeField] private AudioClip deathSound;

        private bool isDead = false;

        public bool IsDead()
        {
            return isDead;
        }      

        public void TakeDamage(float damage)
        {
            healthPoints = Mathf.Max(healthPoints - damage, 0);
           
            if (healthPoints == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
      

        // saving system //

        public object CaptureState()
        {
            return healthPoints;
        }

        public void RestoreState(object state)
        {
            healthPoints = (float)state;
            if (healthPoints <= 0)
            {
                Die();
            }
            if (healthPoints > 0 && isDead)
            {  
             //   GetComponent<Animator>().SetTrigger("revive");
                isDead = false;
            }
        }
    }
}

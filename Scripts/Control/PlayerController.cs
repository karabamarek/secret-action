using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Movement;
using RPG.Combat;
using RPG.Core;
using System;
using UnityEngine.EventSystems;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        private NavMeshAgent agent;
        private float lastClickTime;
        private float timeSiceLastClick;
        private float clickDelay = 0.2f;
        private Health health;

        enum CursorType
        {
            None,
            Movement,
            Combat,
            UI
        }

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }
        [SerializeField] CursorMapping[] cursorMappings = null;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        private void Update()
        {
            if (InteractWithUI()) return;
            if (health.IsDead())
            {
                SetCursor(CursorType.None);
                return;
            }

            if (InteractWithComponent()) return;
            if (InteractWithCombat()) return;
            if (InteractWithMovement()) return;
            SetCursor(CursorType.None);
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast())
                    {
                        SetCursor(CursorType.Combat);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckForDoubleClick()
        {
            timeSiceLastClick = Time.time - lastClickTime;
            lastClickTime = Time.time;

            if (timeSiceLastClick <= clickDelay) return true;
            else return false;
        }

        private bool InteractWithUI()
        {
            // this only reacts with UI gameObjects
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }
            return false;
        }

        private bool InteractWithCombat()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                CombatTarget target = hit.transform.GetComponent<CombatTarget>();
                if (target == null) continue;

                if (!GetComponent<Fighter>().CanAttack(target.gameObject)) continue;
 //                  TurningOnAndOffFoV(target);         Possible feature to be able to not see the FoV of each enemy and be able to turn it on only for enemies that player wants
                if (Input.GetMouseButtonDown(0))
                {
                    if (!CheckForDoubleClick())
                    {
                        agent.speed = 3f;
                        GetComponent<Fighter>().Attack(target.gameObject);
                    }
                    else if (CheckForDoubleClick())
                    {
                        agent.speed = 5.66f;
                        GetComponent<Fighter>().Attack(target.gameObject);
                    }
                }
                SetCursor(CursorType.Combat);
                return true;
            }
            return false;
        }      

        //     private static void TurningOnAndOffFoV(CombatTarget target)
        //     {
        //         if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        //         {
        //             target.transform.GetChild(1).transform.GetChild(0).GetComponent<MeshRenderer>().gameObject.SetActive(false);
        //        }
        //     }

        private bool InteractWithMovement()
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (hasHit)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!CheckForDoubleClick())
                    {
                        agent.speed = 3f;
                        GetComponent<Mover>().StartMoveAction(hit.point);
                    }
                    else if (CheckForDoubleClick())
                    {
                        agent.speed = 5.66f;
                        GetComponent<Mover>().StartMoveAction(hit.point);
                    }
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;            
        }

        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping cursorMapping in cursorMappings)
            {
                if (cursorMapping.type == type)
                {
                    return cursorMapping;
                }
            }
            return cursorMappings[0];
        }      


        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
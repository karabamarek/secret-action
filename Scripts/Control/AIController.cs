using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using System;
using RPG.Saving;

namespace RPG.Control
{
    public class AIController : MonoBehaviour, ISaveable
    {
        // TODO
        // Guard should also rotate around while suspicious

        public bool isLoading = false;

        [SerializeField] private float chaseDistance = 5f;
        [SerializeField] private PatrolPath patrolPath;
        [SerializeField] private FieldOfView fieldOfView;
        [SerializeField] private float startingSuspiciousTime = 10f;
        [SerializeField] private float waypointTolerance = 1f;
        [SerializeField] private float waypointDwellTime = 3f;
        [SerializeField] private float agentPatrolSpeed = 2f;
        [SerializeField] private float agentAttackSpeed = 4f;
        [SerializeField] private float speedOfRotation = 5f;

        [Header("Look around")]
        [SerializeField] private Transform head;
        [SerializeField] private bool lookAround = true;
        [SerializeField] private float maxRotation = 75f;
        [SerializeField] private float speedOfRotationInDegreesPerSecond = 45f;

        private GameObject player;
        private Health health;
        private Fighter fighter;
        private Mover mover;
        private NavMeshAgent agent;

        private Vector3 guardPosition;
        private Vector3 playerLastSeenPosition;
        private Quaternion startingRotation;
        private float timeSinceLastSawPlayer = Mathf.Infinity;
        private float timeSinceArrivedAtWaypoint = Mathf.Infinity;
        private int currentWaypointIndex = 0;
        private float suspiciousTime = 0;
        private bool attacking = false;
        private float distanceToWaypoint;
        private bool rotatingRight = true;

        private void Awake()
        {
            health = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
            mover = GetComponent<Mover>();
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            player = GameObject.FindWithTag("Player");
            guardPosition = transform.position;
            agent.speed = agentPatrolSpeed;
            startingRotation = transform.rotation;
        }
 
        private void Update()
        {
            if (IsAtPosition())
            {
                suspiciousTime -= Time.deltaTime;
            }

            // Rotate head depending on this boolean --> moving FoV from side to side
            if (lookAround)
            {
                LookAround();
            }

            if (health.IsDead())
            {
                fieldOfView.gameObject.SetActive(false);
                return;
            }

            if (fieldOfView.PlayerIsVisible() && !isLoading)
            {
                suspiciousTime = startingSuspiciousTime;
                playerLastSeenPosition = fieldOfView.GetPlayerLastSeenPosition();
                AttackBehaviour();
            }

            else if (!fieldOfView.PlayerIsVisible() && lookAround == false)
            {
                MoveTowardsPlayerLastKnownPosition();
            }

            else if (suspiciousTime <= 0)
            {
                PatrolBehaviour();
            }

            UpdateTimers();
        }       

        private void MoveTowardsPlayerLastKnownPosition()
        {
            agent.speed = agentAttackSpeed;
            head.localRotation = Quaternion.Euler(Vector3.zero);

            if (!IsAtPosition())
            {
                mover.StartMoveAction(playerLastSeenPosition);
            }
            if (IsAtPosition())
            {
                SuspicionBehaviour();
            }
        }

        private bool IsAtPosition()
        {
            float tollerance = 2f; // to ensure that enemy is not stuck because he cannot reach exact destination

            if (Vector3.Distance(transform.position, playerLastSeenPosition) < tollerance)
            {
                return true;
            }
            return false;
        }

        private void LookAround()
        {
            //   this version was easier but not compatible with saving system
            //   head.localRotation = Quaternion.Euler(0, maxRotation * Mathf.Sin(Time.time * speedOfRotation), 0);
            
            
            // Preventing angle to go from 0 to 359 - insted going to negative numbers
            float angle = head.localEulerAngles.y;
            angle = (angle > 180) ? angle - 360 : angle;

            if (rotatingRight)
            {
                if (angle <= maxRotation)
                {
                    head.Rotate(new Vector3(0, speedOfRotationInDegreesPerSecond * Time.deltaTime, 0), Space.Self);
                }
                else if (angle > maxRotation)
                {
                    rotatingRight = false;
                }
            }
            else if (!rotatingRight)
            {
                if (angle >= -maxRotation)
                {
                    head.Rotate(new Vector3(0, -speedOfRotationInDegreesPerSecond * Time.deltaTime, 0), Space.Self);
                }
                else if (angle < -maxRotation)
                {
                    rotatingRight = true;
                }
            }
        }

        private void UpdateTimers()
        {
            timeSinceArrivedAtWaypoint += Time.deltaTime;
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition;
            lookAround = true;
            agent.speed = agentPatrolSpeed;

            // for standing guards to rotate towards starting direction after leaving their post
            if (patrolPath == null && Vector3.Distance(transform.position, nextPosition) <= 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, startingRotation, speedOfRotation * Time.deltaTime);
            }

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceArrivedAtWaypoint = 0f;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }            

            if (timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                mover.StartMoveAction(nextPosition);
            }
        }

        private bool AtWaypoint()
        {
            distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            if (distanceToWaypoint < waypointTolerance)
            {
                return true;
            }
            return false;
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }
                
        private void SuspicionBehaviour()
        {
            lookAround = true;
        }

        private void AttackBehaviour()
        {
            lookAround = false;
            head.localRotation = Quaternion.Euler(Vector3.zero); // while chasing player looking forward towards him
            agent.speed = agentAttackSpeed;
            fighter.Attack(player);
        }       

        // saving system //

        [System.Serializable]
        struct AIControllerSaveData
        {
            public float suspiciousTime;
            public bool lookAround;
            public SerializableVector3 playerLastPosition;
            public SerializableVector3 headRotation;
            public int currentWaypointIndex;
            public float timeSinceArrivedAtWaypoint;
            public bool rotatingRight;
        }

        public object CaptureState()
        {
            AIControllerSaveData data = new AIControllerSaveData();
            data.suspiciousTime = suspiciousTime;
            data.lookAround = lookAround;
            data.playerLastPosition = new SerializableVector3(playerLastSeenPosition);
            data.headRotation = new SerializableVector3(head.localEulerAngles);
            data.currentWaypointIndex = currentWaypointIndex;
            data.timeSinceArrivedAtWaypoint = timeSinceArrivedAtWaypoint;
            data.rotatingRight = rotatingRight;


            return data;
        }

        public void RestoreState(object state)
        {
            AIControllerSaveData data = (AIControllerSaveData)state;
            suspiciousTime = data.suspiciousTime;
            isLoading = true;
            lookAround = data.lookAround;          
            playerLastSeenPosition = data.playerLastPosition.ToVector();
            head.localEulerAngles = data.headRotation.ToVector();
            currentWaypointIndex = data.currentWaypointIndex;
            timeSinceArrivedAtWaypoint = data.timeSinceArrivedAtWaypoint;
            rotatingRight = data.rotatingRight;

            StartCoroutine(IsLoading());
        }

        // fixing loading bug //
        // enemy refreshes new player position if he sees player in his FoV, this is fix for now to prevent such behaviour

        // How to reproduce this bug
        // Disable this WaitForSeconds(0.3f) delay - or just decrease it
        // Go into game, save game while you are hidden from enemy
        // Run into his FoV so he is running to you and press Load game while he sees you
        // He will run towards your loaded position even he should not
        // Way to fix is press Load again while out of his sight and he will act normal
        // I fixed it by disabling his ability to locate player for this 0.3f delay - for now it works fine

        // possible problem could be that player can findout about being unfindable during first 0.3 seconds after load and abuse it to avoid detection
        // I am not able to reproduce it yet but keeping it as a reminder here
        // possible solution would be just dont allow player to save right after loading for some time > 0.3f

        IEnumerator IsLoading()
        {
            yield return new WaitForSeconds(0.3f);
            isLoading = false;
        }
    }
}

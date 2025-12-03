using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

public enum NPCState { Idle, Walking }
public enum MovementMode { Default, Natural }

public class NPC_Controller : MonoBehaviour {
    [Header("References")]
    public NavMeshAgent agent;
    public NPC_PathHelper pathHelper;
    public Animator anim;
    public Transform _LookAtTarget;

    [Header("Movement Settings")]
    public MovementMode movementMode = MovementMode.Natural;
    public float _Speed = 1.6f;
    public float _TurnSpeed = 2.0f;

    [Range(0.1f, 1f)]
    public float minTurnSpeedMultiplier = 0.2f;

    [Header("Look At Settings")]
    public float lookSmoothTime = 0.5f;
    public float lookHeightOffset = 1.6f;

    [Header("Path Logic")]
    public NPCState _CurrentState = NPCState.Idle;
    public bool loopPath = true;
    public float reachDistance = 0.2f;

    [Header("Start Settings")]
    public bool startOnAwake = true;
    public float startOnAwakeDelay = 1f;

    [Header("Animation")]
    public float animDampTime = 0.1f;

    [Header("Events")]
    public UnityEvent OnPointReached;
    public UnityEvent OnPathComplete;

    private int _currentIndex = 0;
    private bool _isMoving = false;
    private int _animSpeedId;
    private bool _switchingPoint = false;

    private float _realtimeSpeed = 0f;

    private Vector3 _currentLookVelocity;

    private void Start() {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponent<Animator>();

        _animSpeedId = Animator.StringToHash("Speed");

        if (agent != null) {
            agent.speed = _Speed;
            agent.stoppingDistance = 0.02f;
            agent.autoBraking = false;

            if (movementMode == MovementMode.Natural) {
                agent.updateRotation = false;
                agent.acceleration = 999f;
            }
            else {
                agent.updateRotation = true;
                agent.angularSpeed = 999f;
                agent.acceleration = 999f;
            }
        }

        if (_LookAtTarget != null) {
            _LookAtTarget.position = transform.position + transform.forward + Vector3.up * lookHeightOffset;
        }

        _CurrentState = NPCState.Idle;

        if (startOnAwake) {
            StartCoroutine(StartPathRoutine());
        }
    }

    private void Update() {
        UpdateStateAndAnimation();
        HandleMovementLogic();
        HandleLookAtLogic();
        CheckPathProgress();
    }

    private void HandleLookAtLogic() {
        if (_LookAtTarget == null) return;

        Vector3 targetWorldPosition;

        if (_isMoving && pathHelper != null && pathHelper.GetPointCount() > 0) {
            Vector3 pointA = pathHelper.GetPointPosition(_currentIndex);

            int totalPoints = pathHelper.GetPointCount();
            int nextIndex = _currentIndex + 1;

            if (nextIndex >= totalPoints) {
                if (loopPath) {
                    nextIndex = 0;
                }
                else {
                    nextIndex = totalPoints - 1;
                }
            }
            Vector3 pointB = pathHelper.GetPointPosition(nextIndex);

            targetWorldPosition = Vector3.Lerp(pointA, pointB, 0.5f);
        }
        else {
            targetWorldPosition = transform.position + transform.forward * 3.0f;
        }

        targetWorldPosition.y += lookHeightOffset;

        _LookAtTarget.position = Vector3.SmoothDamp(
            _LookAtTarget.position,
            targetWorldPosition,
            ref _currentLookVelocity,
            lookSmoothTime
        );
        Debug.DrawLine(_LookAtTarget.position, targetWorldPosition, Color.red);
        if (agent.hasPath) Debug.DrawLine(transform.position, agent.steeringTarget, Color.green);
    }
    private void HandleMovementLogic() {
        if (!_isMoving || agent == null || _CurrentState == NPCState.Idle) {
            _realtimeSpeed = 0f;
            return;
        }

        if (movementMode == MovementMode.Default) {
            if (!agent.updateRotation) agent.updateRotation = true;
            agent.speed = _Speed;
            _realtimeSpeed = _Speed;
        }
        else if (movementMode == MovementMode.Natural) {
            if (agent.updateRotation) agent.updateRotation = false;

            if (agent.hasPath) {
                Vector3 nextPosition = agent.steeringTarget;
                Vector3 direction = (nextPosition - transform.position).normalized;
                direction.y = 0;

                float angle = 0f;
                if (direction != Vector3.zero) {
                    angle = Vector3.Angle(transform.forward, direction);
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _TurnSpeed * Time.deltaTime);
                }

                float turnFactor = 1f - Mathf.Clamp01(angle / 120f);
                float dynamicSpeedMultiplier = Mathf.Lerp(minTurnSpeedMultiplier, 1f, turnFactor);

                if (agent.remainingDistance > agent.stoppingDistance) {
                    _realtimeSpeed = _Speed * dynamicSpeedMultiplier;
                    agent.velocity = transform.forward * _realtimeSpeed;
                }
            }
        }
    }
    private void UpdateStateAndAnimation() {
        float targetAnimSpeed = 0f;
        if (_CurrentState == NPCState.Walking) {
            targetAnimSpeed = _realtimeSpeed;
        }
        if (anim != null) {
            anim.SetFloat(_animSpeedId, targetAnimSpeed, animDampTime, Time.deltaTime);
        }
    }
    private void CheckPathProgress() {
        if (!_isMoving || pathHelper == null || agent == null) return;
        if (_switchingPoint) return;
        if (agent.pathPending) return;

        if (agent.remainingDistance <= reachDistance) {
            HandlePointReached();
        }
    }
    IEnumerator StartPathRoutine() {
        _CurrentState = NPCState.Idle;
        if (startOnAwakeDelay > 0) yield return new WaitForSeconds(startOnAwakeDelay);
        StartPath();
    }
    public void StartPath() {
        if (pathHelper == null || pathHelper.GetPointCount() == 0) {
            Debug.LogWarning("NPC_Controller: No helper assigned.");
            return;
        }
        _currentIndex = 0;
        _isMoving = true;
        _switchingPoint = false;
        _CurrentState = NPCState.Walking;
        MoveToCurrentIndex();
    }

    public void StopPath() {
        _isMoving = false;
        _CurrentState = NPCState.Idle;
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
    }

    private void MoveToCurrentIndex() {
        Vector3 targetPos = pathHelper.GetPointPosition(_currentIndex);
        if (agent != null) agent.SetDestination(targetPos);
    }

    private void HandlePointReached() {
        _switchingPoint = true;
        OnPointReached?.Invoke();
        _currentIndex++;

        if (_currentIndex >= pathHelper.GetPointCount()) {
            if (loopPath) {
                _currentIndex = 0;
                MoveToCurrentIndex();
                StartCoroutine(ResetSwitchingFlag());
            }
            else {
                _isMoving = false;
                _CurrentState = NPCState.Idle;
                OnPathComplete?.Invoke();
            }
        }
        else {
            MoveToCurrentIndex();
            StartCoroutine(ResetSwitchingFlag());
        }
    }
   IEnumerator ResetSwitchingFlag() {
        yield return new WaitForEndOfFrame();
        yield return null;
        _switchingPoint = false;
    }
}
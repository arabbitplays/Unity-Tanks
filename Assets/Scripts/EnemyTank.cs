using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class EnemyTank : Tank
{
    public enum MovementType
    {
        STATIONARY,
        RANDOM,
        PLAYER_TARGET
    }

    public enum ShootingType
    {
        RANDOM,
        PLAYER_TARGET
    }

    public enum FireRate
    {
        NONE,
        SLOW,
        FAST
    }


    [Header("AI")]
    [SerializeField] private MovementType movementType;
    [SerializeField] private ShootingType shootingType;
    [SerializeField] private FireRate fireRate;
    private int shootingChance;
    [SerializeField] private LayerMaskReference scanLayers;
    private int bulletBounces;
    
    private Transform playerTransform;
    private Seeker seeker;
    private Path path = null;
    private List<Vector3> waypoints;
    [SerializeField] private float waypointDistance, maxDistToPlayer, innerTargetDist, outerTargetDist;

    [SerializeField, Range(0f, 2f)] private float distToWalls;

    private List<Vector2> bouncePoints;

    protected override void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        seeker = GetComponent<Seeker>();
        switch (fireRate)
        {
            case FireRate.NONE:
                shootingChance = 0;
                break;
            case FireRate.SLOW:
                shootingChance = 30;
                break;
            case FireRate.FAST:
                shootingChance = 70;
                break;
        }
        base.Awake();
    }

    protected void Start()
    {
        StartCoroutine(ShootingRoutine()); 
        StartCoroutine(MovementRoutine());
        bulletBounces = bulletPrefab.GetComponent<Bullet>().GetBounceCount();
    }

    private IEnumerator ShootingRoutine()
    {
        while(true)
        {
            bool tryShooting = true;

            // ------------ head rotation ----------
            float rotationAngle = 0;
            switch (shootingType)
            {
                case ShootingType.RANDOM:
                    rotationAngle = PickRandomAngle();
                    break;
                case ShootingType.PLAYER_TARGET:
                    Vector2 playerHitDirection = FindPlayerHitDirection();
                    if (playerHitDirection == Vector2.zero)
                    {
                        rotationAngle = PickRandomAngle();
                        tryShooting = false;
                    } else
                    {
                        rotationAngle = Vector2.SignedAngle(currHeadDirection, playerHitDirection);
                    }
                    break;
            }

            float deltaAngle = headTurningSpeed * Time.fixedDeltaTime;
            while (Mathf.Abs(rotationAngle) > 1) {
                float angle = rotationAngle > 0 ? Mathf.Min(rotationAngle, deltaAngle) : Mathf.Max(rotationAngle, -deltaAngle);
                rotationAngle -= angle;
                TurnHead(angle);
                yield return new WaitForFixedUpdate();
            }

            // -------------- shooting ---------------
            if (tryShooting)
            {
                int rand = Random.Range(0, 100);
                if (rand < shootingChance)
                {
                    Shoot();
                }
            }

            yield return new WaitForSeconds(1);
        }
    }

    private float PickRandomAngle()
    {
        return Random.Range(-180, 180);
    }

    private Vector2 FindPlayerHitDirection()
    {
        int maxBouncesLeft = -1;
        Vector2 currBestDir = Vector2.zero;
        List<Vector2> bouncePointBuffer = null;
        for (float angle = -180; angle < 180; angle += 1)
        {
            bouncePoints = new List<Vector2>();
            bouncePoints.Add(transform.position);

            Vector2 dir = Rotate(Vector2.right, angle);
            int bouncesLeft = TraceBulletPath(transform.position, dir, bulletBounces);
            if (bouncesLeft > maxBouncesLeft)
            {
                maxBouncesLeft = bouncesLeft;
                currBestDir = dir;
                bouncePointBuffer = bouncePoints;
            }

            // exit search if a direct path to the player is found
            if (maxBouncesLeft == bulletBounces) 
                return currBestDir;
        }

        bouncePoints = bouncePointBuffer;
        return currBestDir;
    }

    private int TraceBulletPath(Vector2 startPoint, Vector2 dir, int bounces)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPoint, dir, Mathf.Infinity, scanLayers.value);
        if (hits.Length == 1)
            return -1;

        RaycastHit2D closestHit = hits[0];
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].distance == 0)
                continue;

            if (hits[i].distance < minDistance)
            {
                minDistance = hits[i].distance;
                closestHit = hits[i];
            }
        }

        bouncePoints.Add(closestHit.point);

        if (closestHit.collider.gameObject.tag == "Player")
        {
            return bounces;
        }

        int hitLayer = closestHit.collider.gameObject.layer;
        if (bounces > 0 && wallLayer.value == (wallLayer.value | (1 << hitLayer)))
        {
            return TraceBulletPath(closestHit.point, Vector2.Reflect(dir, closestHit.normal), bounces - 1);
        }

        return -1;
    }

    private IEnumerator MovementRoutine()
    {
        while (true)
        {
            path = null;

            Vector2 target = Vector2.zero;
            switch (movementType)
            {
                case MovementType.STATIONARY:
                    yield break;
                case MovementType.RANDOM:
                    target = RandomUtil.GetRandomPositionInBox(Vector2.zero, gameManager.GetCurrLevelBoundingBox(), wallLayer.value, distToWalls);
                    seeker.StartPath(transform.position, target, OnPathComplete);
                    break;
                case MovementType.PLAYER_TARGET:
                    target = RandomUtil.GetRandomPositionInDonut(playerTransform.position, innerTargetDist, outerTargetDist, wallLayer.value, distToWalls);
                    seeker.StartPath(transform.position, target, OnPathComplete);
                    break;
            }

            while (path == null)
            {
                yield return new WaitForFixedUpdate();
            }

            yield return FollowPath();
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
        }
        else
        {
            Debug.LogError(p.errorLog);
        }
    }

    private IEnumerator FollowPath()
    {
        waypoints = path.vectorPath;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector2 dir = waypoints[i] - transform.position;

            float deltaAngle = turningSpeed * Time.fixedDeltaTime;
            while (dir.magnitude > waypointDistance)
            {
                float angleToTarget = Vector2.SignedAngle(currDirection, dir);

                if (Mathf.Abs(angleToTarget) > 1)
                {
                    float angle = angleToTarget > 0 ? Mathf.Min(angleToTarget, deltaAngle) : Mathf.Max(angleToTarget, -deltaAngle);
                    Turn(angle);
                }

                if (Mathf.Abs(angleToTarget) < 60)
                {
                    Move(true);
                }

                dir = waypoints[i] - transform.position;
                yield return new WaitForFixedUpdate();
            }
        }
    }

    public override void Destroy()
    {
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, waypointDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistToPlayer);

        Gizmos.DrawLine(transform.position, transform.position + (Vector3)currDirection);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)currHeadDirection);

        if (waypoints != null)
        {
            for (int i = 1; i < waypoints.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
            }
        }


        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < 100; i++)
            {
                Vector2 target = RandomUtil.GetRandomPositionInDonut(playerTransform.position, innerTargetDist, outerTargetDist, wallLayer.value, distToWalls);
                Gizmos.DrawSphere(target, 0.1f);
            }
        }

        if (bouncePoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 1; i < bouncePoints.Count; i++)
            {
                Gizmos.DrawLine(bouncePoints[i - 1], bouncePoints[i]);
            }
        }
    }
}

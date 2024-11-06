using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tank : MonoBehaviour
{
    protected GameManager gameManager;

    [SerializeField] protected GameObject head;
    [SerializeField] protected Transform shootingPoint;
    [SerializeField] protected GameObject bulletPrefab;
    private List<Bullet> availabileBullets;
    [SerializeField] protected float speed, turningSpeed, headTurningSpeed;
    [SerializeField] protected int bulletCount;

    [SerializeField] protected LayerMaskReference wallLayer;

    protected Vector2 currDirection = Vector2.right;
    protected Vector2 currHeadDirection = Vector2.right;

    protected virtual void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        availabileBullets = new List<Bullet>();
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            Bullet bullet = obj.GetComponent<Bullet>();
            bullet.InitBullet(BulletHitCallback);
        }
    }

    protected virtual void Update()
    {

        // update head rotation
        float angle = Vector2.Angle(currHeadDirection, Vector2.right);
        angle = currHeadDirection.y > 0 ? angle : -angle;
        head.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void BulletHitCallback(Bullet bullet)
    {
        availabileBullets.Add(bullet);
    }

    protected void Move(bool forward)
    {
        Vector2 dir = forward ? currDirection : -currDirection;
        transform.position += (Vector3)dir.normalized * speed * Time.fixedDeltaTime;
    }

    protected void Shoot()
    {
        if (availabileBullets.Count == 0 || Physics2D.OverlapCircle(shootingPoint.position, 0, wallLayer.value) != null)
            return;
        Bullet bullet = availabileBullets[0];
        availabileBullets.Remove(bullet);
        bullet.Activate(shootingPoint.position, currHeadDirection);
    }

    protected void Turn(float angle)
    {
        currDirection = Rotate(currDirection, angle);

        angle = Vector2.Angle(currDirection, Vector2.right);
        angle = currDirection.y > 0 ? angle : -angle;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    protected void TurnHead(float angle)
    {
        currHeadDirection = Rotate(currHeadDirection, angle);
    }

    public abstract void Destroy();

    protected static Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}

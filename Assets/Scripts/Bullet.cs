using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;

    private Vector2 currDirection;
    private bool isActive;

    public delegate void HitCallback(Bullet bullet);
    HitCallback hitCallback;

    [SerializeField] private Transform tipTransform;

    [SerializeField] private LayerMaskReference destroyLayers, bounceLayers, tankLayer;

    [SerializeField] private int bounceCount;
    private int bouncesLeft;

    bool processingBounce = false;

    [SerializeField] private GameObject smallExplosionPrefab, bigExplosionPrefab;

    public void InitBullet(HitCallback callback)
    {
        hitCallback = callback;
        Deactivate();
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            Vector2 deltaPosition = currDirection * speed * Time.fixedDeltaTime;
            transform.position += (Vector3)deltaPosition;
            CheckForCollisions();
        }
    }

    private void CheckForCollisions()
    {
        if (processingBounce)
            return;

        if (bouncesLeft > 0 && Physics2D.OverlapCircle(tipTransform.position, 0, bounceLayers.value) != null)
        {
            StartCoroutine(ProcessBounce());
            return;
        }

        Collider2D hitCollider = Physics2D.OverlapCircle(tipTransform.position, 0, destroyLayers.value);
        if (hitCollider != null)
        {
            ProcessHit(hitCollider);
            Deactivate();
        }
    }

    private void ProcessHit(Collider2D collider)
    {
        if (tankLayer.value == (tankLayer.value | (1 << collider.gameObject.layer)))
        {
            Instantiate(bigExplosionPrefab, collider.transform.position, Quaternion.identity);
            collider.gameObject.GetComponent<Tank>().Destroy();
        } else
        {
            Instantiate(smallExplosionPrefab, transform.position, Quaternion.identity);
            if (collider.gameObject.layer == gameObject.layer)
            {
                collider.gameObject.GetComponent<Bullet>().Deactivate();
            }
        }
    }


    public void Activate(Vector2 spawnPosition, Vector2 dir)
    {
        transform.position = spawnPosition;
        currDirection = dir;
        bouncesLeft = bounceCount;

        UpdateRotation();

        gameObject.SetActive(true);
        isActive = true;
    }

    public void Deactivate()
    {
        hitCallback(this);
        isActive = false;
        gameObject.SetActive(false);
    }

    private IEnumerator ProcessBounce()
    {
        processingBounce = true;

        bouncesLeft--;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, currDirection, Mathf.Infinity, bounceLayers.value);
        currDirection = Vector2.Reflect(currDirection, hit.normal);
        UpdateRotation();

        yield return new WaitForSeconds(Time.fixedDeltaTime);
        processingBounce = false;
    }

    private void UpdateRotation()
    {
        float angle = Vector2.Angle(currDirection, Vector2.right);
        angle = currDirection.y > 0 ? angle : -angle;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public int GetBounceCount()
    {
        return bounceCount;
    }
}

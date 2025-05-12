using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GunController : MonoBehaviour
{
    [Header("References")]
    private Camera _mainCamera;
    private Animator _animator;
    
    [Header("Shooting")]
    [SerializeField] private GameObject firePoint;
    [SerializeField] private float damagePerBullet = 1f;
    [SerializeField] private float shootMaxDelay = 0.5f;
    [SerializeField] private float trailDuration = 0.5f;
    [SerializeField] private Vector3 bulletSpreadVariance;
    private float _shootCurrentDelay;
    
    private InputAction _shootAction;

    private void Start()
    {
        _shootAction = InputSystem.actions.FindAction("Shoot");
        _mainCamera = Camera.main;
        _animator = GetComponent<Animator>();
        
        if (_mainCamera) 
            transform.LookAt(_mainCamera.transform.position + _mainCamera.transform.TransformDirection(Vector3.forward) * 100f);
    }
    public void Update()
    {
        if (_shootAction.IsPressed() && _shootCurrentDelay <= 0)
        {
            Shoot();
        }
    }
    private void FixedUpdate()
    {
        if(_shootCurrentDelay > 0)
            _shootCurrentDelay -= Time.fixedDeltaTime;
    }

    private void Shoot()
    {
        if(_shootCurrentDelay > 0)
            return;
        
        _animator.Play("ShootingAnim", 0, 0f);
        _shootCurrentDelay = shootMaxDelay;
        
        var ray = new Ray(_mainCamera.transform.position, GetShootingDirection());
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            var hitObject = hit.transform.gameObject;
            DealDamage(hitObject);
            StartCoroutine(SpawnBulletTrail(hit.point));
        }
        else
        {
            StartCoroutine(SpawnBulletTrail(ray.origin + ray.direction * 50f));
        }
    }
    private Vector3 GetShootingDirection()
    {
        var direction = _mainCamera.transform.TransformDirection(Vector3.forward);
        direction += new Vector3(
            Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x), 
            Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y), 
            Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.z));
        
        return direction;
    }

    private GameObject SpawnBullet()
    {
        var bullet = BulletPool.Instance.GetBullet();
        bullet.transform.position = firePoint.transform.position;
        return bullet;
    }
    private void DespawnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        BulletPool.Instance.ReturnBullet(bullet);
    }
    
    private IEnumerator SpawnBulletTrail(Vector3 goalPosition)
    {
        float time = 0f;
        var bullet = SpawnBullet();
        bullet.transform.LookAt(goalPosition);
        bullet.SetActive(true);

        Vector3 start = firePoint.transform.position;
    
        while (time < trailDuration)
        {
            bullet.transform.position = Vector3.Lerp(start, goalPosition, time / trailDuration);
            time += Time.deltaTime;
            yield return null;
        }

        bullet.transform.position = goalPosition;

        // If you have a HitFX on the bullet
        if (bullet.TryGetComponent(out ParticleSystem hitFx))
        {
            hitFx.Play();
            yield return new WaitWhile(() => hitFx.isPlaying);
        }

        DespawnBullet(bullet);
    }
    
    private void DealDamage(GameObject hitObject)
    {
        if (hitObject && hitObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damagePerBullet);
        }
    }
}

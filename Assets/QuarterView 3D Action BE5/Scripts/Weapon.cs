using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum EType { Melee, Range };
    public EType type;
    public float rate;

    [Header("Melee")]
    public int damage;
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    [Header("Range")]
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;
    public int curAmmo;
    public int maxAmmo;

    public void Use()
    {
        if (type == EType.Melee)
        {
            StopCoroutine(Swing());
            StartCoroutine(Swing());
        }

        else if (type == EType.Range)
        {
            if (curAmmo <= 0)
            {
                return;
            }

            curAmmo--;
            StopCoroutine(nameof(Shot));
            StartCoroutine(nameof(Shot));
        }
    }

    private IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        trailEffect.emitting = true;

        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(0.05f);
        meleeArea.enabled = false;
        trailEffect.emitting = false;

        yield break;
    }

    private IEnumerator Shot()
    {
        // 총알 발사
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;

        // 탄피 배출
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-5f, -2f) + Vector3.up * Random.Range(2f, 3f);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
        yield return null;
    }
}

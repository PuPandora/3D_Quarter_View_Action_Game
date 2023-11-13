using System.Collections;
using UnityEngine;

public class BossRock : Bullet
{
    Rigidbody rigid;
    private float angularPower = 2;
    private float scaleValue = 0.1f;
    private bool isShoot;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
    }

    private IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(2.2f);
        isShoot = true;
    }

    private IEnumerator GainPower()
    {
        while (!isShoot)
        {
            angularPower += 0.02f;
            scaleValue += 0.005f;
            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);

            yield return null;
        }
    }
}

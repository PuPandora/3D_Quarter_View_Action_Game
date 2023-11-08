using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rigid;

    void Start()
    {
        StartCoroutine(Explosion());
    }

    private IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        meshObj.SetActive(false);
        effectObj.SetActive(true);

        // 원형의 레이캐스트 15범위 안에 있는
        // Enemy 레이어 오브젝트들을 탐색함
        RaycastHit[] rayHits = Physics.SphereCastAll(
                                                    transform.position,         // 시작 지점
                                                    15,                         // 범위
                                                    Vector3.up,                 // 방향
                                                    0f,                         // 거리
                                                    LayerMask.GetMask("Enemy") // 탐색 레이어
                                                    );

        foreach (RaycastHit hitObj in rayHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }
    }
}

using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum EType { Melee, Range };
    public EType type;
    public int damage;
    public float rate;
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public void Use()
    {
        if (type == EType.Melee)
        {
            StartCoroutine(nameof(Swing));
        }
    }

    private IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        trailEffect.emitting = true;

        yield return new WaitForSeconds(0.15f);
        meleeArea.enabled = false;
        trailEffect.emitting = false;

        yield break;
    }
}

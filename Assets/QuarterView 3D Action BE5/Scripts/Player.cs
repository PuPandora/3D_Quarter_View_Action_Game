using UnityEngine;

public class Player : MonoBehaviour
{
    // Move
    public float speed = 15f;
    private float hAxis;
    private float vAxis;
    private Vector3 moveVec;

    // Dodge
    private Vector3 dodgeVec;

    // Jump
    public float jumpPower = 15f;

    // Player Status
    public int ammo;
    public int coin;
    public int health = 100;
    public int hasGrenades;
    [Space(10f)]
    public int maxAmmo = 999;
    public int maxCoin = 99999;
    public int maxHealth = 100;
    public int maxHasGrenades = 4;

    // Item
    [Space(20f)]
    private GameObject nearObject;
    public GameObject[] weapons;
    public GameObject[] grenades;
    private Weapon equipWeapon;
    private int equipWeaponIndex = -1;
    public bool[] hasWeapons;

    // Attack
    private float fireDelay;

    // Input
    private bool vDown; // Walk
    private bool jDown; // Jump
    private bool iDown; // Interaction
    private bool sDown1; // Swap 1
    private bool sDown2; // Swap 2
    private bool sDown3; // Swap 3
    private bool fDown; // Fire

    // State
    private bool isDodge;
    private bool isJump;
    private bool isSwap;
    private bool isFireReady = true;

    // Component
    Animator anim;
    Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Dodge();
        Swap();
        Interaction();
    }

    private void GetInput()
    {
        // Move
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        vDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        // Interaction
        iDown = Input.GetButtonDown("Interaction");
        // Swap
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        // Attack
        fDown = Input.GetButtonDown("Fire1");
    }

    private void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        // 교체 OR 공격 중 이동 불가
        if (isSwap || !isFireReady)
        {
            moveVec = Vector3.zero;
        }

        transform.position += moveVec * speed * (vDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", vDown);
    }

    private void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    private void Jump()
    {
        if (isJump || isDodge)
        {
            return;
        }

        // 멈춰있을 때 스페이스바
        if (jDown && moveVec == Vector3.zero)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    private void Attack()
    {
        if (equipWeapon == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate <= fireDelay;

        // 회피, 교체 중에는 공격 불가
        if (isDodge && isSwap)
        {
            return;
        }

        // 공격 키를 누르고 시간이 됐을 때
        if (fDown && isFireReady)
        {
            equipWeapon.Use();
            anim.SetTrigger("doSwing");
            fireDelay = 0;
        }
    }

    private void Dodge()
    {
        if (isJump || isDodge)
        {
            return;
        }

        // 움직일 때 스페이스바
        if (jDown && moveVec != Vector3.zero)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke(nameof(DodgeOut), 0.4f);
        }
    }

    private void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    private void Swap()
    {
        // 해당 무기가 없거나 이미 장착 중이라면 return
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) return;
        else if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) return;
        else if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) return;

        if (isJump || isDodge)
        {
            return;
        }

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        else if (sDown2) weaponIndex = 1;
        else if (sDown3) weaponIndex = 2;

        // 누른 키에 맞게 무기 교체
        if (sDown1 || sDown2 || sDown3)
        {
            if (equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);
            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;

            Invoke(nameof(SwapOut), 0.5f);
        }
    }

    private void SwapOut()
    {
        isSwap = false;
    }

    private void Interaction()
    {
        if (isJump || isDodge)
        {
            return;
        }

        // 근처 아이템 획득
        if (iDown && nearObject != null)
        {
            // 무기
            if (nearObject.CompareTag("Weapon"))
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 아이템 획득
        if (other.CompareTag("Item"))
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.EType.Ammo:
                    ammo += item.value;

                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;

                case Item.EType.Coin:
                    coin += item.value;

                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;

                case Item.EType.Grenade:
                    if (hasGrenades >= maxHasGrenades)
                    {
                        hasGrenades = maxHasGrenades;
                        return;
                    }
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    break;

                case Item.EType.Heart:
                    health += item.value;

                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            nearObject = other.gameObject;
            Debug.Log(nearObject);
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            nearObject = null;
        }
    }
}

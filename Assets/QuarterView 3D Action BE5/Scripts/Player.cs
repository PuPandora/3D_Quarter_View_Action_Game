using UnityEngine;

public class Player : MonoBehaviour
{
    // Important
    Animator anim;
    Rigidbody rigid;
    public Camera followCamera;

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
    public GameObject grenadeObj;
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
    private bool rDown; // Relaod
    private bool gDown; // Grenade

    // State
    private bool isDodge;
    private bool isJump;
    private bool isSwap;
    private bool isFireReady = true;
    private bool isReload;
    private bool isBorder;

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
        Grenade();
        Attack();
        Reload();
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
        // Battle
        fDown = Input.GetButton("Fire1");
        rDown = Input.GetButtonDown("Reload");
        gDown = Input.GetButtonDown("Fire2");
    }

    private void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        // 교체, 공격, 재장전 중 이동 불가
        if (isSwap || isReload || !isFireReady) 
        {
            moveVec = Vector3.zero;
        }

        // 앞에 벽이 있다면 이동 제한
        if (!isBorder)
        {
            transform.position += moveVec * speed * (vDown ? 0.3f : 1f) * Time.deltaTime;
        }

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", vDown);
    }

    private void Turn()
    {
        // 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        // 마우스에 의한 회전
        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;

        if (fDown)
        {
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
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

    private void Grenade()
    {
        if (hasGrenades <= 0)
        {
            return;
        }

        // 수류탄 사용이 가능한 상황
        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                // 수류탄 투척
                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
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

        // 회피, 교체, 재장전 중에는 공격 불가
        if (isDodge || isSwap || isReload)
        {
            return;
        }

        // 공격 키를 누르고 시간이 됐을 때
        if (fDown && isFireReady)
        {
            equipWeapon.Use();
            // 근접 무기면 doSwing, 원거리 무기면 doShot
            anim.SetTrigger(equipWeapon.type == Weapon.EType.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    private void Reload()
    {
        if (equipWeapon == null)
        {
            return;
        }

        if (equipWeapon.type == Weapon.EType.Melee)
        {
            return;
        }

        if (isJump || isDodge || isSwap || !isFireReady || isReload)
        {
            return;
        }

        if (ammo <= 0)
        {
            return;
        }

        if (rDown)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke(nameof(ReloadOut), 0.5f);
        }
    }

    private void ReloadOut()
    {
        // 탄약 소모
        // 소지 중인 탄약 총의 최대 탄약 수 보다 적다면 그만큼만 탄약 충전
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;

        isReload = false;
    }

    private void Dodge()
    {
        // 점프, 회피, 재장전 중에는 회피 불가
        if (isJump || isDodge || isReload)
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
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) return;

        // 점프, 회피, 재장전 중에는 교체 불가
        if (isJump || isDodge || isReload)
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

    private void FreezeRotation()
    {
        // 회전 방지
        rigid.angularVelocity = Vector3.zero;
    }

    private void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
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

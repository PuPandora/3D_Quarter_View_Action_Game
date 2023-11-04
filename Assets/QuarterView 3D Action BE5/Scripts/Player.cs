using UnityEngine;

public class Player : MonoBehaviour
{
    // Move
    public float speed = 15f;
    private float hAxis;
    private float vAxis;
    private bool vDown;
    private Vector3 moveVec;

    // Jump
    private bool jDown;
    private float jumpPower = 15f;
    private bool isJump;

    // Dodge
    private bool isDodge;
    private Vector3 dodgeVec;

    // Item
    private GameObject nearObject;
    private bool iDown;
    public GameObject[] weapons;
    private GameObject equipWeapon;
    public bool[] hasWeapons;
    private bool sDown1;
    private bool sDown2;
    private bool sDown3;
    private bool isSwap;
    private int equipWeaponIndex = -1;

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
    }

    private void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        // 교체 중 이동 불가
        if (isSwap)
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
                equipWeapon.SetActive(false);
            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);

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

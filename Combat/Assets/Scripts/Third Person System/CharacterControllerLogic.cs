using UnityEngine;
using System.Collections;

public class CharacterControllerLogic : MonoBehaviour
{
    public enum CharacterState
    {
        Idle, Running, Walking, Sprinting, 
        WalkBackwards, Turning, 
        Jumping, Falling, Landing, Sliding, Using, 
        Dead, Talking, TalkingLastLine
    }

    public static CharacterControllerLogic Instance;

    [SerializeField]
    private CharacterState _state = CharacterState.Idle;
    [SerializeField]
    private ThirdPersonCamera _gameCam;
	[SerializeField]
	private Animator _animator;
    [SerializeField]
    private CapsuleCollider _capCollider;
    [SerializeField]
    private bool _isGrounded;
    //[SerializeField]
    //private float _directionDampTime = 0.25f;
	[SerializeField]
	private float _directionSpeed = 4.0f;
	[SerializeField]
	private float _rotationDegreePerSecond = 120f;
	[SerializeField]
	private float _speedDampTime = 0.25f;
    [SerializeField]    
    private float _fovDampTime = 3f;
    [SerializeField]
    private float _jumpMultiplier = 1f;

    [SerializeField]
    private float _jumpDist = 1f;
    [SerializeField]
    private float _slideThreshold = 40f;
    //[System.NonSerialized]
    //private float groundNormal = 0.0f;

	private float _speed = 0.0f;
	private float _direction = 0f;
    private float _charAngle = 0f;
	private float _horizontal = 0.0f;
	private float _vertical = 0.0f;
 //   private float _floorRayDistance = 0.0f;
    private float _fallHeightStart = 5f;
    private float _heightAtFallStart = 0f;
    private float _fatalFallHeight = 24f;
    private const float SPRINT_SPEED = 3.0f;
    private const float SPRINT_FOV = 75.0f;
    private const float NORMAL_FOV = 60.0f;
    private float _capsuleHeight; 
	private AnimatorStateInfo _stateInfo;
    private AnimatorTransitionInfo _transInfo;
    private Vector3 _slideDirection;
//    private Vector3 lastGroundNormal = Vector3.zero;
    private GameObject _initialPosition;
    private GameObject _ragdoll;

    private int _m_IdlePivotLId = 0;
    private int _m_IdlePivotRId = 0;
	private int _m_LocomotionId = 0;
    private int _m_LocomotionPivotLId = 0;
    private int _m_LocomotionPivotRId = 0;
    private int _m_LocomotionPivotLTransId = 0;
    private int _m_LocomotionPivotRTransId = 0;

    public CharacterState State
    {
        get
        {
            return this._state;
        }
    }
    public Animator Animator
    {
        get
        {
            return this._animator;
        }
    }

    public bool IsGrounded
    {
        get
        {
            return this._isGrounded;
        }
    }

    public float Speed
    {
        get
        {
            return this._speed;
        }
    }

    public float LocomotionThreshold { get { return 0.2f; } }

	void Start () 
	{
        Instance = this;
		_animator = this.GetComponent<Animator>();
        _capCollider = this.GetComponent<CapsuleCollider>();
        _capsuleHeight = _capCollider.height;

		if (_animator.layerCount >= 2) 
		{
			_animator.SetLayerWeight(1, 1);
		}

		//hash all animation names for performance
        _m_IdlePivotLId = Animator.StringToHash("Base Layer.IdlePivotL");
        _m_IdlePivotRId = Animator.StringToHash("Base Layer.IdlePivotR");
        _m_LocomotionId = Animator.StringToHash("Base Layer.Locomotion");
        _m_LocomotionPivotLId = Animator.StringToHash("Base Layer.LocomotionPivotL");
        _m_LocomotionPivotRId = Animator.StringToHash("Base Layer.LocomotionPivotR");
        _m_LocomotionPivotLTransId = Animator.StringToHash("Base Layer.Locomotion -> Base Layer.LocomotionPivotL");
        _m_LocomotionPivotRTransId = Animator.StringToHash("Base Layer.Locomotion -> Base Layer.LocomotionPivotR");

        GameObject initialPosition = new GameObject("Player spawnpoint");
        initialPosition.transform.position = transform.position;
        initialPosition.transform.rotation = transform.rotation;
        _initialPosition = initialPosition;

	}
	
	// Update is called once per frame
	void Update () 
	{
        if (_state == CharacterState.Dead)
        {
            if (Input.anyKeyDown)
                Reset();
            return;
        }

        //check for falling
        _isGrounded = IsGroundedTest();

        if(_isGrounded)
            CheckForSliding();

        if(Input.GetKey(KeyCode.P))
        {
            Die();
        }

        if (_state == CharacterState.Talking || _state == CharacterState.TalkingLastLine)
            return;

		if(_animator && _gameCam.CamState != ThirdPersonCamera.CamStates.FirstPerson)
		{
            _stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            _transInfo = _animator.GetAnimatorTransitionInfo(0);

            //JUMP
            if (Input.GetButton("Jump"))
            {
                _animator.SetBool("Jump", true);
                _state = CharacterState.Jumping;
            }
            else
            {
                _animator.SetBool("Jump", false);
            }

			//Pull values from controller/keybord
			_horizontal = Input.GetAxis("Horizontal");
			_vertical = Input.GetAxis("Vertical");
            if (!CanWalkForward())
                if(_vertical == 1)
                    _vertical = 0;

            _charAngle = 0f;
            _direction = 0f;
            float charSpeed = 0f;

			//translate controls stick coordinates into world/cam/character space
            StickToWorldSpace(this.transform, _gameCam.transform, ref _direction, ref charSpeed, ref _charAngle, IsInPivot());

            if(Input.GetKey(KeyCode.S) && _gameCam.CamState == ThirdPersonCamera.CamStates.Target)
            {
                Animator.SetBool("Backwards", true);
                _state = CharacterState.WalkBackwards;
            }
            else
                Animator.SetBool("Backwards", false);

            //SPRINT
            if (Input.GetKey(KeyCode.RightShift) && _state != CharacterState.WalkBackwards && _state != CharacterState.Sprinting)
            {
                _speed = Mathf.Lerp(_speed, SPRINT_SPEED, Time.deltaTime);
                _gameCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_gameCam.GetComponent<Camera>().fieldOfView, SPRINT_FOV, _fovDampTime * Time.deltaTime);
                _state = CharacterState.Sprinting;

            }
            else
            {
                _speed = charSpeed;
                _gameCam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(_gameCam.GetComponent<Camera>().fieldOfView, NORMAL_FOV, _fovDampTime * Time.deltaTime);
            }

			_animator.SetFloat("Speed", _speed, _speedDampTime, Time.deltaTime);
	//		_animator.SetFloat("Direction", _direction, _directionDampTime, Time.deltaTime);

            if (_speed > LocomotionThreshold)
            {
                if (!IsInPivot())
                {
                    Animator.SetFloat("TurningAngle", _charAngle);
                }

                if (_state != CharacterState.Sprinting && _state != CharacterState.WalkBackwards && !IsInIdleTurning())
                {
         //           Debug.LogWarning(Animator.GetCurrentAnimatorStateInfo(0).IsName("IdlePivotL"));
                    _state = CharacterState.Running;
                }
            }
            else if (_speed < LocomotionThreshold && Mathf.Abs(_horizontal) < 0.02f)
            {
                Animator.SetFloat("Direction", 0f);
                Animator.SetFloat("TurningAngle", 0f);
                _state = CharacterState.Idle;
            }          
            if (IsInIdleTurning())
            {
             //   Debug.LogWarning("we are turning left!");
                _state = CharacterState.Turning;
            }
            
		}

     //   Debug.Log(_state);
	}

    private bool CanWalkForward()
    {
        RaycastHit hitInfo;
        Debug.DrawRay(transform.position, transform.forward, Color.yellow);

        Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z), transform.forward, out hitInfo, 1.0f);
      //  Debug.Log(Vector3.Dot(Vector3.up, hitInfo.normal) + " kan hij doorlopen? " + (Vector3.Dot(Vector3.up, hitInfo.normal) > 0.60f));   //45 maximum hoek
        if (Vector3.Dot(Vector3.up, hitInfo.normal) > 0.65f || Vector3.Dot(Vector3.up, hitInfo.normal) <= 0)
        {
        //    Debug.Log(Vector3.Dot(Vector3.up, hitInfo.normal) + "pass");
            return true;
        }
        else
        //    Debug.LogWarning(Vector3.Dot(Vector3.up, hitInfo.normal) + "stop");
            return false;
    }

    private void CheckForSliding()
    {
     //   Debug.DrawRay(transform.position, Vector3.down, Color.yellow);
        _slideDirection = Vector3.zero;
        RaycastHit rayHit;
        if (Physics.Raycast(transform.position, Vector3.down, out rayHit))
        {
          //  Debug.Log(myAng);

            if (Vector3.Angle(rayHit.normal, Vector3.up) > _slideThreshold && _isGrounded)
            {
                Animator.SetBool("Slide", true);
                _state = CharacterState.Sliding;
                Vector3 hit = rayHit.normal;
                _slideDirection = new Vector3(rayHit.normal.x, -rayHit.normal.y, rayHit.normal.z);
                Vector3.OrthoNormalize(ref hit, ref _slideDirection);
                Vector3 moveVector = _slideDirection;


                transform.position = transform.position + moveVector * Time.deltaTime * 10f;
           

                Debug.DrawRay(transform.position, transform.forward, Color.red);
                Debug.DrawRay(transform.position, _slideDirection, Color.white);

            }
            else
            {
                Animator.SetBool("Slide", false);
            }
        }
    }

    void FixedUpdate()
    {
        //rotate the character model if stick is tilted right or left, but only if the character is moving in that direction
        if (IsInLocomotion() && _gameCam.CamState != ThirdPersonCamera.CamStates.Free && !IsInPivot() && ((_direction >= 0 && _horizontal >= 0) || (_direction < 0 && _horizontal < 0)))
        {
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, _rotationDegreePerSecond * (_horizontal < 0f ? -1f : 1f), 0f),
                                                  Mathf.Abs(_horizontal));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
            this.transform.rotation = (this.transform.rotation * deltaRotation);
        }

        if (IsInJump())
        {
            float oldY = transform.position.y;
            transform.Translate(Vector3.up * _jumpMultiplier * _animator.GetFloat("JumpCurve"));    
            if (IsInLocomotionJump())
            {
                transform.Translate(Vector3.forward * Time.deltaTime * _jumpDist);
            }
            _capCollider.height = _capsuleHeight + (_animator.GetFloat("CapsuleCurve") * 0.5f);    
            if (_gameCam.CamState != ThirdPersonCamera.CamStates.Free)
            {
                _gameCam.CameraXform.Translate(Vector3.up * (transform.position.y - oldY));
            }
        }
    }

    private bool IsGroundedTest()
    {
        Debug.DrawRay(transform.position, Vector3.down, Color.cyan);
        float distanceToGround = 0f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 150f))
        {
            distanceToGround = hit.distance;
        }
        // Debug.Log("Distance to the ground is: " + distanceToGround);
        if (distanceToGround > .2f)
        {
            _isGrounded = false;
            if(_animator.GetBool("Falling")) // we are falling
            {
                
                transform.position = transform.position - new Vector3(0, 10f, 0) * Time.deltaTime;
            }
            else if (distanceToGround > _fallHeightStart)
            {
                StartFalling();
            }
        }
        else
        {
            _isGrounded = true;
            if (_animator.GetBool("Falling"))
            {
                //we land
                _animator.SetBool("Falling", false);
                Debug.LogWarning("we fell: " + (_heightAtFallStart - transform.position.y));
                if (_heightAtFallStart - transform.position.y > _fatalFallHeight)
                    Die();
            }
        }
        return (_isGrounded);
    }

    private void StartFalling()
    {
        _heightAtFallStart = transform.position.y;
        Animator.SetBool("Falling", true);
        _state = CharacterState.Falling;
    }

    public void GoToTalkingState()
    {
        _state = CharacterState.Talking;
        ForceSpeed(0);
        ForceTurningAngle(0);
        _vertical = 0f;
        _horizontal = 0f;
        Animator.SetBool("Backwards", false);
    }
    public void GoToTalkingLastLineState()
    {
        _state = CharacterState.TalkingLastLine;
    }
    public void GoToIdleState()
    {
        _state = CharacterState.Idle;
    }
    public void ForceSpeed(float speed)
    {
        _animator.SetFloat("Speed", speed);
    }

    public void ForceTurningAngle(float angle)
    {
        _animator.SetFloat("TurningAngle", angle);
    }

    public bool IsInSlide()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Slide");
    }

    public bool IsInJump()
    {
        return (IsInIdleJump() || IsInLocomotionJump());
    }

    public bool IsInIdleTurning()
    {
        return _stateInfo.fullPathHash == _m_IdlePivotLId || _stateInfo.fullPathHash == _m_IdlePivotRId;
    }

    public bool IsInIdleJump()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.IdleJump");
    }

    public bool IsInLocomotionJump()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.LocomotionJump");
    }

    public bool IsInPivot()
    {
        return _stateInfo.fullPathHash == _m_LocomotionPivotLId || 
            _stateInfo.fullPathHash == _m_LocomotionPivotRId ||
            _transInfo.nameHash == _m_LocomotionPivotLTransId ||
            _transInfo.nameHash == _m_LocomotionPivotRTransId;
    }

    public bool IsInLocomotion()
    {
        return _stateInfo.fullPathHash == _m_LocomotionId;
    }

	public void StickToWorldSpace(Transform root, Transform camera, ref float directionOut, ref float speedOut, ref float angleOut, bool isPivoting)
	{
		Vector3 rootDirection = root.forward;

		Vector3 stickDirection = new Vector3 (_horizontal, 0, _vertical);
		speedOut = stickDirection.sqrMagnitude;

		//Get camera rotation
		Vector3 CameraDirection = camera.forward;
		CameraDirection.y = 0.0f; ///kill y
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(CameraDirection));

		//Convert joystick input in worldspace coordinates
		Vector3 moveDirection = referentialShift * stickDirection;
		Vector3 axisSign = Vector3.Cross (moveDirection, rootDirection);

		Debug.DrawRay (new Vector3 (root.position.x, root.position.y + 2f, root.position.z), moveDirection, Color.green);
		Debug.DrawRay (new Vector3 (root.position.x, root.position.y + 2f, root.position.z), rootDirection, Color.magenta);
		Debug.DrawRay (new Vector3 (root.position.x, root.position.y + 2f, root.position.z), stickDirection, Color.blue);

		float angleRootToMove = Vector3.Angle (rootDirection, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);

        if(!isPivoting)
        {
            angleOut = angleRootToMove;
        }

		angleRootToMove /= 180f;

		directionOut = angleRootToMove * _directionSpeed;
	}

    private void Die()
    {
        Debug.LogError("DIE!");
        _state = CharacterState.Dead;
        SetupRagdoll();
    }

    private void Reset() //reset character so we can play again
    {
        _state = CharacterState.Idle;
        transform.position = _initialPosition.transform.position;
        transform.rotation = _initialPosition.transform.rotation;
        ClearRagdoll();
    }

    private void SetupRagdoll()
    {
    if(_ragdoll == null)
        {
            _ragdoll = GameObject.Instantiate(Resources.Load("Prefabs/Emmon Ragdoll"), 
                                            transform.position, 
                                            transform.rotation) as GameObject;
        }

        var characterPelvis = transform.FindChild("Emmon_Reference/Emmon_Hips");
        var ragdollPelvis = _ragdoll.transform.FindChild("Emmon_Reference/Emmon_Hips");

        MatchChildrenTransforms(characterPelvis, ragdollPelvis);

        var geometry = transform.FindChild("geometry");
        foreach (Transform child in geometry)
        {
            child.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        //camera, look at the ragdoll
        _gameCam.FindCameraRagdollTarget(_ragdoll);
    }

    void ClearRagdoll()
    {
        if(_ragdoll != null)
        {
            GameObject.Destroy(_ragdoll);
            _ragdoll = null;
        }

        Transform geometry = transform.FindChild("geometry");

        foreach(Transform child in geometry)
        {
            child.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }

        _gameCam.FindRestartCameraTarget();


    }

    void MatchChildrenTransforms(Transform source, Transform target)
    {
        if (source.childCount > 0)
        {
            foreach (Transform sourceTransform in source.transform)
            {
                Transform targetTransform = target.Find(sourceTransform.name);

                if (targetTransform != null)
                {
                    MatchChildrenTransforms(sourceTransform, targetTransform);
                    targetTransform.localPosition = sourceTransform.localPosition;
                    targetTransform.localRotation = sourceTransform.localRotation;
                }
            }
        }
    }

}

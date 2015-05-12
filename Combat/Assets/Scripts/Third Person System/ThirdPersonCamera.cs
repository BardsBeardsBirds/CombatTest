using UnityEngine;
using System.Collections;
//using UnityEditor;

struct CameraPosition
{
    //position to align camera to, probably soewhere behind the character
    //or position to point camera at, probably somewhere along the character's axis
    private Vector3 _position;
    //transform used for any rotation
    private Transform _xForm;

    public Vector3 Position { get { return _position; } set { _position = value; } }
    public Transform XForm { get { return _xForm; } set { _xForm = value; } }

    public void Init(string camName, Vector3 pos, Transform transform, Transform parent)
    {
        _position = pos;
        _xForm = transform;
        _xForm.name = camName;
        _xForm.parent = parent;
        _xForm.localPosition = Vector3.zero;
        _xForm.localPosition = pos;
    }
}

public class ThirdPersonCamera : MonoBehaviour 
{
    [SerializeField]
    private Transform _cameraXForm;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
	private float _distanceAway;
    [SerializeField]
    private float _distanceAwayMultiplier = 1.5f; 
	[SerializeField]
	private float _distanceUp;
    [SerializeField]
    private float _distanceUpMultiplier = 1.5f; 
	[SerializeField]
	private float _smooth;
	[SerializeField]
	private Transform _followXForm;
    [SerializeField]
    private CharacterControllerLogic _follow;
    [SerializeField]
    private Vector2 _firstPersonXAxisClamp = new Vector2(-70.0f, 60.0f);
    [SerializeField]
    private float _firstPersonLookSpeed = 1.5f;
    [SerializeField]
    private float _fPSRotationDegreePerSecond = 120f;
    [SerializeField]
    private float _firstPersonThreshold = 0.5f;
    [SerializeField]
    private float _freeThreshold = -0.1f;
    [SerializeField]
    private Vector2 _camMinDistFromChar = new Vector2(1f, -0.5f);
    [SerializeField]
    private float _rightStickThreshold = 0.1f;
    [SerializeField]
    private const float _freeRotationDegreePerSecond = -5f;
    [SerializeField]
    private float _mouseWheelSensitivity = 3.0f;
    [SerializeField]
    private float _compensationOffset = 0.2f;
    [SerializeField]
    private CamStates _startingState = CamStates.Free;

    private Vector3 _velocityCamSmooth = Vector3.zero;
    [SerializeField]
    private float _camSmoothDampTime = 0.1f;
    private Vector3 _velocityLookDir = Vector3.zero;
    [SerializeField]
    private float _lookDirDampTime = 0.1f;

	private Vector3 _lookDir;
    private Vector3 _curLookDir;
	private Vector3 _targetPosition;
    private CamStates _camState = CamStates.Behind;
    private float _xAxisRot = 0.0f;
    private CameraPosition _firstPersonCamPos;
    private const float BEHIND_THRESHOLD = 0.01f;
    private Vector3 _savedRigToGoal;
    private float _lookWeight;
    private float _distanceAwayFree;
    private float _distanceUpFree;
    private Vector2 _rightStickPrevFrame = Vector2.zero;
    private float _lastStickMin = float.PositiveInfinity;
    private Vector3 _nearClipDimensions = Vector3.zero; // width, height, radius
    private Vector3[] _viewFrustum;
    private Vector3 _characterOffset;

    public Transform CameraXform
    {
        get
        {
            return this._cameraXForm;
        }
    }
    public Vector3 LookDir
    {
        get
        {
            return this._curLookDir;
        }
    }

    public CamStates CamState
    {
        get
        {
            return this._camState;
        }
    }

    public enum CamStates
    {
        Behind,
        FirstPerson,
        Target,
        Free
    }

    public Vector3 RigToGoalDirection
    {
        get
        {
            // Move height and distance from character in separate parentRig transform since RotateAround has control of both position and rotation
            Vector3 rigToGoalDirection = Vector3.Normalize(_characterOffset - this.transform.position);
            // Can't calculate distanceAway from a vector with Y axis rotation in it; zero it out
            rigToGoalDirection.y = 0f;
            return rigToGoalDirection;
        }
    }

	void Start () 
	{
        _cameraXForm = this.transform;//.parent;
        _camera = this.GetComponent<Camera>();

        if (_cameraXForm == null)
        {
            Debug.LogError("Parent camera to empty GameObject.", this);
        }

        _follow = GameObject.FindWithTag("Player").GetComponent<CharacterControllerLogic>();
		_followXForm = GameObject.FindWithTag ("CameraTarget").transform;

        _lookDir = _followXForm.forward;
        _curLookDir = _followXForm.forward;

        //position and parent a gameobject where first person view should be
        _firstPersonCamPos = new CameraPosition();
        _firstPersonCamPos.Init
        (
            "First Person Camera",
            new Vector3(0.0f, 1.7f, 0.2f),
            new GameObject().transform,
            _follow.transform
        );

        _camState = _startingState;

        // Intialize values to avoid having 0s
        _characterOffset = _followXForm.position + new Vector3(0f, _distanceUp, 0f);
        _distanceUpFree = _distanceUp;
        _distanceAwayFree = _distanceAway;
        _savedRigToGoal = RigToGoalDirection;
	}

    //void OnDrawGizmos()
    //{
    //    if (EditorApplication.isPlaying && !EditorApplication.isPaused)
    //    {
    //        DebugDraw.DrawDebugFrustum(_viewFrustum);
    //    }
    //}

	void LateUpdate () 
	{
        _viewFrustum = DebugDraw.CalculateViewFrustum(_camera, ref _nearClipDimensions);

        //pull values rom controller/keybord
        float rightX = 0f;
        float rightY = 0f;

        float leftX = Input.GetAxis("Horizontal");
        float leftY = Input.GetAxis("Vertical");
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        float mouseWheelScaled = mouseWheel * _mouseWheelSensitivity;
        float leftTrigger = 0f;
        bool bButtonPressed = Input.GetKey(KeyCode.Escape);
        bool qKeyDown = Input.GetKey(KeyCode.Q);
        bool eKeyDown = Input.GetKey(KeyCode.E);
        bool lShiftKeyDown = Input.GetKey(KeyCode.LeftShift);

        if (mouseWheel != 0)
        {
            rightY = mouseWheelScaled;
        }
        if (qKeyDown)
        {
            rightX = 1f;
        }
        if (eKeyDown)
        {
            rightX = -1f;
        }
        if (lShiftKeyDown)
        {
            leftTrigger = 1f;
        }

        _characterOffset = _followXForm.position + (_distanceUp * _followXForm.up);
        Vector3 lookAt = _characterOffset;
        _targetPosition = Vector3.zero;

        //determine camera state
        //target camera
        if (leftTrigger < BEHIND_THRESHOLD)
        {
            _camState = CamStates.Target;
        }
        else
        {
            // first person
            if (rightY > _firstPersonThreshold && _camState != CamStates.Free && !_follow.IsInLocomotion())
            {
                //reset look before entering the first person mode
                _xAxisRot = 0;
                _lookWeight = 0f;
                _camState = CamStates.FirstPerson;
            }
            // free camera
            if ((rightY < _freeThreshold || mouseWheel < 0f) && System.Math.Round(_follow.Speed, 2) == 0)
            {
                _camState = CamStates.Free;
                _savedRigToGoal = Vector3.zero;
            }
            //behind
            if ((_camState == CamStates.FirstPerson && bButtonPressed) ||
                (_camState == CamStates.Target && leftTrigger >= BEHIND_THRESHOLD))
            {
                _camState = CamStates.Behind;
            }
        }

       //execute camera states
        switch (_camState)
        {
            case CamStates.Behind:
                ResetCamera();
                // Only update camera look direction if moving
                if (_follow.Speed > _follow.LocomotionThreshold && _follow.IsInLocomotion() && !_follow.IsInPivot())
                {

                    _lookDir = Vector3.Lerp(_followXForm.right * (leftX < 0 ? 1f : -1f), _followXForm.forward * (leftY < 0 ? -1f : 1f), Mathf.Abs(Vector3.Dot(this.transform.forward, _followXForm.forward)));
                    Debug.DrawRay(this.transform.position, _lookDir, Color.white);
                    // Calculate direction from camera to player, kill Y, and normalize to give a valid direction with unit magnitude
                    _curLookDir = Vector3.Normalize(_characterOffset - this.transform.position);
                    _curLookDir.y = 0;
                    Debug.DrawRay(this.transform.position, _curLookDir, Color.green);
                    // Damping makes it so we don't update targetPosition while pivoting; camera shouldn't rotate around player
                    _curLookDir = Vector3.SmoothDamp(_curLookDir, _lookDir, ref _velocityLookDir, _lookDirDampTime);
                }
                _targetPosition = _characterOffset + _followXForm.up * _distanceUp - Vector3.Normalize(_curLookDir) * _distanceAway;
                Debug.DrawLine(_followXForm.position, _targetPosition, Color.magenta);
                break;
            case CamStates.Target:
                ResetCamera();
                _lookDir = _followXForm.forward;
                _curLookDir = _followXForm.forward;

                if (CharacterControllerLogic.Instance.State == CharacterControllerLogic.CharacterState.Talking || CharacterControllerLogic.Instance.State == CharacterControllerLogic.CharacterState.TalkingLastLine)
                    _targetPosition = _characterOffset + _followXForm.up * _distanceUp - Vector3.Normalize(_curLookDir) * _distanceAway;
              //      _targetPosition = _cameraXForm.position;
                else 
                    _targetPosition = _characterOffset + _followXForm.up * _distanceUp - _lookDir * _distanceAway;

                break;
            case CamStates.FirstPerson:
                //looking up and down
                //calculate the amount of rotation and apply to the firstPersonCamPos GameObject
                _xAxisRot += (leftY * _firstPersonLookSpeed);
                _xAxisRot = Mathf.Clamp(_xAxisRot, _firstPersonXAxisClamp.x, _firstPersonXAxisClamp.y);
                _firstPersonCamPos.XForm.localRotation = Quaternion.Euler(_xAxisRot, 0, 0);

                //superimpose first personCamPos GameObject's rotation on camera
                Quaternion rotationShift = Quaternion.FromToRotation(this.transform.forward, _firstPersonCamPos.XForm.forward);
                this.transform.rotation = rotationShift * this.transform.rotation;

                _lookWeight = Mathf.Lerp(_lookWeight, 1.0f, Time.deltaTime * _firstPersonLookSpeed);

                //looking left and right
                //similarly to how the character is rotated while in locomotion, use Quaternion * to add rotation to character
                Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, _fPSRotationDegreePerSecond * (leftX < 0f ? -1f : 1f), 0f), Mathf.Abs(leftX));
                Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
                _follow.transform.rotation = (_follow.transform.rotation * deltaRotation);

                //move camera to firstpersonCamPos
                _targetPosition = _firstPersonCamPos.XForm.position;

                //smoothly transition look direction towards fpcampos when entering first person mode
                lookAt = Vector3.Lerp(_targetPosition + _followXForm.forward, this.transform.position + this.transform.forward, _camSmoothDampTime * Time.deltaTime);

                //choose lookat target based on distance
                lookAt = Vector3.Lerp(this.transform.position + this.transform.forward, lookAt, Vector3.Distance(this.transform.position, _firstPersonCamPos.XForm.position));
                break;
            case CamStates.Free:
                _lookWeight = Mathf.Lerp(_lookWeight, 0.0f, Time.deltaTime * _firstPersonLookSpeed);

                Vector3 rigToGoal = _characterOffset - _cameraXForm.position;
                rigToGoal.y = 0f;
                Debug.DrawRay(_cameraXForm.transform.position, rigToGoal, Color.red);
                // Panning in and out
                // If statement works for positive values; don't tween if stick not increasing in either direction; also don't tween if user is rotating
                // Checked against rightStickThreshold because very small values for rightY mess up the Lerp function
                if (rightY < _lastStickMin && rightY < -1f * _rightStickThreshold && rightY <= _rightStickPrevFrame.y && Mathf.Abs(rightX) < _rightStickThreshold)
                {
                    // Zooming out
                    _distanceUpFree = Mathf.Lerp(_distanceUp, _distanceUp * _distanceUpMultiplier, Mathf.Abs(rightY));
                    _distanceAwayFree = Mathf.Lerp(_distanceAway, _distanceAway * _distanceAwayMultiplier, Mathf.Abs(rightY));
                    _targetPosition = _characterOffset + _followXForm.up * _distanceUpFree - RigToGoalDirection * _distanceAwayFree;
                    _lastStickMin = rightY;
                }
                else if (rightY > _rightStickThreshold && rightY >= _rightStickPrevFrame.y && Mathf.Abs(rightX) < _rightStickThreshold)
                {
                    // Zooming in
                    // Subtract height of camera from height of player to find Y distance
                    _distanceUpFree = Mathf.Lerp(Mathf.Abs(transform.position.y - _characterOffset.y), _camMinDistFromChar.y, rightY);
                    // Use magnitude function to find X distance
                    _distanceAwayFree = Mathf.Lerp(rigToGoal.magnitude, _camMinDistFromChar.x, rightY);
                    _targetPosition = _characterOffset + _followXForm.up * _distanceUpFree - RigToGoalDirection * _distanceAwayFree;
                    _lastStickMin = float.PositiveInfinity;
                }
                // Store direction only if right stick inactive
                if (rightX != 0 || rightY != 0)
                {
                    _savedRigToGoal = RigToGoalDirection;
                }
                // Rotating around character
                _cameraXForm.RotateAround(_characterOffset, _followXForm.up, _freeRotationDegreePerSecond * (Mathf.Abs(rightX) > _rightStickThreshold ? rightX : 0f));
                
            // Still need to track camera behind player even if they aren't using the right stick; achieve this by saving distanceAwayFree every frame
                if (_targetPosition == Vector3.zero)
                {
                    _targetPosition = _characterOffset + _followXForm.up * _distanceUpFree - _savedRigToGoal * _distanceAwayFree;
                }
                break;
        }

        compensateForWalls(_characterOffset, ref _targetPosition);
        smoothPosition(_cameraXForm.position, _targetPosition);
        transform.LookAt(lookAt);

        // Make sure to cache the unscaled mouse wheel value if using mouse/keyboard instead of controller
        _rightStickPrevFrame = new Vector2(rightX, mouseWheel != 0 ? mouseWheelScaled : rightY);

     //   _rightStickPrevFrame = new Vector2(rightX, rightY);
     //   _mouseWheel != 0 ? _mouseWheelScaled : rightY);
	}

	private void smoothPosition(Vector3 fromPos, Vector3 toPos)
	{
        // Making a smooth transition between camera's current position and the position it wants to be in
        _cameraXForm.position = Vector3.SmoothDamp(fromPos, toPos, ref _velocityCamSmooth, _camSmoothDampTime);
	}

    private void compensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
    {
        // Compensate for walls between camera
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(fromObject, toTarget, out wallHit))
        {
            Debug.DrawRay(wallHit.point, wallHit.normal, Color.red);
            toTarget = wallHit.point;
        }
        // Compensate for geometry intersecting with near clip plane
        Vector3 camPosCache = _camera.transform.position;
        _camera.transform.position = toTarget;
        _viewFrustum = DebugDraw.CalculateViewFrustum(_camera, ref _nearClipDimensions);

        for (int i = 0; i < (_viewFrustum.Length / 2); i++)
        {
            RaycastHit cWHit = new RaycastHit();
            RaycastHit cCWHit = new RaycastHit();
            // Cast lines in both directions around near clipping plane bounds
            while (Physics.Linecast(_viewFrustum[i], _viewFrustum[(i + 1) % (_viewFrustum.Length / 2)], out cWHit) ||
            Physics.Linecast(_viewFrustum[(i + 1) % (_viewFrustum.Length / 2)], _viewFrustum[i], out cCWHit))
            {
                Vector3 normal = wallHit.normal;
                if (wallHit.normal == Vector3.zero)
                {
                    // If there's no available wallHit, use normal of geometry intersected by LineCasts instead
                    if (cWHit.normal == Vector3.zero)
                    {
                        if (cCWHit.normal == Vector3.zero)
                        {
                            Debug.LogError("No available geometry normal from near clip plane LineCasts. Something must be amuck.", this);
                        }
                        else
                        {
                            normal = cCWHit.normal;
                        }
                    }
                    else
                    {
                        normal = cWHit.normal;
                    }
                }
                toTarget += (_compensationOffset * normal);
                _camera.transform.position += toTarget;
                // Recalculate positions of near clip plane
                _viewFrustum = DebugDraw.CalculateViewFrustum(_camera, ref _nearClipDimensions);
            }
        }
        _camera.transform.position = camPosCache;
        _viewFrustum = DebugDraw.CalculateViewFrustum(_camera, ref _nearClipDimensions);
    }

    private void ResetCamera()
    {
        _lookWeight = Mathf.Lerp(_lookWeight, 0.0f, Time.deltaTime * _firstPersonLookSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
    }

    public void FindCameraRagdollTarget(GameObject ragdoll)
    {
    //    Vector3 cameraPos = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y + 2.5f, this.transform.position.z), 5 * Time.deltaTime);

        _followXForm = ragdoll.transform.FindChild("Emmon_Reference/Emmon_Hips/Emmon_Spine");
    }

    public void FindRestartCameraTarget()
    {
        _followXForm = GameObject.FindWithTag("CameraTarget").transform;
    }
}

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public enum PlayerStates
{
    Rest
}
public class PlayerScript : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] float _moveSpeed = 13;
    [SerializeField] float _rotationSpeed = 80;
    [SerializeField] float _planeRotationSpeed = 50;

    [Header("PlayerInput")]
    //[SerializeField] CharacterController _playerCC;
    [SerializeField] Vector2 _movementInput;
    [SerializeField] Vector2 _rotateInput;
    [SerializeField] bool _southButtonInput;
    [SerializeField] bool _westButtonInput;
    [SerializeField] bool _northButtonInput;
    [SerializeField] bool _eastButtonInput;
    [SerializeField] bool _RSInput;
    [SerializeField] bool _LSInput;
    
    private Vector3 _moveVector;
    private Vector3 _appliedMoveVector;

    [Header("Debug")]
    [SerializeField] PlayerStates _playerState;
    private CinemachineVirtualCamera _gameCam;
    private static PlayerScript _playerInstance;
    private GameManagerScript _gameManager;
    private SceneManagerScript _sceneManager;
    private InputManagerScript _inputManager;
    private MeshRenderer _playerMesh;
    private Transform _body;

    // G&S
    public static PlayerScript PlayerInstance { get { return _playerInstance; } }
    public Vector2 MovementInput { get { return _movementInput; } set { _movementInput = value; } }
    public CinemachineVirtualCamera InGameCamera { get { return _gameCam; } }

    // Main
    private void Awake() 
    {
        PlayerSingleton();
    }
    private void Start()
    {
        SetUpReferences();
        SubscribeToEvents();
        ResetPlayer();
    }
    private void Update()
    {
        Move(MovementInput);
        PlaneRotations();
    }
    
    // G&S
    public Vector3 CursorPosition { get { return transform.position; } }

    // Essentials
    private void PlayerSingleton()
    {
        if (_playerInstance == null)
        {
            _playerInstance = this;
        }
        else if (_playerInstance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
    private void SetUpReferences()
    {
        _gameManager = GameManagerScript.GMInstance;
        _inputManager = InputManagerScript.IMInstance;
        _sceneManager = SceneManagerScript.SMInstance;
        //_playerCC = gameObject.GetComponent<CharacterController>();
        _gameCam = GetComponentInChildren<CinemachineVirtualCamera>();
        _body = transform.GetChild(0).transform;
        _playerMesh = GetComponent<MeshRenderer>();
    }
    private void SubscribeToEvents()
    {
        _gameManager.OnGMSetUpComplete -= SetUpPlayer;
        _gameManager.OnGMSetUpComplete += SetUpPlayer;
    }
    public void SubscribeGameInputs()
    {
        // UNSUB
        _inputManager.InputMap.Game.Move.performed -= OnMove;
        _inputManager.InputMap.Game.Rotate.performed -= OnRotate;
        _inputManager.InputMap.Game.ButtonSouth.started -= OnButtonSouth;
        _inputManager.InputMap.Game.ButtonWest.performed -= OnButtonWest;
        _inputManager.InputMap.Game.ButtonEast.started -= OnButtonEast;
        _inputManager.InputMap.Game.ShoulderR.started -= OnShoulderR;
        _inputManager.InputMap.Game.ShoulderL.started -= OnShoulderL;

        _inputManager.InputMap.Game.Move.canceled -= OnMove;
        _inputManager.InputMap.Game.Rotate.canceled -= OnRotate;
        _inputManager.InputMap.Game.ButtonSouth.canceled -= OnButtonSouth;
        _inputManager.InputMap.Game.ButtonWest.canceled -= OnButtonWest;
        _inputManager.InputMap.Game.ButtonEast.canceled -= OnButtonEast;
        _inputManager.InputMap.Game.ShoulderR.canceled -= OnShoulderR;
        _inputManager.InputMap.Game.ShoulderL.canceled -= OnShoulderL;

        // SUB
        _inputManager.InputMap.Game.Move.performed += OnMove;
        _inputManager.InputMap.Game.Rotate.performed += OnRotate;
        _inputManager.InputMap.Game.ButtonSouth.started += OnButtonSouth;
        _inputManager.InputMap.Game.ButtonWest.started += OnButtonWest;
        _inputManager.InputMap.Game.ButtonNorth.started += OnButtonNorth;
        _inputManager.InputMap.Game.ButtonEast.started += OnButtonEast;
        _inputManager.InputMap.Game.ShoulderR.started += OnShoulderR;
        _inputManager.InputMap.Game.ShoulderL.started += OnShoulderL;
        //_inputManager.InputMap.Game.StartButton.performed += OnStartButton;

        _inputManager.InputMap.Game.Move.canceled += OnMove;
        _inputManager.InputMap.Game.Rotate.canceled += OnRotate;
        _inputManager.InputMap.Game.ButtonSouth.canceled += OnButtonSouth;
        _inputManager.InputMap.Game.ButtonWest.canceled += OnButtonWest;
        _inputManager.InputMap.Game.ButtonNorth.canceled += OnButtonNorth;
        _inputManager.InputMap.Game.ButtonEast.canceled += OnButtonEast;
        _inputManager.InputMap.Game.ShoulderR.canceled += OnShoulderR;
        _inputManager.InputMap.Game.ShoulderL.canceled += OnShoulderL;
        //_inputManager.InputMap.Game.StartButton.canceled += OnStartButton;
    }
    private void SetUpPlayer()
    {
        _playerState = PlayerStates.Rest;

        if (SceneManager.GetActiveScene().buildIndex == 4 ||
            SceneManager.GetActiveScene().buildIndex == 5 ||
            SceneManager.GetActiveScene().buildIndex == 6)
        {
            SpawnPlayer(new Vector3(0, 1, -6));
        }
        else
        {
            TogglePlayerMesh(false);
        }
    }

    // Player Spawn
    public void ResetPlayer() 
    {
        _gameManager.ResetScore();
        _gameManager.Victory = false;
    }
    public void TogglePlayerMesh(bool state)
    {
        if (state != _playerMesh.enabled)
        {
            _playerMesh.enabled = state;
        }
    }
    public void MoveToSpawnPoint(Vector3 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, pos.z);
    }
    public void SpawnPlayer(Vector3 pos)
    {
        TogglePlayerMesh(true);
        MoveToSpawnPoint(pos);
    }

    // PlayerUI
    private void LinkUI()
    {
        // Link UI
    }

    // Player Movement
    private void Move(Vector2 input)
    {
        /*_moveVector.x = 0;
        _moveVector.z = input.y * _moveSpeed;

        _appliedMoveVector = transform.TransformDirection(_moveVector);
        _playerCC.Move(_appliedMoveVector * Time.deltaTime);
        gameObject.transform.Rotate(new Vector3(0, input.x * _rotationSpeed * Time.deltaTime, 0));*/
        Vector3 moveVector = new Vector3(0f, 0f, input.y) * _moveSpeed * Time.deltaTime;
        transform.Translate(moveVector, Space.Self);

        float rotation = input.x * _rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotation, 0f);
    }

    // Plane
    private void RotatePlane(bool input, Vector3 direction)
    {
        if (input)
        {
            transform.Rotate(direction, _planeRotationSpeed * Time.deltaTime);
        }
    }
    private void PlaneRotations()
    {
        RotatePlane(_southButtonInput, Vector3.left);
        RotatePlane(_northButtonInput, Vector3.right);
        RotatePlane(_westButtonInput, Vector3.forward);
        RotatePlane(_eastButtonInput, Vector3.back);
    }

    // Collision
    

    private void OnMove(InputAction.CallbackContext context) 
    {
        MovementInput = context.ReadValue<Vector2>();
        //Debug.Log("MovePlayer");
    }
    private void OnRotate(InputAction.CallbackContext context)
    {
        _rotateInput = context.ReadValue<Vector2>();
        //Debug.Log("RotateInput");
    }
    private void OnButtonSouth(InputAction.CallbackContext context) 
    {
        _southButtonInput = context.ReadValueAsButton();
        //Debug.Log("SouthPlayer");
    }
    private void OnButtonWest(InputAction.CallbackContext context) 
    {
        _westButtonInput = context.ReadValueAsButton();
        //Debug.Log("WestPlayer");
    }
    private void OnButtonNorth(InputAction.CallbackContext context) 
    {
        _northButtonInput = context.ReadValueAsButton();
        Debug.Log("NorthPlayer");
    }
    private void OnButtonEast(InputAction.CallbackContext context) 
    {
        _eastButtonInput = context.ReadValueAsButton();
        //Debug.Log("EastPlayer");
    }
    private void OnShoulderR(InputAction.CallbackContext context) 
    {
        _RSInput = context.ReadValueAsButton();
        //Debug.Log("ShoulderRPlayer");
    }
    private void OnShoulderL(InputAction.CallbackContext context) 
    {
        _LSInput = context.ReadValueAsButton();  
        //Debug.Log("ShoulderLPlayer");
    }
    private void OnStartButton(InputAction.CallbackContext context) 
    {
        Debug.Log("StartPlayer");
    }

    // OnTrigger
    /*private void GetCollisionPoints(Collider other)
    {
        Bounds bounds = other.bounds;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        List<Vector3> intersectionPoints = new List<Vector3>();

        for (float x = min.x; x <= max.x; x += bounds.size.x)
        {
            for (float y = min.y; y <= max.y; y += bounds.size.y)
            {
                for (float z = min.z; z <= max.z; z += bounds.size.z)
                {
                    Vector3 intersectionPoint = new Vector3(x, y, z);
                    intersectionPoints.Add(intersectionPoint);
                }
            }
        }

        // Loop through the intersection points
        for (int i = 0; i < intersectionPoints.Count; i++)
        {
            Vector3 intersectionPoint = other.transform.TransformPoint(intersectionPoints[i]);
            Debug.Log("Intersection Point " + i + ": " + intersectionPoint);
        }
        
    }
    private void OnTriggerStay(Collider other)
    {
        if (_RSInput)
        {
            GetCollisionPoints(other);
        }
        


    }*/
    private void OnCollisionStay(Collision collision)
    {
        if (_RSInput)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.Log("Contact point: " + contact.point);
            }
        }
    }
}

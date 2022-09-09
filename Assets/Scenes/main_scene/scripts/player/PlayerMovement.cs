using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [SerializeField]
    CharacterController _characterController;
    [SerializeField]
    Transform _cameraParent;
    [SerializeField]
    PhotonView _photonView;

    [SerializeField]
    float _moveSpeed = 3f;
    [SerializeField]
    float _mouseRotationSpeed = 3f;

    [SerializeField]
    LayerMask _interactableObjectslayer;

    [SerializeField]
    LayerMask _worldObjectsLayer;

    [SerializeField]
    PlayerNamePanel _playerNamePanel;

    bool _isPhotonMine;
    Vector3 _fallVelocity;
    PlayerState _playerState;

    public Player PhotonOwner
    {
        get
        {
            return _photonView.Owner;
        }
    }

    public void Initialize(string playerName, PlayerProfile.PlayerSkinColors skinColor, bool isMe)
    {
        _isPhotonMine = isMe;

        _playerNamePanel.DisplayPlayerName(playerName, Camera.main);

        GetComponent<Renderer>().material.SetColor("_Color", skinColor.ToColor());
    }

    public void SitDownPlayerOnChair(Chair chair, Transform playerPosition)
    {
        _playerState = PlayerState.Sitting;
        this.transform.position = playerPosition.position;
        this.transform.rotation = playerPosition.rotation;

        Singleton<ApplicationCore>.Instance.PlayerSittedOnChair(this, chair);
    }

    public void GrabChair(Chair chair)
    {
        if (this._isPhotonMine)
            _playerState = PlayerState.MovingChair;
        
        chair.transform.SetParent(this.transform);
        chair.transform.localPosition = new Vector3(0f, -0.5f, 1f);
        chair.transform.localRotation = Quaternion.identity;

        Singleton<ApplicationCore>.Instance.PlayerGrabbedChair(this, chair);
    }

    public void SetCameraOnPlayer(Camera camera)
    {
        camera.transform.SetParent(_cameraParent);
        camera.transform.localPosition = Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
    }

    public void StandUpFromChair(Chair targetChair)
    {
        this._playerState = PlayerState.Walking;

        this.transform.position = targetChair.PlayedStandUpPosition;
    }

    public void DropChair(Chair targetChair)
    {
        if (_isPhotonMine)
            this._playerState = PlayerState.Walking;

        targetChair.transform.localPosition = new Vector3(0f, -1f, 1f);

        targetChair.transform.SetParent(Singleton<ChairsObjectsManager>.Instance.ChairsParent);
    }

    void WalkingUpdate()
    {
        Moving();

        CheckChairsInFront();
    }

    void Moving()
    {
        var move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");

        _characterController.Move(move * (_moveSpeed * Time.deltaTime));

        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            transform.Rotate(_mouseRotationSpeed * Input.GetAxisRaw("Mouse X") * Vector3.up);
        }
        else
            Cursor.lockState = CursorLockMode.None;

        _fallVelocity.y += -9f * Time.deltaTime;

        _characterController.Move(_fallVelocity * Time.deltaTime);
    }

    void CheckChairsInFront()
    {
        if (HasChairInFront(out var chair))
            Singleton<ApplicationCore>.Instance.ShowChairInfo(chair);
        else
            Singleton<ApplicationCore>.Instance.HideChairInfo();
    }

    void SittingUpdate()
    {
        // По идее, ничего не делает игрок.
    }

    void MovingChairUpdate()
    {
        Moving();

        CheckCanDropChair();
    }

    void CheckCanDropChair()
    {
        var startPos = transform.position;
        var endPos = startPos + transform.forward;
        Debug.DrawLine(startPos, endPos, Color.red);

        if (Physics.Linecast(startPos, endPos, _worldObjectsLayer))
        {
            Singleton<ApplicationCore>.Instance.ShowCanDropChair(false);
            return;
        }

        startPos = transform.position - transform.right * 0.5f;
        endPos = startPos + transform.forward;
        Debug.DrawLine(startPos, endPos, Color.red);

        if (Physics.Linecast(startPos, endPos, _worldObjectsLayer))
        {
            Singleton<ApplicationCore>.Instance.ShowCanDropChair(false);
            return;
        }

        startPos = transform.position + transform.right * 0.5f;
        endPos = startPos + transform.forward;
        Debug.DrawLine(startPos, endPos, Color.red);

        if (Physics.Linecast(startPos, endPos, _worldObjectsLayer))
        {
            Singleton<ApplicationCore>.Instance.ShowCanDropChair(false);
            return;
        }

        Singleton<ApplicationCore>.Instance.ShowCanDropChair(true);
    }

    bool HasChairInFront(out Chair chair)
    {
        chair = null;

        var startpos = transform.position + new Vector3(0f, -0.5f, 0f) + transform.forward;
        var endPos = transform.position + new Vector3(0f, 0.5f, 0f) + transform.forward;

        Debug.DrawLine(startpos, endPos, Color.red);

        if (Physics.Linecast(startpos, endPos, out var hitInfo, _interactableObjectslayer) && hitInfo.collider.tag == Chair.CHAIR_TAG)
            chair = hitInfo.collider.GetComponent<Chair>();
        else
        {
            startpos = transform.position + new Vector3(0f, -0.25f, 0f);
            endPos = startpos + transform.forward;

            Debug.DrawLine(startpos, endPos, Color.blue);

            if (Physics.Linecast(startpos, endPos, out hitInfo, _interactableObjectslayer) && hitInfo.collider.tag == Chair.CHAIR_TAG)
                chair = hitInfo.collider.GetComponent<Chair>();
        }

        return chair != null;
    }

    #region UNITY

    void Update()
    {
        if (!_isPhotonMine)
            return;

        switch (_playerState)
        {
            case PlayerState.Walking:
                WalkingUpdate();
                break;
            case PlayerState.Sitting:
                SittingUpdate();
                break;
            case PlayerState.MovingChair:
                MovingChairUpdate();
                break;
            default:
                break;
        }
    }

    #endregion

    #region Inner Types

    enum PlayerState
    {
        Walking,
        Sitting,
        MovingChair,
    }

    #endregion
}

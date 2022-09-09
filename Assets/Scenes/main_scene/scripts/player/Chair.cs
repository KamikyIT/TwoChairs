using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(PhotonView))]
public class Chair : MonoBehaviour
{
    public const string CHAIR_TAG = "Chair";

    static Color AVAILABLE_COLOR = new Color(0f, 1f, 0f, 0.5f);
    static Color UNAVAILABLE_COLOR = new Color(1f, 0f, 0f, 0.5f);

    [SerializeField]
    Transform _playerPosition;

    [SerializeField]
    Transform _playerStandUpPosition;

    [SerializeField]
    string _chairId;

    [SerializeField]
    Renderer _availableRenderer;

    BoxCollider _boxCollider;
    Player _occupantPlayer;
    Player _grabberPlayer;
    PhotonView _photonView;

    public string ChairId { get { return _chairId; } }

    public PhotonView PhotonView
    {
        get
        {
            if (_photonView == null)
                _photonView = GetComponent<PhotonView>();
            return _photonView;
        }
    }

    public BoxCollider BoxCollider
    {
        get
        {
            if (_boxCollider == null)
                _boxCollider = GetComponent<BoxCollider>();
            return _boxCollider;
        }
    }

    public Player OccupantPlayer
    {
        get
        {
            return _occupantPlayer;
        }
        set
        {
            if (value == null)
            {
                BoxCollider.enabled = true;
                _occupantPlayer = null;
                return;
            }

            var targetPlayerComponent = Singleton<PlayersObjectsManager>.Instance.FindPlayerGameObject(value);
            if (targetPlayerComponent != null)
            {
                _occupantPlayer = value;
                BoxCollider.enabled = true;
                targetPlayerComponent.SitDownPlayerOnChair(this, this._playerPosition);
            }
        }
    }

    public Player GrabberPlayer
    {
        get
        {
            return _grabberPlayer;
        }
        set
        {
            if (value == null)
            {
                BoxCollider.enabled = true;
                _grabberPlayer = null;
                return;
            }

            var targetPlayerComponent = Singleton<PlayersObjectsManager>.Instance.FindPlayerGameObject(value);
            if (targetPlayerComponent != null)
            {
                _grabberPlayer = value;
                BoxCollider.enabled = false;

                targetPlayerComponent.GrabChair(this);
            }
        }
    }

    public Vector3 PlayedStandUpPosition
    {
        get
        {
            return _playerStandUpPosition.transform.position;
        }
    }

    #region UNITY

    void Update()
    {
        if (OccupantPlayer == null && GrabberPlayer == null)
        {
            _availableRenderer.gameObject.SetActive(true);
            _availableRenderer.material.SetColor("_Color", AVAILABLE_COLOR);
        }
        else if (OccupantPlayer == PhotonNetwork.LocalPlayer || GrabberPlayer == PhotonNetwork.LocalPlayer)
            _availableRenderer.gameObject.SetActive(false);
        else
        {
            _availableRenderer.gameObject.SetActive(true);
            _availableRenderer.material.SetColor("_Color", UNAVAILABLE_COLOR);
        }
    }

    #endregion
}
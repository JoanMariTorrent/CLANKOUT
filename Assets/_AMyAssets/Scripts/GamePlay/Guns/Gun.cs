using System;
using System.Collections;
using NUnit.Framework;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;
using System.Collections.Generic;

public class Gun : StateNode
{
    [Header("Stats")]
    [SerializeField] private float _range = 20f;
    [SerializeField] private int _gunDamage = 10;
    [SerializeField] private float _fireRate = 0.5f;
    [SerializeField] private bool _automatic;

    [Header("Recoil")]
    [SerializeField] private float _recoilStrenght = 1f;
    [SerializeField] private float _recoilDuration = 0.2f;
    [SerializeField] private AnimationCurve _recoilCurve;
    [SerializeField] private AnimationCurve _rotationCurve;
    [SerializeField] private float _rotationAmount = 25f;

    [Header("References")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private List<Renderer> _renderers = new();
    [SerializeField] private ParticleSystem _enviormentHit, _playerHitEffect;


    private float _lastFireTime;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Coroutine _recoilCoroutine;

    private PlayerID _ownerID;

    private void Awake()
    {
        ToggleVisuals(false);
    }

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
            ToggleVisuals(true);
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        ToggleVisuals(false);
    }


    private void Start()
    {
        _originalPosition = transform.localPosition;
        _originalRotation = transform.localRotation;
    }

    private void ToggleVisuals(bool toggle)
    {
        foreach (var renderer in _renderers)
        { 
            renderer.enabled = toggle;
        }
    }

    protected override void OnSpawned()
    { 
        base.OnSpawned();

        enabled = isOwner;
    }




    public override void StateUpdate(bool asServer)
    {
        base.StateUpdate(asServer);

        // si este script no lo ejecuta el owner, no se hace la funcion entera
        if (!isOwner) return; 

        // Si es automatica y no mantiene el click o no es automatica y no pulsa el click, se sale de la funcion
        if (_automatic && !Input.GetKey(KeyCode.Mouse0) || !_automatic && !Input.GetKeyDown(KeyCode.Mouse0)) return;

        // si el ultimo disparo mas el cooldown de disparo sumado, es mas grande que el tiempo que llevas sin disparar antes de darle al click se sale de la funcion
        if (_lastFireTime + _fireRate > Time.unscaledTime) return;


        PlayShotEffect();
        _lastFireTime = Time.unscaledTime;


        // Hace un raycast, si este no cumple los requisitos dentro de los parentesis, sale de al funcion
        if (!Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out var hit, _range, _hitLayer, QueryTriggerInteraction.Ignore)) return;

        // Si el objetivo que le da al raycast, no es un jugador, ejecuta el void para el enviroment hit y se sale
        if (!hit.transform.TryGetComponent(out PlayerHealth _playerHealth)) 
        {
            EnviormentHit(hit.point, hit.normal);
            return;
        }

        // Ya con todos los pasos anteriores, esto es 100% un jugador, entonces se ejecuta el void con los VFX
        PlayerHit(_playerHealth, _playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);

        //Se le quita daño al jugador
        _playerHealth.ChangeHealth(-_gunDamage);

        //Busca un instance del scoreManager
        if (InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
        {
            //Si extiste el scoreManager, intante pillar el script de PlayerHealth (el principal del jugador)
            if (hit.transform.TryGetComponent(out PlayerHealth victim))
            {
                // Pide al servidor que aumente el daño en el Scoreboard del jugador afectado (no modifica el score directamente desde el cliente)
                scoreManager.AddDamageServerRpc(victim.PlayerID, _gunDamage);
            }
        }
    }








    [ObserversRpc(runLocally: true)]
    private void PlayerHit(PlayerHealth player, Vector3 localposition, Vector3 normal)
    {
        if (_playerHitEffect && player && player.transform)
        {
            var effect = Instantiate(_playerHitEffect, player.transform.TransformPoint(localposition), Quaternion.LookRotation(normal));
            effect.Play();
        }
    }


    [ObserversRpc(runLocally: true)]
    private void EnviormentHit(Vector3 position, Vector3 normal)
    {
        if (_enviormentHit)
        {
            var effect = Instantiate(_enviormentHit, position, Quaternion.LookRotation(normal));
            effect.Play();
        }
    }

    

    [ObserversRpc(runLocally:true)]
    private void PlayShotEffect()
    {
        if(_muzzleFlash)
            _muzzleFlash.Play();
        if (_recoilCoroutine != null)
            StopCoroutine(_recoilCoroutine);

        _recoilCoroutine = StartCoroutine(PlayRecoil());
    }

    private IEnumerator PlayRecoil()
    {
        float elapsed = 0f;

        while (elapsed < _recoilDuration)
        { 
            elapsed += Time.deltaTime;
            float curveTime = elapsed / _recoilDuration;

            //position recoil
            float recoilValue = _recoilCurve.Evaluate(curveTime);
            Vector3 recoilOffset = Vector3.back * (recoilValue * _recoilStrenght);
            transform.localPosition = _originalPosition + recoilOffset;

            //rotation recoil
            float rotationValue = _rotationCurve.Evaluate(curveTime);
            Vector3 rotationOffset = new Vector3(rotationValue * _rotationAmount, 0f, 0f);
            transform.localRotation = _originalRotation * Quaternion.Euler(rotationOffset);

            yield return null;
        }

        transform.localPosition = _originalPosition;
        transform.localRotation = _originalRotation;
    }
}

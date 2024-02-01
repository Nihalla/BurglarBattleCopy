using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CascadeGame
{
    public class Cascade : MonoBehaviour
    {
        [Header("Cascade Game Objects")]
        [SerializeField] private GameObject _bead;
        [SerializeField] private GameObject _track;
        [SerializeField] private GameObject _hitWindow;

        [Header("Cascade Parameters")]
        [SerializeField] private float _beadSpeed;
        [SerializeField] private float _hitWindowBuffer;
        [SerializeField] private float _hitWindowPos;
        [SerializeField] private float _hitWindowSize;

        [Header("Cascade Tracking")]
        private CascadeState _state;
        private Vector3 _beadStartPos;
        private Vector3 _trackScale;


        #region Getters/Setters

        #region Cascade Game Objects
        public GameObject GetBeadGameObject()
        {
            return _bead;
        }

        public GameObject GetTrackGameObject()
        {
            return _track;
        }

        public GameObject GetHitWindowGameObject()
        {
            return _hitWindow;
        }

        #endregion

        #region Cascade Parameters
        public float GetBeadSpeed()
        {
            return _beadSpeed;
        }

        public void SetBeadSpeed(float beadSpeed)
        {
            _beadSpeed = beadSpeed;
        }

        public float GetHitWindowBuffer()
        {
            return _hitWindowBuffer;
        }

        public void SetHitWindowBuffer(float hitWindowBuffer)
        {
            _hitWindowBuffer = hitWindowBuffer;
        }

        public float GetHitWindowPos()
        {
            return _hitWindowPos;
        }

        public void SetHitWindowPos(float hitWindowPos)
        {
            Vector3 trackPos = _track.transform.localPosition;
            _hitWindow.transform.localPosition = new Vector3(trackPos.x, (trackPos.y - _trackScale.y / 2) + (_trackScale.y * hitWindowPos) ,trackPos.z);
            _hitWindowPos = _hitWindow.transform.localPosition.y;
        }

        public float GetHitWindowSize()
        {
            return _hitWindowSize;
        }

        public void SetHitWindowSize(float hitWindowSize)
        {
            Vector3 trackScale = _track.transform.localScale;
            Vector3 hitWindowScale = _hitWindow.transform.localScale;
            _hitWindow.transform.localScale = new Vector3(hitWindowScale.x, trackScale.y * hitWindowSize, hitWindowScale.z);
            _hitWindowSize = _hitWindow.transform.localScale.y;
        }

        public void InitCascade(float beadSpeed, float hitWindowBuffer, float hitWindowPos, float hitWindowSize)
        {
            _beadStartPos = _bead.transform.position;
            _trackScale = _track.transform.localScale;

            SetBeadSpeed(beadSpeed);
            SetHitWindowBuffer(hitWindowBuffer);
            SetHitWindowPos(hitWindowPos);
            SetHitWindowSize(hitWindowSize);
        }

        public void ResetCascade()
        {
            _bead.transform.position = _beadStartPos;
            float hitWindowSize = Random.Range(0.15f, 0.4f);
            SetHitWindowSize(hitWindowSize);
            float hitWindowPos = Random.Range(GetHitWindowBuffer(), 1f - (_trackScale.y * (hitWindowSize / 2)));
            SetHitWindowPos(hitWindowPos);
        }
        #endregion

        #region Cascade Tracking
        public CascadeState GetState()
        {
            return _state;
        }

        public void SetState(CascadeState state)
        {
            _state = state;
        }

        public Vector3 GetBeadStartPos()
        {
            return _beadStartPos;
        }

        public void SetBeadStartPos(Vector3 startPos)
        {
            _beadStartPos = startPos;
        }

        public Vector3 GetTrackScale()
        {
            return _trackScale;
        }

        public void SetTrackScale(Vector3 trackScale)
        {
            _trackScale = trackScale;
        }

        #endregion

        #endregion

        private void FixedUpdate()
        {
            if (_state == CascadeState.RUNNING)
            {
                CascadeBead();
            }
        }

        private void CascadeBead()
        {
            float step = _beadSpeed * Time.deltaTime;
            Vector3 position = _bead.transform.position;
            Vector3 target = new Vector3(_beadStartPos.x, _beadStartPos.y - _trackScale.y, _beadStartPos.z);
            _bead.transform.position = Vector3.MoveTowards(position, target, step);
        }

        public void CheckWinCondition()
        {
            float beadPos = _bead.transform.localPosition.y;
            float hitWindowPos = _hitWindow.transform.localPosition.y;
            float hitWindowMin = hitWindowPos - (GetHitWindowSize() / 2);
            float hitWindowMax = hitWindowPos + (GetHitWindowSize() / 2);

            if (beadPos >= hitWindowMin && beadPos <= hitWindowMax)
            {
                SetState(CascadeState.SUCCESS);
            }
            else
            {
                SetState(CascadeState.FAILURE);
            }
        }

    }
    public enum CascadeState
    {
        INACTIVE,
        RUNNING,
        SUCCESS,
        FAILURE
    }
}

// Team Sisyphean - Beckham Bagley, Charlie Light, Joe Gollin, Louis Phillips, Ryan Sewell, Tom Roberts

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace PlayerControllers
{
    [RequireComponent(typeof(FirstPersonController))]
    public class Loot : MonoBehaviour
    {
        //loot value is currently public due to the large number of dependencies that have not moved over to use the getter and setters 
        public int currentLoot;
        [SerializeField] private int _maximumLoot;
        [SerializeField] private GameObject _lootSack;
        [SerializeField] private LayerMask _groundLayer;

        private GameObject _instantiatedLoot;
        private FirstPersonController _playerController;
        private float _sphereRadius = 1f;
        private float _distanceFromPlayer = 2f;

        [SerializeField] float lootDropDivider = 1.5f;

        [SerializeField] GameObject _lootBagObj;

        public FirstPersonController PlayerController => _playerController;

        private void Awake()
        {
            _playerController = GetComponent<FirstPersonController>();
        }

        /// <summary>
        /// Drops loot when caught by guard.
        /// </summary>
        public void DropLoot(int dividedValue)
        {
            int oldLoot = currentLoot;

            // NOTE(Zack): we're clamping the value of the loot to be positive
            //currentLoot = currentLoot/ dividedValue;
            currentLoot = 0;
            currentLoot = math.max(0, currentLoot);

            // NOTE(Zack): we're clamping the value of the loot to be positive
            int droppingLoot = (int)math.max(0, oldLoot - currentLoot);
            if (droppingLoot > 0)
            {
                GameObject lootBag = Instantiate(_lootBagObj, this.transform.position, Quaternion.identity);
                lootBag.GetComponent<PlayerLootBagDrop>().SetPlayerAndValue(_playerController.playerID, droppingLoot);
            }
        }

        /// <summary>
        /// This is the function that allows player to drop all of their loot as a pick up in the world. 
        /// If the player has more than 0 loot a physical object is dropped in the world which then falls if it is not on the ground.
        /// This object also cannot be placed in a floor or in a wall. 
        /// </summary>
        public void ShareLoot()
        {
            // HACK(Zack): this check is here just for the editor so that we don't get a null reference,
            // when spawning the player in the scene (not transitioning from the MainMenu)
#if UNITY_EDITOR
            if (_playerController == null) return;
#endif

            if (_playerController.GetPlayerInput().shareLoot)
            {
                if (currentLoot > 0)
                {
                    Vector3 dropPoint = this.transform.position + transform.forward * _distanceFromPlayer;
                    if (!Physics.CheckSphere(dropPoint, _sphereRadius, _groundLayer))
                    {
                        _instantiatedLoot = Instantiate(_lootSack, dropPoint, transform.rotation);
                        _instantiatedLoot.GetComponent<DroppedLoot>().SetDroppedLootValue(Mathf.RoundToInt(currentLoot));
                        currentLoot = 0;

                    }
                }
            }
        }

        // NOTE(Zack): we're using integers to simpilfy things as we're rounding to integers in the functions above anyway
        public int TakeLoot(int wantedLoot)
        {
            int newGoldAmount = (int)currentLoot - wantedLoot;
            newGoldAmount = math.max(0, newGoldAmount);

            int stolen = (int)currentLoot - newGoldAmount;
            currentLoot = newGoldAmount;
            return stolen;
        }

        public int GetCurrentLoot()
        {
            return currentLoot;
        }
        
        public int GetMaximumLoot()
        {
            return _maximumLoot;
        }

        public void SetCurrentLoot(float newCurrentLoot)
        {
            // NOTE(Zack): we're clamping the value of the loot to be positive
            currentLoot = math.max(0, (int)newCurrentLoot);
            if (currentLoot > _maximumLoot)
            {
                currentLoot = _maximumLoot;
            }
        }
    }
}


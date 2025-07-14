using GameNetcodeStuff;
using HarmonyLib;
using ShyGuy.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Scopophobia
{
    public class ShyGuyPaintingProp : GrabbableObject
    {
        private bool isTriggered;
        private bool hasSpawned;
        public PlayerControllerB targetPlayer;
        public List<PlayerControllerB> oldTarget = new List<PlayerControllerB>();//change this to a list so we can save more players than just one for each painting.
        private float spawnTimer;
        private float triggeredTime = 15f;
        public AudioSource PaintingSound;

        public void Awake()
        {

        }
        public override void Start()
        {
            base.Start();
            try
            {
                SetVals();
            }
            catch { ScopophobiaPlugin.logger.LogInfo("Failed to Init Shy Guy Painting"); }
        }

        public override void Update()
        {
            base.Update();
            if (IsServer && !isHeld && !IsOwner)
            {
                GetComponent<NetworkObject>().RemoveOwnership();
            }
            if(StartOfRound.Instance.currentLevel.PlanetName.Contains("company") || StartOfRound.Instance.currentLevel.PlanetName.Contains("gordion") || !StartOfRound.Instance.shipHasLanded){ return; }
            if (!isInElevator && !hasSpawned && !isTriggered && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
            {
                if (fallTime >= 1f && !reachedFloorTarget)
                {
                    targetFloorPosition = this.transform.position;
                }
            }

            if (!isTriggered && !hasSpawned)
            {
                if (isHeld || !reachedFloorTarget || fallTime < 1f || isInElevator)
                {
                    base.Update();
                }
                if (IsServer && isHeld && !isTriggered && !oldTarget.Contains(playerHeldBy))
                {
                    targetPlayer = playerHeldBy;
                    isTriggered = true;
                }

            }
            if (IsServer && isTriggered && !hasSpawned)
            {
                if (!targetPlayer.isInHangarShipRoom)
                {
                    if(!hasSpawned)
                    { StartCoroutine(SpawnShyGuyCoroutine(UnityEngine.Random.Range(5,15))); hasSpawned = true; }
                }
            }
        }

        public IEnumerator SpawnShyGuyCoroutine(float spawnDelay)
        {
            if (targetPlayer == null)
            {
                yield break;
            }

            var enemyType = RoundManager.Instance.currentLevel.Enemies.Find(
                x => x.enemyType.enemyName == "Shy guy"
            );

            if (enemyType == null || enemyType.enemyType.enemyPrefab == null)
            {
                yield break;
            }

            yield return new WaitForSeconds(spawnDelay);

            Vector3 spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInRadius(
                targetPlayer.transform.position, 15f, RoundManager.Instance.navHit
            );

            GameObject shyGuy = Instantiate(enemyType.enemyType.enemyPrefab, spawnPos, Quaternion.identity);
            shyGuy.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);

            ShyGuyAI shyGuyAI = shyGuy.GetComponent<ShyGuyAI>();
            if (shyGuyAI == null)
            {
                Debug.LogError("SpawnShyGuy: Could not find ShyGuyAI on prefab.");
                yield break;
            }

            // Set initial behavior state and target
            shyGuyAI.SCP096Targets.Add(targetPlayer);
            shyGuy.GetComponent<ShyGuyAI>().triggerDuration = 15f;
            shyGuyAI.SwitchToBehaviourState(1);

            SetVals();
        }
        public void SetVals()
        {
            hasSpawned = false;
            isTriggered = false;
            if(targetPlayer != null) oldTarget.Add(targetPlayer);
            targetPlayer = null;

        }
        public override void LateUpdate()
        {
            base.LateUpdate();
        }
    }
}

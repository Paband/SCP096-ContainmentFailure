using GameNetcodeStuff;
using ShyGuy.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Scopophobia
{
    public class ShyGuyPaintingProp : GrabbableObject
    {
        [Header("Painting Settings")]
        public List<PlayerControllerB> oldTarget = new List<PlayerControllerB>();//change this to a list so we can save more players than just one for each painting.
        public PlayerControllerB targetPlayer;
        public int randomChance;
        private bool updatedScannode;

        private bool isTriggered;
        public bool hasSpawned;
        private bool isForceSpawn;
        private ScanNodeProperties scanNode;
        public AudioSource PaintingSound;
        private float useCooldown = 30f;

        [Header("Painting Audio")]
        public AudioClip[] PaintingCrySFX;
        public AudioClip[] fearSFX; 
        private float lastUseTime = 0f;

        public override int GetItemDataToSave()
        {
            return base.GetItemDataToSave();
        }
        public void Awake()
        {

        }
        public override void Start()
        {
            base.Start();
            try
            {
                scanNode = GetComponentInChildren<ScanNodeProperties>();
                if (Config.hidePaintingName) 
                { 
                    UpdateScannode(1);
                }
            }
            catch { ScopophobiaPlugin.Instance.LogInfoExtended("Failed to Init Shy Guy Painting"); }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            ScopophobiaPlugin.logger.LogInfo($"Shy Guy Painting Grabbed. Am I Owner?: {IsOwner}"); 
        }
        public void UpdateScannode(int which = 1)
        {
            switch (which)
            {
                case 1: scanNode.headerText = Config.hidePaintingName && !string.IsNullOrWhiteSpace(Config.nameToUseForPainting) ? Config.nameToUseForPainting : "Painting"; break;
                case 2: scanNode.headerText = "Odd Painting of SCP-096"; updatedScannode = true; break;
            }
        }
        public void TriggerFromBeltBag(PlayerControllerB player)
        {
            if (!hasSpawned && !isTriggered && !isHeldByEnemy && oldTarget.Contains(player) && StartOfRound.Instance.shipHasLanded && StartOfRound.Instance.timeSinceRoundStarted >= 2f && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap) return;
            isTriggered = true;
            targetPlayer = player;
            ScopophobiaPlugin.Instance.LogInfoExtended($"Shy Guy Painting triggered by {targetPlayer.playerClientId}");

            randomChance = UnityEngine.Random.Range(0, 101);
            if (randomChance <= Mathf.Clamp(Config.ChanceOfShyGuy, 0, 100))
            {
                PlayAudioFX(fearSFX);
                StartSpawnShyGuy((int)targetPlayer.playerClientId);
                hasSpawned = true;
                ScopophobiaPlugin.Instance.LogInfoExtended("Random chance met, spawning a shy guy");
            }
            else
            {
                PlayAudioFX(PaintingCrySFX);
                ResetSpawnState();
                ScopophobiaPlugin.Instance.LogInfoExtended($"Survived Spawn Attempt. Random chance was: {randomChance}");
                if (IsOwner)
                {
                    HUDManager.Instance.DisplayTip("Shy Guy Painting", "There's an odd Cry emanating from the Belt Bag, better be careful!", false, false, "LC_ShyGuyPaintingTip2");
                }
            }
        }

        private IEnumerator DelayedTriggerFromBeltBag(PlayerControllerB player)
        {
            // Wait a short moment so the bag finishes updating network state
            yield return null; // waits one frame
            yield return new WaitForSeconds(0.2f);

            if (!IsOwner)
            {
                ScopophobiaPlugin.Instance.LogInfoExtended($"[Painting] Ignoring belt bag trigger (not owner).");
                yield break;
            }

            ScopophobiaPlugin.Instance.LogInfoExtended($"[Painting] Delayed trigger firing for {player.playerUsername}");

            targetPlayer = player;
            try
            {
                StartSpawnShyGuy(); // safe to call now
                hasSpawned = true;
            }
            catch (Exception ex)
            {
                ScopophobiaPlugin.Instance.LogErrorExtended($"[Painting] Failed to spawn ShyGuy from belt bag: {ex}");
            }
        }
        private bool CanTriggerPainting()
        {
            return isHeld && !hasSpawned && !isTriggered && !isHeldByEnemy && playerHeldBy != null && IsOwner && !oldTarget.Contains(playerHeldBy) && StartOfRound.Instance.shipHasLanded && StartOfRound.Instance.timeSinceRoundStarted >= 2f && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap;
        }
        public override void Update()
        {
            base.Update();

            // Return early if not held or already completed the effect, or if player is old target, or not owner, ship landed, etc
            if (!CanTriggerPainting()) return;//check at 2f so we can ensure ship has landed fully before triggering
            if (!updatedScannode)
            {
                UpdateScannode(2);//update scannode back to odd painting of SCP
            }
            isTriggered = true;
            targetPlayer = GameNetworkManager.Instance.localPlayerController;
            ScopophobiaPlugin.Instance.LogInfoExtended($"Shy Guy Painting triggered by {targetPlayer.playerUsername}");

            randomChance = UnityEngine.Random.Range(0, 101);
            if (randomChance <= Mathf.Clamp(Config.ChanceOfShyGuy, 0, 100))
            {
                PlayAudioFX(fearSFX);
                StartSpawnShyGuy();
                hasSpawned = true;
                ScopophobiaPlugin.Instance.LogInfoExtended("Random chance met, spawning a shy guy");
            }
            else
            {
                PlayAudioFX(PaintingCrySFX);
                ResetSpawnState();
                ScopophobiaPlugin.Instance.LogInfoExtended("Survived Spawn Attempt");
                if (IsOwner)
                {
                    HUDManager.Instance.DisplayTip("There's an odd sound", "There's an odd sound emanating from the painting, better be careful!", false, false, "LC_ShyGuyPaintingTip1");
                }
            }
        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!IsOwner || isHeldByEnemy) return;
            if (Time.time - lastUseTime < useCooldown) return;
            lastUseTime = Time.time;

            if (!StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap || !StartOfRound.Instance.shipHasLanded) return;
            ScopophobiaPlugin.Instance.LogInfoExtended("Shy Guy Painting used — forcing spawn.");

            isForceSpawn = true;
            targetPlayer = GameNetworkManager.Instance.localPlayerController;

            PlayAudioFX(fearSFX);
            StartSpawnShyGuy();
            StartCoroutine(ResetPaintingCooldown());
        }
        private IEnumerator ResetPaintingCooldown()
        {
            yield return new WaitForSeconds(useCooldown);
            isForceSpawn = false;
            targetPlayer = null;

            ScopophobiaPlugin.Instance.LogInfoExtended("Painting Triggers Reset, Can Spawn Again");
        }
       
        public void PlayAudioFX(AudioClip[] clip)
        {
            if (PaintingSound == null) return;
            if (clip == null) return;
            int num = UnityEngine.Random.Range(0, clip.Length);
            PaintingSound.clip = clip[num];
            PaintingSound.volume = 0.3f;
            PaintingSound.Play();
        }

        public void StartSpawnShyGuy(int? explicitTargetClientId = null)
        {
            if (((NetworkBehaviour)this).IsServer)
            {
                int targetId = explicitTargetClientId ?? ((int)base.playerHeldBy.playerClientId);
                SpawnEnemyOnServer(targetId);
            }
            else
            {
                SpawnEnemyServerRpc();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnEnemyServerRpc(ServerRpcParams rpcParams = default(ServerRpcParams))
        {
            int triggeringClientId = (int)rpcParams.Receive.SenderClientId;
            SpawnEnemyOnServer(triggeringClientId);
        }

        public void ResetSpawnState()
        {
            hasSpawned = false;
            isTriggered = false;
            if ((UnityEngine.Object)(object)targetPlayer != null && !oldTarget.Contains(targetPlayer))
            {
                oldTarget.Add(targetPlayer);
            }
            targetPlayer = null;
            randomChance = 0;
        }

        public void SpawnEnemyOnServer(int targetClientId)
        {
            PlayerControllerB target = StartOfRound.Instance.allPlayerScripts[targetClientId];
            Vector3 spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInRadius(target.transform.position, 15f, RoundManager.Instance.navHit);
            ScopophobiaPlugin.Instance.LogInfoExtended($"[SpawnEnemyOnServer] Triggered by client {targetClientId} ({StartOfRound.Instance.allPlayerScripts[targetClientId].playerUsername})");
            SpawnableEnemyWithRarity enemy = RoundManager.Instance.currentLevel.Enemies.Find((SpawnableEnemyWithRarity x) => x.enemyType.enemyName.ToLower() == "shy guy");
            if (enemy == null)
            {
                ScopophobiaPlugin.Instance.LogInfoExtended("Shy Guy Enemy Not found");
                return;
            }
            GameObject obj = Instantiate(enemy.enemyType.enemyPrefab, spawnPos, Quaternion.identity);
            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                ScopophobiaPlugin.Instance.LogInfoExtended("Found Network Object");
            }
            netObj.SpawnWithOwnership(StartOfRound.Instance.allPlayerScripts[0].actualClientId, destroyWithScene: true);
            ShyGuyAI ai = obj.GetComponentInChildren<ShyGuyAI>();
            ai.SwitchToBehaviourState(1);
            StartCoroutine(InitializeAI(ai, target));
        }

        private IEnumerator InitializeAI(ShyGuyAI ai, PlayerControllerB target)
        {
            yield return new WaitForSeconds(Config.triggerTime);//delay by trigger

            ai.ChangeOwnershipOfEnemy(target.actualClientId);
            ai.AddTargetToList((int)target.actualClientId, false);
            ResetSpawnState();
        }
    }
}

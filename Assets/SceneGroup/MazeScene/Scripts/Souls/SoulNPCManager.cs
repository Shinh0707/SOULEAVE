using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SL.Lib
{
    public class SoulNPCManager : DynamicObject
    {
        [SerializeField] private GameObject soulNPCPrefab;
        private int totalSouls;
        private int currentSouls;
        private int eatenSouls;
        public int TotalSouls => totalSouls;
        public int EatenSouls => eatenSouls;
        public int CurrentSouls => currentSouls;
        public int SavedSouls => totalSouls - currentSouls - eatenSouls;
        private List<SoulNPCController> soulNPCs = new();

        public void Initialize()
        {
            totalSouls = 0;
            currentSouls = 0;
            eatenSouls = 0;
        }
        public override void UpdateState()
        {
            foreach (var soulNpc in soulNPCs)
            {
                soulNpc.UpdateState();
            }
            var delSouls = soulNPCs.Where(s => s.IsDead).ToList();
            soulNPCs = soulNPCs.Where(s => !s.IsDead).ToList();
            var maxSouls = delSouls.Count();
            for(int i = 0;i < maxSouls; i++)
            {
                currentSouls--;
                if (!delSouls[i].EatenByPlayer)
                {
                    eatenSouls++;
                }
                Destroy(delSouls[i].gameObject);
                delSouls[i] = null;
            }
        }
        public void HandleInput()
        {
            foreach (var soulNpc in soulNPCs)
            {
                soulNpc.HandleInput();
            }
        }

        public void InitializeSouls(List<Vector2> positions, (int, int) mazeSize)
        {
            foreach (var position in positions)
            {
                SpawnSoul(position, mazeSize);
            }
        }

        private void SpawnSoul(Vector2 position, (int, int) mazeSize)
        {
            GameObject soulNpcObject = Instantiate(soulNPCPrefab, position, Quaternion.identity);
            var soulNpc = soulNpcObject.GetComponent<SoulNPCController>();
            soulNpc.InitializeStatus(new CharacterStatus(

                PlayerStatusManager.MaxIntensity * ((float)SLRandom.Random.NextDouble() + 0.5f),
                PlayerStatusManager.RestoreIntensityPerSecond * ((float)SLRandom.Random.NextDouble() + 0.5f),
                1,
                1,
                PlayerStatusManager.MaxSpeed * ((float)SLRandom.Random.NextDouble() + 0.5f),
                0.5f
            ));
            soulNpc.Initialize(position, mazeSize);
            soulNPCs.Add(soulNpc);
            totalSouls++;
            currentSouls++;
        }

        private void SpawnNewSoul()
        {
            Vector2 spawnPosition = MazeGameScene.Instance.MazeManager.GetRandomPosition();
            SpawnSoul(spawnPosition, MazeGameScene.Instance.MazeManager.mazeSize);
        }
    }
}
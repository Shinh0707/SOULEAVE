using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SL.Lib
{
    public class SoulNPCManager : DynamicObject
    {
        [SerializeField] private GameObject soulNPCPrefab;
        private List<SoulNPCController> soulNPCs = new();

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
            soulNpc.InitializeStatus(new SoulNPCStatus
            {
                MaxMP = 1,
                MaxIntensity = PlayerStatusManager.MaxIntensity * ((float)SLRandom.Random.NextDouble() + 0.5f),
                RestoreMPPerSecond = 1,
                RestoreIntensityPerSecond = PlayerStatusManager.RestoreIntensityPerSecond * ((float)SLRandom.Random.NextDouble() + 0.5f),
            });
            soulNpc.Initialize(position, mazeSize);
            soulNPCs.Add(soulNpc);
        }

        private void SpawnNewSoul()
        {
            Vector2 spawnPosition = MazeGameScene.Instance.MazeManager.GetRandomPosition();
            SpawnSoul(spawnPosition, MazeGameScene.Instance.MazeManager.mazeSize);
        }

        public void RemoveSoul(SoulNPCController soulNpc)
        {
            soulNPCs.Remove(soulNpc);
            Destroy(soulNpc.gameObject);
        }
    }
}
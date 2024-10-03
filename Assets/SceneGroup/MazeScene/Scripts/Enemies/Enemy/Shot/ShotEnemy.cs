using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SL.Lib
{
    public class ShotEnemy : EnemyController
    {
        [SerializeField] private int maxNeedles = 2;
        [SerializeField] private GameObject NeedlePrefab;
        private Dictionary<string, GameObject> needles = new();
        public enum ShotState
        {
            Stay,
            Searching
        }
        protected override void Think()
        {
            if (needles.Count < maxNeedles)
            {
                var soul = SearchAround();
                if (soul != null)
                {
                    Debug.Log($"{soul.Name} was founded!");
                    var needleObject = Instantiate(NeedlePrefab, character.transform);
                    Debug.Log($"Create Needle!");
                    var needle = needleObject.GetComponent<Needle>();
                    if (needle != null)
                    {
                        needle.Initialize(Position);
                        needle.SetTarget(soul.ID);
                        needles[soul.ID] = needleObject;
                        needle.StartTargeting(3);
                    }
                }
            }
            var removes = needles.Where(ndle => ndle.Value == null).Select(ndle => ndle.Key).ToList();
            foreach (var key in removes) 
            {
                needles.Remove(key);
            }
        }

        private ISoulController SearchAround()
        {
            var targets = SoulControllerManager.InstantinatedControllers.Values.Where(soul => (!needles.ContainsKey(soul.ID))&&(Vector2.Distance(soul.Position, Position) <= SightRange)).OrderBy(soul => Vector2.Distance(soul.Position, Position));
            RaycastHit2D hit;
            foreach (var target in targets)
            {
                hit = Physics2D.Raycast(Position, (target.Position - Position).normalized, Vector2.Distance(target.Position, Position), LayerMask.GetMask("Wall"));
                if (hit.collider == null)
                {
                    return target;
                }
            }
            return null;
        }
    }
}
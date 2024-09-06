using SL.Lib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PathVisualizeEffect", menuName = "SkillSystem/Effects/PathVisualizeEffect")]
public class PathVisualizeEffectUnit : EffectUnit
{
    public int maxLevel = 10;
    public float showTime = 10f;
    public GameObject PathObject;
    private PathFinder pathFinder;
    private GameObject pathParent;
    private Queue<GameObject> activePathObjects = new();
    private Queue<GameObject> unactivePathObjects = new();
    public override IEnumerator ApplyEffect(PlayerController player, int level, KeyCode triggerKey)
    {
        while (Input.GetKeyDown(triggerKey))
        {
            yield return null;
        }
        pathFinder = new(MazeGameScene.Instance.MazeManager.GetBaseMap() == 0);
        var path = pathFinder.FindPath(MazeGameScene.Instance.MazeManager.GetTensorPosition(player.transform.position), MazeGameScene.Instance.MazeManager.GoalTensorPosition, Mathf.Clamp01((float)level / maxLevel));
        if (path != null)
        {
            Success = true;
            if (pathParent == null)
            {
                pathParent = new GameObject("PathParent");
                while (pathParent == null)
                {
                    yield return null;
                }
            }
            foreach (var position in path.Select(p => MazeGameScene.Instance.MazeManager.GetWorldPosition(p))) 
            {
                if (unactivePathObjects.Count == 0)
                {
                    activePathObjects.Enqueue(Instantiate(PathObject, position, Quaternion.identity, pathParent.transform));
                }
                else
                {
                    var obj = unactivePathObjects.Dequeue();
                    obj.transform.position = position;
                    obj.transform.rotation = Quaternion.identity;
                    obj.SetActive(true);
                    activePathObjects.Enqueue(obj);
                }
            }
            yield return new WaitForGameSeconds(showTime);
            while(activePathObjects.Count > 0) 
            {
                var obj = activePathObjects.Dequeue();
                obj.SetActive(false);
                unactivePathObjects.Enqueue(obj);
            }
        }
        else
        {
            Success = false;
        }
        
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using SL.Lib;

public class SkillTreeLayout : MonoBehaviour
{
    public RectTransform targetScreenCanvas;
    public GameObject skillNodePrefab;
    public RectTransform skillNodeArea;
    public TextMeshProUGUI zoomTextMesh;
    public float nodeSpacing = 200f;
    public float levelSpacing = 300f;
    public Material LineMaterial;
    public Gradient unmetRequirementColor;
    public Gradient metRequirementColor;

    private Dictionary<SelectableSkillName, SkillNodeInfo> skillNodes = new();
    private Dictionary<SelectableSkillName, SkillNodeUI> activeNodes = new();
    private Queue<SkillNodeUI> inactiveNodes = new Queue<SkillNodeUI>();
    private Camera mainCamera;

    public float cameraMoveSpeed = 500f;
    public float minZoom = 0.5f;
    public float maxZoom = 2f;
    public float zoomSpeed = 0.1f;
    public float boundaryPadding = 200f;
    public float snapThreshold = 100f;
    public float snapSpeed = 5f;
    public float centeringSpeed = 5f;

    private Vector2 cameraDragStart;
    private bool isDragging = false;
    private float currentZoom = 1f;
    private Vector2 virtualCameraPosition;
    private Vector2 virtualScreenCenter;
    private float screenOffset = 0f;

    private SLCoroutine centeringCoroutine;
    private SLCoroutine snapCoroutine;
    private GameObject lineDrawer;
    private Dictionary<(SelectableSkillName, SelectableSkillName),LineRenderer> lineRenderers = new();
    private class SkillNodeInfo
    {
        public Vector2 VirtualPosition;
        public List<SelectableSkillName> DependentNodes = new();
        public List<SelectableSkillName> RequiredNodes = new();
        public int Depth;
        public bool IsRoot = false;
    }
    public IEnumerator Initialize()
    {
        mainCamera = Camera.main;
        centeringCoroutine = new();
        snapCoroutine = new();
        var button = skillNodeArea.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleBackgroundClick);
        BuildSkillTree();
        LayoutSkillTree();
        UpdateVisibleNodes();
        yield return null;
        SkillTreeScene.Instance.State = SkillTreeSceneState.SkillSelect;
    }

    private void Update()
    {
        if (SkillTreeScene.Instance.IsInitialized)
        {
            UpdateScreenOffset();
            UpdateVirtualScreenCenter();
            HandleInput();
            UpdateVisibleNodes();
        }
    }

    private void HandleInput()
    {
        if (!centeringCoroutine.IsBusy)
        {
            HandleCameraMovement();
            HandleCameraZoom();
            if (Input.GetMouseButtonUp(0))
            {
                SnapToNearestNode();
            }
        }
    }

    private void HandleCameraMovement()
    {
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * cameraMoveSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            cameraDragStart = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentMousePos = Input.mousePosition;
            movement -= (currentMousePos - cameraDragStart) * currentZoom;
            cameraDragStart = currentMousePos;
        }
        if(snapCoroutine.IsBusy && (isDragging || movement.sqrMagnitude > 0.0f))
        {
            snapCoroutine.Stop();
        }

        virtualCameraPosition += movement;
        virtualCameraPosition = LimitCameraPosition(virtualCameraPosition);
    }

    private void HandleCameraZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            float prevZoom = currentZoom;
            currentZoom -= scrollDelta * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            Vector2 mousePositionBeforeZoom = ScreenToVirtualPosition(Input.mousePosition);
            Vector2 mousePositionAfterZoom = ScreenToVirtualPosition(Input.mousePosition);
            virtualCameraPosition += mousePositionBeforeZoom - mousePositionAfterZoom;
            zoomTextMesh.text = $"x{1 / currentZoom:F2}";
        }
    }

    private void SnapToNearestNode()
    {
        SelectableSkillName nearestNode = FindNearestNode();
        if (nearestNode != null)
        {
            Vector2 nodePosition = skillNodes[nearestNode].VirtualPosition;
            float distance = Vector2.Distance(nodePosition, virtualCameraPosition);

            if (distance < snapThreshold)
            {
                snapCoroutine.RunOrRestart(this, LerpMoveToTarget(nodePosition, snapSpeed));
            }
        }
    }
    public void CenterOnNode(SelectableSkillName skillName)
    {
        Vector2 nodePosition = skillNodes[skillName].VirtualPosition;
        if (snapCoroutine != null)
        {
            snapCoroutine.Stop();
        }
        isDragging = false;
        centeringCoroutine.RunOrRestart(this, LerpMoveToTarget(nodePosition, centeringSpeed));
    }

    private IEnumerator LerpMoveToTarget(Vector2 targetPosition, float speed)
    {
        while (Vector2.Distance(virtualCameraPosition, targetPosition) > 0.1f)
        {
            virtualCameraPosition = Vector2.Lerp(virtualCameraPosition, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        virtualCameraPosition = targetPosition;
    }

    private SelectableSkillName FindNearestNode()
    {
        return skillNodes.OrderBy(n => Vector2.Distance(n.Value.VirtualPosition, virtualCameraPosition))
            .FirstOrDefault().Key;
    }

    private void UpdateScreenOffset()
    {
        RectTransform dialogRect = SkillTreeScene.Instance.DialogManager.DialogRect;
        if (dialogRect != null)
        {
            screenOffset = dialogRect.sizeDelta.x - dialogRect.anchoredPosition.x;
        }
        else
        {
            screenOffset = 0f;
        }
    }

    private void UpdateVirtualScreenCenter()
    {
        float visibleWidth = targetScreenCanvas.sizeDelta.x - screenOffset;
        virtualScreenCenter = new Vector2(visibleWidth / 2f, targetScreenCanvas.sizeDelta.y / 2f);
    }

    

    private void HandleBackgroundClick()
    {
        SkillTreeScene.Instance.DialogManager.CloseDialog();
    }

    private Vector2 ScreenToVirtualPosition(Vector2 screenPosition)
    {
        return (screenPosition - virtualScreenCenter) * currentZoom + virtualCameraPosition;
    }

    private Vector2 VirtualToScreenPosition(Vector2 virtualPosition)
    {
        return (virtualPosition - virtualCameraPosition) / currentZoom + virtualScreenCenter;
    }
    private Vector2 VirtualToWorldPosition(Vector2 virtualPosition)
    {
        var pos = mainCamera.ScreenToWorldPoint(VirtualToScreenPosition(virtualPosition));
        pos.z = -1f;
        return pos;
    }

    private void BuildSkillTree()
    {
        var allSkillNames = SkillTree.Instance.GetAllSkillNames();

        foreach (string skillName in allSkillNames)
        {
            if (!skillNodes.ContainsKey(skillName))
            {
                skillNodes[skillName] = new SkillNodeInfo { Depth = 0 };
            }

            Skill skill = SkillTree.Instance.GetSkill(skillName);
            if (skill.data.IsRoot)
            {
                skillNodes[skillName].IsRoot = true;
            }
            foreach (var req in skill.data.requirements)
            {
                skillNodes[skillName].RequiredNodes.Add(req.skillName);
                if (!skillNodes.ContainsKey(req.skillName))
                {
                    skillNodes[req.skillName] = new SkillNodeInfo { Depth = 0 };
                }
                skillNodes[req.skillName].DependentNodes.Add(skillName);
            }
        }

        AssignDepths();
    }

    private void AssignDepths()
    {
        var rootNodes = skillNodes.Where(kv => kv.Value.IsRoot).Select(kv => kv.Key).ToList();

        foreach (var rootNode in rootNodes)
        {
            AssignDepthRecursive(rootNode, 0);
        }
    }

    private void AssignDepthRecursive(SelectableSkillName nodeName, int depth)
    {
        var node = skillNodes[nodeName];
        node.Depth = Mathf.Max(node.Depth, depth);

        foreach (var dependentNode in node.DependentNodes)
        {
            AssignDepthRecursive(dependentNode, depth + 1);
        }
    }

    private void LayoutSkillTree()
    {
        var depthNodes = skillNodes.GroupBy(kv => kv.Value.Depth)
                                   .OrderByDescending(g => g.Key)
                                   .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

        float yOffset = 0;
        foreach (var depth in depthNodes.Keys)
        {
            LayoutNodesAtDepth(depthNodes[depth], yOffset);
            yOffset += levelSpacing;
        }

        AdjustNodePositions();
    }

    private void LayoutNodesAtDepth(List<SelectableSkillName> nodes, float yOffset)
    {
        float xOffset = -(nodes.Count - 1) * nodeSpacing / 2;
        foreach (var nodeName in nodes)
        {
            skillNodes[nodeName].VirtualPosition = new Vector2(xOffset, yOffset);
            xOffset += nodeSpacing;
        }
    }

    private void AdjustNodePositions()
    {
        foreach (var node in skillNodes.Values.OrderBy(n => n.Depth))
        {
            if (node.RequiredNodes.Count > 0)
            {
                float avgX = node.RequiredNodes.Average(n => skillNodes[n].VirtualPosition.x);
                node.VirtualPosition = new Vector2(avgX, node.VirtualPosition.y);
            }

            if (node.DependentNodes.Count == 1)
            {
                var dependentNode = skillNodes[node.DependentNodes[0]];
                if (dependentNode.RequiredNodes.Count == 1)
                {
                    dependentNode.VirtualPosition = new Vector2(node.VirtualPosition.x, dependentNode.VirtualPosition.y);
                }
            }
        }
    }

    private void UpdateVisibleNodes()
    {
        Rect viewportRect = GetViewportRect();

        List<SelectableSkillName> nodesToDeactivate = new();
        foreach (var pair in activeNodes)
        {
            Vector2 screenPosition = VirtualToScreenPosition(skillNodes[pair.Key].VirtualPosition);
            if (!viewportRect.Contains(screenPosition))
            {
                nodesToDeactivate.Add(pair.Key);
            }
        }

        foreach (SelectableSkillName skillName in nodesToDeactivate)
        {
            SkillNodeUI nodeUI = activeNodes[skillName];
            nodeUI.gameObject.SetActive(false);
            inactiveNodes.Enqueue(nodeUI);
            activeNodes.Remove(skillName);
        }

        foreach (var pair in skillNodes)
        {
            Vector2 screenPosition = VirtualToScreenPosition(pair.Value.VirtualPosition);
            if (viewportRect.Contains(screenPosition) && !activeNodes.ContainsKey(pair.Key))
            {
                SkillNodeUI nodeUI;
                if (inactiveNodes.Count > 0)
                {
                    nodeUI = inactiveNodes.Dequeue();
                    nodeUI.gameObject.SetActive(true);
                }
                else
                {
                    var obj = Instantiate(skillNodePrefab, skillNodeArea);
                    nodeUI = obj.GetComponent<SkillNodeUI>();
                }

                nodeUI.Initialize(pair.Key);
                nodeUI.GetComponent<RectTransform>().anchoredPosition = screenPosition;
                nodeUI.transform.localScale = Vector3.one / currentZoom;
                activeNodes[pair.Key] = nodeUI;
            }
            else if (activeNodes.TryGetValue(pair.Key, out SkillNodeUI nodeUI))
            {
                nodeUI.Initialize(pair.Key);
                nodeUI.GetComponent<RectTransform>().anchoredPosition = screenPosition;
                nodeUI.transform.localScale = Vector3.one / currentZoom;
            }
        }
        UpdateNodeConnections();
    }

    private void UpdateNodeConnections()
    {
        float defaultThickness = 0.5f;
        float currentThickness = defaultThickness / currentZoom;
        Dictionary<SelectableSkillName, Vector2> nodeWorldPositions = new(); 
        Vector3 GetNodeWorldPosition(SelectableSkillName skillName)
        {
            if(!nodeWorldPositions.TryGetValue(skillName, out Vector2 worldPos))
            {
                worldPos = VirtualToWorldPosition(skillNodes[skillName].VirtualPosition);
                nodeWorldPositions[skillName] = worldPos;
            }
            return worldPos;
        }
        var reusableLines = new Queue<(SelectableSkillName,SelectableSkillName)>(lineRenderers.Keys.Where(l => !activeNodes.ContainsKey(l.Item1) && !activeNodes.ContainsKey(l.Item2)).ToArray());
        if (lineDrawer == null)
        {
            lineDrawer = new GameObject("LineDrawer");
        }
        void AddLine(SelectableSkillName req, SelectableSkillName dep)
        {
            var key = (req, dep);
            if (!lineRenderers.TryGetValue(key, out LineRenderer line))
            {
                var revKey = (dep, req);
                if (!lineRenderers.TryGetValue(revKey, out line)) {
                    if (reusableLines.Count > 0)
                    {
                        var reusableLine = reusableLines.Dequeue();
                        line = lineRenderers[reusableLine];
                        lineRenderers.Remove(reusableLine);
                    }
                    else
                    {
                        line = lineDrawer.AddComponent<LineRenderer>();
                        line.material = LineMaterial;
                        line.useWorldSpace = true;
                        line.sortingOrder = -1;
                        line.alignment = LineAlignment.TransformZ;
                    }
                    lineRenderers.Add(key, line);
                }
            }
            UpdateLineRenderer(line, req, dep);
        }
        void UpdateLineRenderer(LineRenderer lineRenderer, SelectableSkillName req, SelectableSkillName dep)
        {
            lineRenderer.startWidth = currentThickness;
            lineRenderer.endWidth = currentThickness;
            lineRenderer.colorGradient = SkillTree.Instance.GetSkill(req).data.IsRequirementMet(dep)? metRequirementColor:unmetRequirementColor;
            var positions = lineRenderer.colorGradient.InterpolateAlongGradient(GetNodeWorldPosition(req), GetNodeWorldPosition(dep));
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
        foreach (var pair in activeNodes)
        {
            SelectableSkillName skillName = pair.Key;
            foreach (SelectableSkillName requiredNodeName in skillNodes[skillName].RequiredNodes)
            {
                AddLine(skillName, requiredNodeName);
            }
            foreach (SelectableSkillName dependedNodeName in skillNodes[skillName].DependentNodes)
            {
                AddLine(dependedNodeName, skillName);
            }
        }
    }
    private Rect GetViewportRect()
    {
        return new Rect(0, 0, targetScreenCanvas.sizeDelta.x, targetScreenCanvas.sizeDelta.y);
    }

    private Vector2 LimitCameraPosition(Vector2 position)
    {
        Vector2 minPosition = skillNodes.Values.Aggregate((a, b) => a.VirtualPosition.x < b.VirtualPosition.x ? a : b).VirtualPosition;
        Vector2 maxPosition = skillNodes.Values.Aggregate((a, b) => a.VirtualPosition.x > b.VirtualPosition.x ? a : b).VirtualPosition;

        float aspectRatio = (float)(targetScreenCanvas.sizeDelta.x) / targetScreenCanvas.sizeDelta.y;
        float verticalSize = targetScreenCanvas.sizeDelta.y / (2f * currentZoom);
        float horizontalSize = verticalSize * aspectRatio;

        minPosition -= new Vector2(horizontalSize + boundaryPadding, verticalSize + boundaryPadding);
        maxPosition += new Vector2(horizontalSize + boundaryPadding, verticalSize + boundaryPadding);

        return new Vector2(
            Mathf.Clamp(position.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(position.y, minPosition.y, maxPosition.y)
        );
    }
}
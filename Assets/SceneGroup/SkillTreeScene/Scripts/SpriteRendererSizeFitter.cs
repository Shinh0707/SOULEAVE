using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRendererSizeFitter : MonoBehaviour
{
    public SpriteRenderer targetSpriteRenderer;
    public SpriteRenderer resizeSpriteRenderer;

    public enum FitMode
    {
        Clip,
        StretchToFit,
        FitWidth,
        FitHeight,
        FitLongerSide,
        FitShorterSide
    }

    public FitMode fitMode = FitMode.Clip;

    [System.Flags]
    public enum FitAnchor
    {
        Center = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    }

    public FitAnchor fitAnchor = FitAnchor.Center;

    public void SizeFit()
    {
        if (targetSpriteRenderer == null || resizeSpriteRenderer == null)
        {
            Debug.LogError("Target or resize SpriteRenderer is not set.");
            return;
        }

        Vector2 targetSize = targetSpriteRenderer.bounds.size;
        Vector2 originalSize = resizeSpriteRenderer.bounds.size;
        Vector2 newSize = originalSize;

        switch (fitMode)
        {
            case FitMode.Clip:
                // Do nothing, keep original size
                break;
            case FitMode.StretchToFit:
                newSize = targetSize;
                break;
            case FitMode.FitWidth:
                float widthRatio = targetSize.x / originalSize.x;
                newSize = new Vector2(targetSize.x, originalSize.y * widthRatio);
                break;
            case FitMode.FitHeight:
                float heightRatio = targetSize.y / originalSize.y;
                newSize = new Vector2(originalSize.x * heightRatio, targetSize.y);
                break;
            case FitMode.FitLongerSide:
                if (targetSize.x / originalSize.x > targetSize.y / originalSize.y)
                    newSize = new Vector2(targetSize.x, originalSize.y * (targetSize.x / originalSize.x));
                else
                    newSize = new Vector2(originalSize.x * (targetSize.y / originalSize.y), targetSize.y);
                break;
            case FitMode.FitShorterSide:
                if (targetSize.x / originalSize.x < targetSize.y / originalSize.y)
                    newSize = new Vector2(targetSize.x, originalSize.y * (targetSize.x / originalSize.x));
                else
                    newSize = new Vector2(originalSize.x * (targetSize.y / originalSize.y), targetSize.y);
                break;
        }

        resizeSpriteRenderer.transform.localScale = new Vector3(
            newSize.x / resizeSpriteRenderer.sprite.bounds.size.x,
            newSize.y / resizeSpriteRenderer.sprite.bounds.size.y,
            1
        );

        Vector3 positionOffset = Vector3.zero;

        if (fitAnchor.HasFlag(FitAnchor.Left))
            positionOffset.x = (targetSize.x - newSize.x) / 2;
        else if (fitAnchor.HasFlag(FitAnchor.Right))
            positionOffset.x = -(targetSize.x - newSize.x) / 2;

        if (fitAnchor.HasFlag(FitAnchor.Top))
            positionOffset.y = -(targetSize.y - newSize.y) / 2;
        else if (fitAnchor.HasFlag(FitAnchor.Bottom))
            positionOffset.y = (targetSize.y - newSize.y) / 2;

        resizeSpriteRenderer.transform.position = targetSpriteRenderer.transform.position + positionOffset;
    }
}
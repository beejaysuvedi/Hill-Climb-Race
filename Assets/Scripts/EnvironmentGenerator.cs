using UnityEngine;
using UnityEngine.U2D;

public class EnvironmentGenerator : MonoBehaviour
{
    [SerializeField] private SpriteShapeController spriteShapeController;
    [SerializeField] private Transform player;

    [Header("Road Settings")]
    [SerializeField, Min(40)] private int levelLength = 100;
    [SerializeField, Min(1f)] private float xMultiplier = 2f;
    [SerializeField, Min(0f)] private float yMultiplier = 4f;
    [SerializeField, Range(0f, 1f)] private float curveSmoothness = 0.5f;
    [SerializeField, Min(0.001f)] private float noiseStep = 0.08f;
    [SerializeField] private float bottom = -15f;

    private int firstPoint;

    private void Start()
    {
        if (spriteShapeController == null)
            spriteShapeController = GetComponent<SpriteShapeController>();

        if (spriteShapeController == null || player == null)
        {
            Debug.LogError(
                "EnvironmentGenerator: Assign Sprite Shape Controller and Player.",
                this
            );

            enabled = false;
            return;
        }

        // Keep existing Inspector values within safe limits.
        levelLength = Mathf.Max(40, levelLength);
        xMultiplier = Mathf.Max(1f, xMultiplier);
        yMultiplier = Mathf.Max(0f, yMultiplier);
        noiseStep = Mathf.Max(0.001f, noiseStep);
        curveSmoothness = Mathf.Clamp01(curveSmoothness);
        bottom = Mathf.Min(-5f, bottom);

        spriteShapeController.spline.isOpenEnded = false;
        spriteShapeController.autoUpdateCollider = true;

        int playerPoint = GetPlayerPoint();
        firstPoint = Mathf.Max(0, playerPoint - levelLength / 4);

        GenerateLevel();
    }

    private void FixedUpdate()
    {
        int playerPoint = GetPlayerPoint();

        // Extend the road while the car is still far from its edge.
        if (playerPoint >= firstPoint + levelLength / 2)
        {
            firstPoint = Mathf.Max(0, playerPoint - levelLength / 4);
            GenerateLevel();
        }
    }

    private int GetPlayerPoint()
    {
        Vector3 localPosition =
            spriteShapeController.transform.InverseTransformPoint(
                player.position
            );

        return Mathf.FloorToInt(localPosition.x / xMultiplier);
    }

    private float GetHeight(int point)
    {
        // A flat starting area, followed by gradually increasing hills.
        float hillStrength = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.InverseLerp(10f, 25f, point)
        );

        float noise = Mathf.PerlinNoise(point * noiseStep, 0f);

        return noise * yMultiplier * hillStrength;
    }

    private void GenerateLevel()
    {
        Spline spline = spriteShapeController.spline;
        spline.Clear();

        // Limit tangent length so curves cannot fold backwards.
        float tangentLength = Mathf.Min(
            curveSmoothness,
            xMultiplier / 3f
        );

        for (int i = 0; i < levelLength; i++)
        {
            int point = firstPoint + i;

            spline.InsertPointAt(
                i,
                new Vector3(
                    point * xMultiplier,
                    GetHeight(point),
                    0f
                )
            );

            bool isEnd = i == 0 || i == levelLength - 1;

            if (isEnd || tangentLength <= 0f)
            {
                spline.SetTangentMode(i, ShapeTangentMode.Linear);
            }
            else
            {
                spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                spline.SetLeftTangent(
                    i, Vector3.left * tangentLength
                );
                spline.SetRightTangent(
                    i, Vector3.right * tangentLength
                );
            }
        }

        float leftX = firstPoint * xMultiplier;
        float rightX = (firstPoint + levelLength - 1) * xMultiplier;

        int bottomRight = spline.GetPointCount();
        spline.InsertPointAt(
            bottomRight, new Vector3(rightX, bottom, 0f)
        );
        spline.SetTangentMode(bottomRight, ShapeTangentMode.Linear);

        int bottomLeft = spline.GetPointCount();
        spline.InsertPointAt(
            bottomLeft, new Vector3(leftX, bottom, 0f)
        );
        spline.SetTangentMode(bottomLeft, ShapeTangentMode.Linear);

        // Update the visible ground and its collision surface together.
        spriteShapeController.RefreshSpriteShape();
        spriteShapeController.BakeMesh().Complete();
        spriteShapeController.BakeCollider();
    }
}
using UnityEngine;
using UnityEngine.U2D;

[ExecuteAlways]
public class EnvironmentGenerator : MonoBehaviour
{
    [SerializeField] private SpriteShapeController spriteShapeController;

    [SerializeField] private int levelLength = 100;
    [SerializeField] private float xMultiplier = 1f;
    [SerializeField] private float yMultiplier = 1f;
    [SerializeField] private float curveSmoothness = 0.5f;
    [SerializeField] private float noiseStep = 0.5f;
    [SerializeField] private float bottom = -10f;

    private Vector3 lastPosition;

    private void OnValidate()
    {
        if (spriteShapeController == null)
            return;

        GenerateLevel();
    }

    private void GenerateLevel()
    {
        Spline spline = spriteShapeController.spline;

        // Remove existing points
        spline.Clear();

        lastPosition = Vector3.zero;

        // Top-left point
        spline.InsertPointAt(0, new Vector3(0, 0, 0));
        spline.SetTangentMode(0, ShapeTangentMode.Linear);

        // Generate terrain
        for (int i = 1; i < levelLength; i++)
        {
            float x = i * xMultiplier;

            float noise = Mathf.PerlinNoise(i * noiseStep, 0f);
            float y = noise * yMultiplier;

            Vector3 position = new Vector3(x, y, 0);

            spline.InsertPointAt(i, position);

            if (i != levelLength - 1)
            {
                spline.SetTangentMode(i, ShapeTangentMode.Continuous);

                spline.SetLeftTangent(
                    i,
                    Vector3.left * curveSmoothness
                );

                spline.SetRightTangent(
                    i,
                    Vector3.right * curveSmoothness
                );
            }

            lastPosition = position;
        }

        // Bottom-right point
        int bottomRightIndex = spline.GetPointCount();

        spline.InsertPointAt(
            bottomRightIndex,
            new Vector3(lastPosition.x, bottom, 0)
        );

        spline.SetTangentMode(
            bottomRightIndex,
            ShapeTangentMode.Linear
        );

        // Bottom-left point
        int bottomLeftIndex = spline.GetPointCount();

        spline.InsertPointAt(
            bottomLeftIndex,
            new Vector3(0, bottom, 0)
        );

        spline.SetTangentMode(
            bottomLeftIndex,
            ShapeTangentMode.Linear
        );

        spriteShapeController.RefreshSpriteShape();
    }
}
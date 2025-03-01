using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType { Path, Wall, BorderWall }
    public TileType type = TileType.Path;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateColor();
    }

    public void ChangeTileType(TileType newType)
    {
        if (type == TileType.BorderWall) return;
        type = newType;
        UpdateColor();
    }

    public void SetAsBorderWall()
    {
        type = TileType.BorderWall;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (sr == null) return;

        switch (type)
        {
            case TileType.Path:
                sr.color = Color.green;
                break;
            case TileType.Wall:
                sr.color = Color.black;
                break;
            case TileType.BorderWall:
                sr.color = Color.black;
                break;
        }
    }
}

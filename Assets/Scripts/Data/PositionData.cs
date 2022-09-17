
using System.Numerics;

public struct PositionData
{
    public float x;
    public float y;

    public override string ToString()
    {
        return new Vector2(x, y).ToString();
    }
}

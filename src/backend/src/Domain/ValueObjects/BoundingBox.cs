namespace FamilyTree.Domain.ValueObjects;

using backend.Domain.Common;

public class BoundingBox : ValueObject
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private BoundingBox() { } // For EF Core

    public BoundingBox(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
        yield return Width;
        yield return Height;
    }
}

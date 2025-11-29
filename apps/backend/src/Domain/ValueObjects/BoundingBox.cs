namespace backend.Domain.ValueObjects;

public class BoundingBox : ValueObject
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
        yield return Width;
        yield return Height;
    }
}
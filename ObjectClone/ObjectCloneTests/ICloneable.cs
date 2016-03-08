namespace ObjectCloneTests
{
    /// <summary>
    ///     Generic ICloneable interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICloneable<T>
    {
        T Clone();
    }
}
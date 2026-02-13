namespace EF.Generic
{
    public class DataResult<T>
    {
        public T Value { get; }

        public DataResult(T value)
        {
            Value = value;
        }
    }
}
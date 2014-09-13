namespace epvpapi.Evaluation
{
    public abstract class TargetableParser<T>
    {
        public T Target { get; set; }

        public TargetableParser(T target)
        {
            Target = target;
        }
    }
}

namespace epvpapi.Evaluation
{
    public abstract class TargetableParser<T>
    {
        protected TargetableParser(T target)
        {
            Target = target;
        }

        protected T Target { get; private set; }
    }
}
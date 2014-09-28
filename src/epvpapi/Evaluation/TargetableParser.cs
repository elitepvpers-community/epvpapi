namespace epvpapi.Evaluation
{
    /// <summary>
    /// Base class for Html parsers that aim to update a specific target of type <c>T</c>
    /// </summary>
    /// <typeparam name="T"> Type of the target to update </typeparam>
    internal abstract class TargetableParser<T>
    {
        /// <summary>
        /// Target being updated by the parser
        /// </summary>
        protected T Target { get; set; }

        public TargetableParser(T target)
        {
            Target = target;
        }
    }
}

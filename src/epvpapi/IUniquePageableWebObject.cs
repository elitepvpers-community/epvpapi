namespace epvpapi
{
    public interface IUniquePageableWebObject
    {
        /// <summary>
        /// Returns the online web url to the unique web object
        /// </summary>
        string GetUrl(uint pageIndex = 1);
    }
}
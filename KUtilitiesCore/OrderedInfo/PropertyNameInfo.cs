namespace KUtilitiesCore.OrderedInfo
{
    public struct PropertyNameInfo(string technicalName, string displayName)
    {
        public string TechnicalName { get; set; } = technicalName;
        public string DisplayName { get; set; } = displayName;
    }
}
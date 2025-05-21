namespace KUtilitiesCore.OrderedInfo
{
    public struct PropertyNameInfo(string technicalName, string displayName)
    {
        public string PropertyName { get; set; } = technicalName;
        public string DisplayName { get; set; } = displayName;
    }
}
namespace OptimizelyBlogs.Foundation.SiteSetting.Infrastructure
{
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public sealed class SettingsContentTypeAttribute : ContentTypeAttribute
    {
        public string? SettingsName { get; set; } = default(string?);
    }

    [AttributeUsage(validOn: AttributeTargets.Class)]
    public sealed class ContentFolder : ContentTypeAttribute
    {
        public string? FolderName { get; set; } = default(string?);
    }
}

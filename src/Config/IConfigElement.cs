namespace UnityExplorer.Config
{
    public interface IConfigElement
    {
        string Name { get; }
        string Description { get; }
        string DefaultDescription { get; }

        string NameKey { get; }
        string DescriptionKey { get; }

        bool IsInternal { get; }
        Type ElementType { get; }

        object BoxedValue { get; set; }
        object DefaultValue { get; }

        object GetLoaderConfigValue();

        void RevertToDefaultValue();

        Action OnValueChangedNotify { get; set; }
    }
}

namespace UnityExplorer.Config
{
    public class ConfigElement<T> : IConfigElement
    {
        public string Name => TranslationManager.Get(NameKey);
        public string Description => TranslationManager.Get(DescriptionKey);
        public string DefaultDescription => TranslationManager.Get(TranslationManager.Lang.English, DescriptionKey);

        public string NameKey { get; }
        public string DescriptionKey => $"{NameKey}_hint";

        public bool IsInternal { get; }
        public Type ElementType => typeof(T);

        public Action<T> OnValueChanged;
        public Action OnValueChangedNotify { get; set; }

        public object DefaultValue { get; }

        public ConfigHandler Handler => IsInternal
            ? ConfigManager.InternalHandler
            : ConfigManager.Handler;

        public T Value
        {
            get => m_value;
            set => SetValue(value);
        }
        private T m_value;

        object IConfigElement.BoxedValue
        {
            get => m_value;
            set => SetValue((T)value);
        }

        public ConfigElement(string name, T defaultValue, bool isInternal = false)
        {
            NameKey = name;

            m_value = defaultValue;
            DefaultValue = defaultValue;

            IsInternal = isInternal;

            ConfigManager.RegisterConfigElement(this);
        }

        private void SetValue(T value)
        {
            if ((m_value == null && value == null) || (m_value != null && m_value.Equals(value)))
                return;

            m_value = value;

            Handler.SetConfigValue(this, value);

            OnValueChanged?.Invoke(value);
            OnValueChangedNotify?.Invoke();

            Handler.OnAnyConfigChanged();
        }

        object IConfigElement.GetLoaderConfigValue() => GetLoaderConfigValue();

        public T GetLoaderConfigValue()
        {
            return Handler.GetConfigValue(this);
        }

        public void RevertToDefaultValue()
        {
            Value = (T)DefaultValue;
        }
    }
}

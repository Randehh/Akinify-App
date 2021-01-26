using System;
using System.Windows.Markup;

namespace Akinify_App {
    public class EnumBindingSourceExtension : MarkupExtension {
        private Type m_EnumType;
        public Type EnumType {
            get { return m_EnumType; }
            set {
                if (value != m_EnumType) {
                    if (null != value) {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum)
                            throw new ArgumentException("Type must be for an Enum.");
                    }

                    m_EnumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType) {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (null == m_EnumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualEnumType = Nullable.GetUnderlyingType(this.m_EnumType) ?? this.m_EnumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == m_EnumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}

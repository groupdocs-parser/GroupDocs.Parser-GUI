using System;

namespace GroupDocs.Parser.GUI.ViewModels
{
    public class BoolWrapper
    {
        private bool value;

        public event Action ValueChanged;

        public BoolWrapper(bool initialValue)
        {
            value = initialValue;
        }

        public bool Value => value;

        public void SetValue(bool newValue)
        {
            if (value != newValue)
            {
                value = newValue;

                ValueChanged?.Invoke();
            }
        }
    }
}

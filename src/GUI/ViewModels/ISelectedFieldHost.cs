namespace GroupDocs.Parser.GUI.ViewModels
{
    internal interface ISelectedFieldHost
    {
        IFieldViewModel SelectedField { get; set; }

        void Remove(IFieldViewModel field);
    }
}

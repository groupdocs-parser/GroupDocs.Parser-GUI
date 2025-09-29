namespace GroupDocs.Parser.GUI.ViewModels
{
    interface IFieldViewModel
    {
        bool IsSelected { get; set; }
        string Name { get; set; }
        int PageIndex { get; }
        string Text { get; set; }
        bool IsHidden { get; set; }
    }
}

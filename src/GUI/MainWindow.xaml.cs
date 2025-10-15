using GroupDocs.Parser.GUI.Controls;
using System;
using System.Windows;
using System.Windows.Input;

namespace GroupDocs.Parser.GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public event EventHandler<PercentagePositionEventArgs> PercentagePositionChanged;
    public event EventHandler<MouseWheelEventArgs> MouseWheelCustom;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void CustomScrollViewer_PercentagePositionChanged(object sender, PercentagePositionEventArgs e)
    {
        PercentagePositionChanged?.Invoke(this, e);
    }

    private void CustomScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        MouseWheelCustom?.Invoke(this, e);
    }
}

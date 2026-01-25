using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Kuttab.ViewModels;
using System;

namespace Kuttab.Views;

public partial class TajweedRuleWindow : Window
{
    public TajweedRuleWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TajweedRuleViewModel viewModel)
        {
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }
}

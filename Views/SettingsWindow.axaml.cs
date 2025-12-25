using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Kuttab.ViewModels;
using System;

namespace Kuttab.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Subscribe to ViewModel events
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.SettingsSaved += OnSettingsSaved;
            viewModel.Cancelled += OnCancelled;
        }
        
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.SettingsSaved += OnSettingsSaved;
            viewModel.Cancelled += OnCancelled;
        }
    }

    private void OnSettingsSaved(object? sender, SettingsSavedEventArgs e)
    {
        Close(e);
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        Close();
    }
}

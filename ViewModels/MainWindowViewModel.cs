﻿using FatturaElettronica_PRISM_WPF_V2.Services;
using Prism.Mvvm;

namespace FatturaElettronica_PRISM_WPF_V2.ViewModels;
public class MainWindowViewModel : BindableBase
{
    private string _title = "Prism Application";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public MainWindowViewModel(IDbManager dbManager)
    {

    }
}

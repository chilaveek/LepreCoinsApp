namespace LepreCoinsApp.Views;

using Interfaces.DTO;
using ApplicationCore.Interfaces;
using Interfaces.Service;
using LepreCoinsApp.ViewModels;
using System.ComponentModel;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.LoadDataCommand.CanExecute(null))
        {
            await _viewModel.LoadDataAsync();
        }
    }


    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {

        await Shell.Current.GoToAsync("add-expense");
    }
    private async void OnAddIncomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("add-income");
    }
    private async void OnTransactionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TransactionDto transaction)
            return;

        var action = await DisplayActionSheetAsync(
            "Выберите действие",
            "Отмена",
            "Удалить",
            "Редактировать");

        switch (action)
        {
            case "Редактировать":
                await Shell.Current.GoToAsync($"edit-transaction?id={transaction.Id}");
                break;

            case "Удалить":
                var confirm = await DisplayAlertAsync(
                    "Подтверждение",
                    "Удалить эту транзакцию?",
                    "Да",
                    "Нет");

                if (confirm)
                {

                }
                break;
        }

        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnScanReceiptClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите фото чека",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
                return;

            await DisplayAlertAsync("Информация", $"Выбран файл: {result.FileName}", "OK");

        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Ошибка при выборе файла: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshRequested(object sender, RefreshEventArgs e)
    {
        try
        {
            await _viewModel.LoadDataAsync();
        }
        finally
        {
            ((RefreshView)sender).IsRefreshing = false;
        }
    }
}
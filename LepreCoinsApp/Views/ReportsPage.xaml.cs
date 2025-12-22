using LepreCoinsApp.ViewModels;
using Microsoft.Maui.Graphics;

namespace LepreCoinsApp.Views;

public partial class ReportsPage : ContentPage
{
    private ReportsViewModel? _viewModel;

    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ReportsViewModel.IsReportVisible))
                {
                    MainThread.BeginInvokeOnMainThread(() => RedrawCharts());
                }
            };

            if (_viewModel.IsReportVisible)
            {
                MainThread.BeginInvokeOnMainThread(() => RedrawCharts());
            }
        }
    }

    private void RedrawCharts()
    {
        if (_viewModel == null) return;

        try
        {
            if (PieChartView != null)
            {
                var drawable = new SimpleBarDrawable(
                    (double)_viewModel.TotalIncome,
                    (double)_viewModel.TotalExpenses
                );
                PieChartView.Drawable = drawable;
            }

            if (BarChartView != null)
            {
                var drawable = new BarChartDrawable(
                    (double)_viewModel.TotalIncome,
                    (double)_viewModel.TotalExpenses
                );
                BarChartView.Drawable = drawable;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }
}

public class SimpleBarDrawable : IDrawable
{
    private double _income;
    private double _expenses;

    public SimpleBarDrawable(double income, double expenses)
    {
        _income = income;
        _expenses = expenses;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        double total = _income + _expenses;
        if (total == 0)
        {
            canvas.FillColor = Color.FromArgb("#333333");
            canvas.FillRectangle(dirtyRect);
            return;
        }

        float centerY = dirtyRect.Height / 2;
        float maxWidth = dirtyRect.Width - 40;

        float incomeWidth = (float)(maxWidth * (_income / total));
        canvas.FillColor = Color.FromArgb("#2ecc71");
        canvas.FillRectangle(20, centerY - 40, incomeWidth, 80);

        canvas.FontColor = Color.FromArgb("#FFFFFF");
        canvas.FontSize = 12;
        canvas.DrawString($" {_income:F0}", 30, centerY - 10, incomeWidth - 20, 20, HorizontalAlignment.Left, VerticalAlignment.Center);

        // Расход справа
        float expensesWidth = maxWidth - incomeWidth;
        canvas.FillColor = Color.FromArgb("#e74c3c");
        canvas.FillRectangle(20 + incomeWidth, centerY - 40, expensesWidth, 80);

        canvas.FontColor = Color.FromArgb("#FFFFFF");
        canvas.DrawString($" {_expenses:F0}", 30 + incomeWidth, centerY - 10, expensesWidth - 20, 20, HorizontalAlignment.Left, VerticalAlignment.Center);

        // Процент в центре разделения
        int percent = (int)(_income / total * 100);
        canvas.FontSize = 10;
        canvas.FontColor = Color.FromArgb("#000000");
        canvas.DrawString($"{percent}%", 20 + incomeWidth - 15, centerY - 50, 30, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}

public class BarChartDrawable : IDrawable
{
    private double _income;
    private double _expenses;

    public BarChartDrawable(double income, double expenses)
    {
        _income = income;
        _expenses = expenses;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_income == 0 && _expenses == 0)
        {
            canvas.FillColor = Color.FromArgb("#1a1a1a");
            canvas.FillRectangle(dirtyRect);
            return;
        }

        float topMargin = 40;
        float bottomMargin = 60;
        float leftMargin = 40;
        float rightMargin = 30;

        float chartHeight = dirtyRect.Height - topMargin - bottomMargin;
        double maxValue = Math.Max(_income, _expenses);

        canvas.FontColor = Color.FromArgb("#FFFFFF");
        canvas.FontSize = 12;

        canvas.DrawString("Доходы vs Расходы", leftMargin, 10, 200, 20, HorizontalAlignment.Left, VerticalAlignment.Top);

        float barWidth = 50;
        float spacing = 100;
        float startX = leftMargin + 30;

        // Линия основания (ось X)
        float baselineY = dirtyRect.Height - bottomMargin;
        canvas.StrokeColor = Color.FromArgb("#444444");
        canvas.StrokeSize = 2;
        canvas.DrawLine(leftMargin - 10, baselineY, dirtyRect.Width - rightMargin, baselineY);

        float incomeHeight = (float)(chartHeight * (_income / maxValue));
        float incomeX = startX;
        float incomeY = baselineY - incomeHeight;

        canvas.FillColor = Color.FromArgb("#2ecc71");
        canvas.FillRoundedRectangle(incomeX, incomeY, barWidth, incomeHeight, 5);

        canvas.FontSize = 20;
        canvas.DrawString("", incomeX + barWidth / 2 - 10, incomeY - 25, 20, 25, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontColor = Color.FromArgb("#2ecc71");
        canvas.FontSize = 10;

        canvas.DrawString(_income.ToString("F0"), incomeX + barWidth / 2, incomeY - 5, 60, 15, HorizontalAlignment.Center, VerticalAlignment.Bottom);

        canvas.FontColor = Color.FromArgb("#999999");
        canvas.FontSize = 9;
        canvas.DrawString("Доходы", incomeX + barWidth / 2, baselineY + 15, 60, 15, HorizontalAlignment.Center, VerticalAlignment.Top);

        float expensesHeight = (float)(chartHeight * (_expenses / maxValue));
        float expensesX = startX + spacing;
        float expensesY = baselineY - expensesHeight;

        canvas.FillColor = Color.FromArgb("#e74c3c");
        canvas.FillRoundedRectangle(expensesX, expensesY, barWidth, expensesHeight, 5);

        canvas.FontSize = 20;
        canvas.DrawString("", expensesX + barWidth / 2 - 10, expensesY - 25, 20, 25, HorizontalAlignment.Center, VerticalAlignment.Center);

        canvas.FontColor = Color.FromArgb("#e74c3c");
        canvas.FontSize = 10;

        canvas.DrawString(_expenses.ToString("F0"), expensesX + barWidth / 2, expensesY - 5, 60, 15, HorizontalAlignment.Center, VerticalAlignment.Bottom);

        canvas.FontColor = Color.FromArgb("#999999");
        canvas.FontSize = 9;
        canvas.DrawString("Расходы", expensesX + barWidth / 2, baselineY + 15, 60, 15, HorizontalAlignment.Center, VerticalAlignment.Top);

        float statsX = startX + spacing * 1.5f + 40;
        float statsY = incomeY;

        double balance = _income - _expenses;
        string balanceText = balance >= 0 ? $"+{balance:F0}" : $"{balance:F0}";
        Color balanceColor = balance >= 0 ? Color.FromArgb("#2ecc71") : Color.FromArgb("#e74c3c");

        canvas.FontColor = balanceColor;

        canvas.FontSize = 9;
        canvas.DrawString("Баланс:", statsX, statsY, 80, 20, HorizontalAlignment.Left, VerticalAlignment.Top);
        canvas.DrawString(balanceText, statsX, statsY + 18, 80, 20, HorizontalAlignment.Left, VerticalAlignment.Top);

        double percentage = _income > 0 ? (_expenses / _income) * 100 : 0;
        canvas.FontColor = Color.FromArgb("#999999");
        canvas.DrawString("Использовано:", statsX, statsY + 45, 80, 20, HorizontalAlignment.Left, VerticalAlignment.Top);
        canvas.FontColor = Color.FromArgb("#3498db");
     
        canvas.DrawString($"{percentage:F0}%", statsX, statsY + 63, 80, 20, HorizontalAlignment.Left, VerticalAlignment.Top);
    }
}

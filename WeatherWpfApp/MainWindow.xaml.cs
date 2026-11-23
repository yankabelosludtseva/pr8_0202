using System;
using System.Windows;
using WeatherWpfApp.Services;

namespace WeatherWpfApp
{
    public partial class MainWindow : Window
    {
        private readonly WeatherService _service = new WeatherService();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadWeatherAsync();
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadWeatherAsync();
        }

        private async System.Threading.Tasks.Task LoadWeatherAsync()
        {
            try
            {
                LoadButton.IsEnabled = false;
                StatusTextBlock.Text = "Загрузка данных…";

                var days = await _service.GetForecastAsync(CityTextBox.Text.Trim());
                DaysItemsControl.ItemsSource = days;

                StatusTextBlock.Text = $"Обновлено: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Не удалось загрузить данные";
            }
            finally
            {
                LoadButton.IsEnabled = true;
            }
        }
    }
}
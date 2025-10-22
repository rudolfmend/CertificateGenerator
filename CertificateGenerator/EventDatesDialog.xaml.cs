using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CertificateGenerator
{
    public partial class EventDatesDialog : Window
    {
        public List<EventTopic> EventTopics { get; set; }

        public EventDatesDialog(List<EventTopic> topics)
        {
            InitializeComponent();

            // Create numbered topics with dates
            EventTopics = new List<EventTopic>();
            for (int i = 0; i < topics.Count; i++)
            {
                EventTopics.Add(new EventTopic
                {
                    Index = i + 1,
                    Topic = topics[i].Topic,
                    EventDate = topics[i].EventDate
                });
            }

            DgEventTopics.ItemsSource = EventTopics;
        }

        private void ApplyCommonDate_Click(object sender, RoutedEventArgs e)
        {
            if (!DtpCommonDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Prosím vyberte dátum.", "Informácia",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var topic in EventTopics)
            {
                topic.EventDate = DtpCommonDate.SelectedDate;
            }

            DgEventTopics.Items.Refresh();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            // Check if all topics have dates
            var topicsWithoutDates = EventTopics.Where(t => !t.EventDate.HasValue).ToList();

            if (topicsWithoutDates.Any())
            {
                var result = MessageBox.Show(
                    $"Niektoré témy nemajú priradený dátum ({topicsWithoutDates.Count}).\n\n" +
                    "Chcete pokračovať aj tak?",
                    "Upozornenie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // EventTopic model class
    public class EventTopic : System.ComponentModel.INotifyPropertyChanged
    {
        private DateTime? _eventDate;

        public int Index { get; set; }
        public string Topic { get; set; }

        public DateTime? EventDate
        {
            get => _eventDate;
            set
            {
                _eventDate = value;
                OnPropertyChanged(nameof(EventDate));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
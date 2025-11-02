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

        private void AutoFillDates_Click(object sender, RoutedEventArgs e)
        {
            if (!DtpAutoStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Prosím vyberte počiatočný dátum.", "Informácia",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DateTime startDate = DtpAutoStartDate.SelectedDate.Value;
            int ruleIndex = CmbDateRule.SelectedIndex;

            ApplyDateRule(startDate, ruleIndex);
            DgEventTopics.Items.Refresh();
        }

        private void ApplyDateRule(DateTime startDate, int ruleIndex)
        {
            DateTime currentDate = startDate;

            foreach (var topic in EventTopics)
            {
                topic.EventDate = currentDate;
                currentDate = GetNextDate(currentDate, ruleIndex);
            }
        }

        private DateTime GetNextDate(DateTime currentDate, int ruleIndex)
        {
            switch (ruleIndex)
            {
                case 0: // Prvý pondelok v mesiaci
                    return GetFirstDayOfWeekInMonth(currentDate.AddMonths(1), DayOfWeek.Monday);

                case 1: // Prvý utorok v mesiaci
                    return GetFirstDayOfWeekInMonth(currentDate.AddMonths(1), DayOfWeek.Tuesday);

                case 2: // Prvý štvrtok v mesiaci
                    return GetFirstDayOfWeekInMonth(currentDate.AddMonths(1), DayOfWeek.Thursday);

                case 3: // Prvý piatok v mesiaci
                    return GetFirstDayOfWeekInMonth(currentDate.AddMonths(1), DayOfWeek.Friday);

                case 4: // Posledný piatok v mesiaci
                    return GetLastDayOfWeekInMonth(currentDate.AddMonths(1), DayOfWeek.Friday);

                case 5: // 15. deň v mesiaci
                    var nextMonth = currentDate.AddMonths(1);
                    return new DateTime(nextMonth.Year, nextMonth.Month, 15);

                case 6: // Prvý deň v mesiaci
                    var nextMonth2 = currentDate.AddMonths(1);
                    return new DateTime(nextMonth2.Year, nextMonth2.Month, 1);

                case 7: // Každý týždeň (+7 dní)
                    return currentDate.AddDays(7);

                case 8: // Každé 2 týždne (+14 dní)
                    return currentDate.AddDays(14);

                default:
                    return currentDate.AddMonths(1);
            }
        }

        private DateTime GetFirstDayOfWeekInMonth(DateTime date, DayOfWeek dayOfWeek)
        {
            DateTime firstDay = new DateTime(date.Year, date.Month, 1);

            while (firstDay.DayOfWeek != dayOfWeek)
            {
                firstDay = firstDay.AddDays(1);
            }

            return firstDay;
        }

        private DateTime GetLastDayOfWeekInMonth(DateTime date, DayOfWeek dayOfWeek)
        {
            DateTime lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

            while (lastDay.DayOfWeek != dayOfWeek)
            {
                lastDay = lastDay.AddDays(-1);
            }

            return lastDay;
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

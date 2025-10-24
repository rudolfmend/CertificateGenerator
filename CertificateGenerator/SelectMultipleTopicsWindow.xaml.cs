using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class SelectMultipleTopicsWindow : Window
    {
        private ObservableCollection<SelectableTopic> _topics;
        public List<EventTopicModel> SelectedTopics { get; private set; }

        public SelectMultipleTopicsWindow(List<EventTopicModel> topics)
        {
            InitializeComponent();

            _topics = new ObservableCollection<SelectableTopic>(
                topics.Select(t => new SelectableTopic(t)));

            DgTopics.ItemsSource = _topics;

            foreach (var t in _topics)
                t.PropertyChanged += Topic_PropertyChanged;
        }

        private void Topic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
                UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            TxtSelectedCount.Text = _topics.Count(t => t.IsSelected).ToString();
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string search = TxtSearch.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(search))
            {
                foreach (var t in _topics)
                    t.IsVisible = true;
            }
            else
            {
                foreach (var t in _topics)
                {
                    t.IsVisible = t.Topic.ToLower().Contains(search) ||
                                 (t.Description?.ToLower().Contains(search) ?? false);
                }
            }

            DgTopics.Items.Refresh();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var t in _topics.Where(t => t.IsVisible))
                t.IsSelected = true;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            SelectedTopics = _topics
                .Where(t => t.IsSelected)
                .Select(t => t.EventTopic)
                .ToList();

            if (SelectedTopics.Count == 0)
            {
                MessageBox.Show("Prosím vyberte aspoň jednu tému.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
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

    public class SelectableTopic : INotifyPropertyChanged
    {
        public EventTopicModel EventTopic { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public string Topic => EventTopic.Topic;
        public string Description => EventTopic.Description;
        public int UsageCount => EventTopic.UsageCount;

        public SelectableTopic(EventTopicModel topic)
        {
            EventTopic = topic;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

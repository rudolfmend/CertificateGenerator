using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using CertificateGenerator.Data;
using CertificateGenerator.Helpers;
using iText.Kernel.Geom;

namespace CertificateGenerator
{
    public partial class TemplateEditorWindow : Window
    {
        private CertificateTemplateRepository _templateRepo;
        private CertificateTemplateModel _currentTemplate;
        private bool _isLoading = false;

        public TemplateEditorWindow()
        {
            InitializeComponent();
            InitializeRepositories();
            LoadTemplates();
            LoadNewTemplate();
        }

        private void InitializeRepositories()
        {
            try
            {
                var dbManager = App.DatabaseManager;
                _templateRepo = new CertificateTemplateRepository(dbManager);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri inicializácii databázy:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTemplates()
        {
            try
            {
                var templates = _templateRepo.GetAll();
                CmbTemplates.ItemsSource = templates;

                if (templates.Count > 0)
                {
                    CmbTemplates.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní šablón:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNewTemplate()
        {
            _currentTemplate = DefaultTemplates.Classic;
            LoadTemplateToUI(_currentTemplate);
        }

        private void LoadTemplateToUI(CertificateTemplateModel template)
        {
            _isLoading = true;

            try
            {
                TxtTemplateName.Text = template.Name;
                ChkIsDefault.IsChecked = template.IsDefault;

                // Farby
                TxtTitleColor.Text = template.TitleColor;
                TxtTextColor.Text = template.TextColor;
                TxtAccentColor.Text = template.AccentColor;
                TxtBackgroundColor.Text = template.BackgroundColor;

                // Veľkosti písma
                TxtTitleFontSize.Text = template.TitleFontSize.ToString();
                TxtHeaderFontSize.Text = template.HeaderFontSize.ToString();
                TxtTextFontSize.Text = template.TextFontSize.ToString();

                // Okraje
                TxtMarginTop.Text = template.MarginTop.ToString();
                TxtMarginRight.Text = template.MarginRight.ToString();
                TxtMarginBottom.Text = template.MarginBottom.ToString();
                TxtMarginLeft.Text = template.MarginLeft.ToString();

                // Rámček
                ChkShowBorder.IsChecked = template.ShowBorder;
                TxtBorderColor.Text = template.BorderColor;
                TxtBorderWidth.Text = template.BorderWidth.ToString();

                // Titulok
                ChkShowTitle.IsChecked = template.ShowTitle;
                TxtCertificateTitle.Text = template.CertificateTitle;

                if (template.TitleAlignment == "LEFT")
                    CmbTitleAlignment.SelectedIndex = 0;
                else if (template.TitleAlignment == "RIGHT")
                    CmbTitleAlignment.SelectedIndex = 2;
                else
                    CmbTitleAlignment.SelectedIndex = 1;

                // Oddeľovač
                ChkShowSeparator.IsChecked = template.ShowSeparatorLine;

                if (template.SeparatorStyle == "LINE")
                    CmbSeparatorStyle.SelectedIndex = 1;
                else if (template.SeparatorStyle == "NONE")
                    CmbSeparatorStyle.SelectedIndex = 2;
                else
                    CmbSeparatorStyle.SelectedIndex = 0;

                // Viditeľnosť polí
                ChkShowOrganizer.IsChecked = template.ShowOrganizer;
                ChkShowEventTopic.IsChecked = template.ShowEventTopic;
                ChkShowEventDate.IsChecked = template.ShowEventDate;
                ChkShowBirthDate.IsChecked = template.ShowBirthDate;
                ChkShowRegistrationNumber.IsChecked = template.ShowRegistrationNumber;
                ChkShowNotes.IsChecked = template.ShowNotes;

                // Vlastný text
                TxtCustomHeader.Text = template.CustomHeaderText ?? "";
                TxtCustomFooter.Text = template.CustomFooterText ?? "";

                _currentTemplate = template;
            }
            finally
            {
                _isLoading = false;
                UpdatePreview();
            }
        }

        private CertificateTemplateModel GetTemplateFromUI()
        {
            var template = new CertificateTemplateModel
            {
                Id = _currentTemplate?.Id ?? 0,
                Name = TxtTemplateName.Text,
                IsDefault = ChkIsDefault.IsChecked == true,

                // Farby
                TitleColor = TxtTitleColor.Text,
                TextColor = TxtTextColor.Text,
                AccentColor = TxtAccentColor.Text,
                BackgroundColor = TxtBackgroundColor.Text,

                // Písma
                TitleFontFamily = "Helvetica-Bold",
                TitleFontSize = int.TryParse(TxtTitleFontSize.Text, out int titleSize) ? titleSize : 20,
                HeaderFontFamily = "Helvetica-Bold",
                HeaderFontSize = int.TryParse(TxtHeaderFontSize.Text, out int headerSize) ? headerSize : 12,
                TextFontFamily = "Helvetica",
                TextFontSize = int.TryParse(TxtTextFontSize.Text, out int textSize) ? textSize : 10,

                // Okraje
                MarginTop = int.TryParse(TxtMarginTop.Text, out int marginTop) ? marginTop : 30,
                MarginRight = int.TryParse(TxtMarginRight.Text, out int marginRight) ? marginRight : 30,
                MarginBottom = int.TryParse(TxtMarginBottom.Text, out int marginBottom) ? marginBottom : 30,
                MarginLeft = int.TryParse(TxtMarginLeft.Text, out int marginLeft) ? marginLeft : 30,

                // Titulok
                ShowTitle = ChkShowTitle.IsChecked == true,
                CertificateTitle = TxtCertificateTitle.Text,
                TitleAlignment = ((ComboBoxItem)CmbTitleAlignment.SelectedItem)?.Tag?.ToString() ?? "CENTER",

                // Oddeľovač
                ShowSeparatorLine = ChkShowSeparator.IsChecked == true,
                SeparatorStyle = ((ComboBoxItem)CmbSeparatorStyle.SelectedItem)?.Tag?.ToString() ?? "UNDERLINE",

                // Rámček
                ShowBorder = ChkShowBorder.IsChecked == true,
                BorderColor = TxtBorderColor.Text,
                BorderWidth = int.TryParse(TxtBorderWidth.Text, out int borderWidth) ? borderWidth : 2,

                // Viditeľnosť
                ShowOrganizer = ChkShowOrganizer.IsChecked == true,
                ShowEventTopic = ChkShowEventTopic.IsChecked == true,
                ShowEventDate = ChkShowEventDate.IsChecked == true,
                ShowBirthDate = ChkShowBirthDate.IsChecked == true,
                ShowRegistrationNumber = ChkShowRegistrationNumber.IsChecked == true,
                ShowNotes = ChkShowNotes.IsChecked == true,

                // Vlastný text
                CustomHeaderText = TxtCustomHeader.Text,
                CustomFooterText = TxtCustomFooter.Text
            };

            return template;
        }

        private void UpdatePreview()
        {
            if (_isLoading) return;

            try
            {
                var template = GetTemplateFromUI();
                PreviewContent.Children.Clear();

                // Nastavenie pozadia
                PreviewBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.BackgroundColor));

                // Nastavenie okrajov
                PreviewBorder.Padding = new Thickness(
                    template.MarginLeft / 2.0,
                    template.MarginTop / 2.0,
                    template.MarginRight / 2.0,
                    template.MarginBottom / 2.0
                );

                // Rámček
                if (template.ShowBorder)
                {
                    PreviewBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.BorderColor));
                    PreviewBorder.BorderThickness = new Thickness(template.BorderWidth);
                }
                else
                {
                    PreviewBorder.BorderThickness = new Thickness(0);
                }

                // Vlastný text v záhlaví
                if (!string.IsNullOrWhiteSpace(template.CustomHeaderText))
                {
                    var headerBlock = new TextBlock
                    {
                        Text = template.CustomHeaderText,
                        FontSize = template.TextFontSize,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.TextColor)),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    PreviewContent.Children.Add(headerBlock);
                }

                // Titulok
                if (template.ShowTitle)
                {
                    var titleAlignment = template.TitleAlignment == "LEFT" ? TextAlignment.Left :
                                       template.TitleAlignment == "RIGHT" ? TextAlignment.Right :
                                       TextAlignment.Center;

                    var titleBlock = new TextBlock
                    {
                        Text = template.CertificateTitle,
                        FontSize = template.TitleFontSize,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.TitleColor)),
                        TextAlignment = titleAlignment,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    PreviewContent.Children.Add(titleBlock);
                }

                // Oddeľovač
                if (template.ShowSeparatorLine && template.SeparatorStyle != "NONE")
                {
                    if (template.SeparatorStyle == "UNDERLINE")
                    {
                        var separator = new TextBlock
                        {
                            Text = "_____________________________________",
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.AccentColor)),
                            Margin = new Thickness(0, 0, 0, 15)
                        };
                        PreviewContent.Children.Add(separator);
                    }
                    else if (template.SeparatorStyle == "LINE")
                    {
                        var line = new Border
                        {
                            Height = 1,
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.AccentColor)),
                            Margin = new Thickness(0, 0, 0, 15)
                        };
                        PreviewContent.Children.Add(line);
                    }
                }

                // Organizátor
                if (template.ShowOrganizer)
                {
                    AddPreviewField("Organizátor: Slovenská komora medicínsko-technických pracovníkov",
                        template.TextFontSize + 1, template.TextColor, false);
                }

                // Téma podujatia
                if (template.ShowEventTopic)
                {
                    AddPreviewField("Téma podujatia: Moderné trendy v zdravotníctve",
                        template.HeaderFontSize, template.AccentColor, true);
                }

                // Účastník
                AddPreviewField("Účastník: Ing. Ján Novák",
                    template.HeaderFontSize + 2, template.TitleColor, true);

                // Dátum podujatia
                if (template.ShowEventDate)
                {
                    AddPreviewField("Dátum podujatia: 15.03.2025",
                        template.TextFontSize, template.TextColor, false);
                }

                // Dátum narodenia
                if (template.ShowBirthDate)
                {
                    AddPreviewField("Dátum narodenia: 01.01.1990",
                        template.TextFontSize, template.TextColor, false);
                }

                // Registračné číslo
                if (template.ShowRegistrationNumber)
                {
                    AddPreviewField("Registračné číslo v komore: 12345",
                        template.TextFontSize, template.TextColor, false);
                }

                // Poznámky
                if (template.ShowNotes)
                {
                    var notesLabel = new TextBlock
                    {
                        Text = "Poznámky:",
                        FontSize = template.TextFontSize + 1,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.TextColor)),
                        Margin = new Thickness(0, 10, 0, 3)
                    };
                    PreviewContent.Children.Add(notesLabel);

                    var notesText = new TextBlock
                    {
                        Text = "Toto je ukážkový text poznámky ku certifikátu. Môže obsahovať dodatočné informácie o absolvovanom podujatí.",
                        FontSize = template.TextFontSize,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.TextColor)),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    PreviewContent.Children.Add(notesText);
                }

                // Vlastný text v pätičke
                if (!string.IsNullOrWhiteSpace(template.CustomFooterText))
                {
                    var footerBlock = new TextBlock
                    {
                        Text = template.CustomFooterText,
                        FontSize = template.TextFontSize - 1,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(template.TextColor)),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 15, 0, 0)
                    };
                    PreviewContent.Children.Add(footerBlock);
                }

                // Časová pečiatka
                var timestamp = new TextBlock
                {
                    Text = $"\nVytvorené: {DateTime.Now:dd.MM.yyyy HH:mm}",
                    FontSize = template.TextFontSize - 1,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                PreviewContent.Children.Add(timestamp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri aktualizácii náhľadu: {ex.Message}");
            }
        }

        private void AddPreviewField(string text, int fontSize, string color, bool bold)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                Margin = new Thickness(0, 0, 0, 8)
            };

            if (bold)
                textBlock.FontWeight = FontWeights.Bold;

            PreviewContent.Children.Add(textBlock);
        }

        // Event handlers
        private void CmbTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTemplates.SelectedItem is CertificateTemplateModel template)
            {
                LoadTemplateToUI(template);
            }
        }

        private void ColorChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void FontSizeChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void MarginChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void BorderSettingsChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void TitleSettingsChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void TitleSettingsChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void TitleSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void SeparatorSettingsChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void SeparatorSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void FieldVisibilityChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void CustomTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void RefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTemplateName.Text))
            {
                MessageBox.Show("Prosím zadajte názov šablóny.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var template = GetTemplateFromUI();

                if (template.Id > 0)
                {
                    _templateRepo.Update(template);
                    MessageBox.Show("Šablóna bola úspešne aktualizovaná.",
                        "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    int id = _templateRepo.Add(template);
                    template.Id = id;
                    _currentTemplate = template;
                    MessageBox.Show("Šablóna bola úspešne uložená.",
                        "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadTemplates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri ukladaní šablóny:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadNewTemplate();
            CmbTemplates.SelectedIndex = -1;
        }

        private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTemplates.SelectedItem is CertificateTemplateModel template && template.Id > 0)
            {
                var result = MessageBox.Show(
                    $"Naozaj chcete odstrániť šablónu '{template.Name}'?",
                    "Potvrdenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _templateRepo.Delete(template.Id);
                        MessageBox.Show("Šablóna bola úspešne odstránená.",
                            "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTemplates();
                        LoadNewTemplate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba pri odstraňovaní šablóny:\n{ex.Message}",
                            "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Prosím vyberte uloženú šablónu na odstránenie.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadClassicTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Classic);
            CmbTemplates.SelectedIndex = -1;
        }

        private void LoadModernTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Modern);
            CmbTemplates.SelectedIndex = -1;
        }

        private void LoadElegantTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Elegant);
            CmbTemplates.SelectedIndex = -1;
        }

        private void LoadMinimalistTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Minimalist);
            CmbTemplates.SelectedIndex = -1;
        }

        private void TestPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF súbory (*.pdf)|*.pdf",
                    FileName = "Test_Certificate.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var template = GetTemplateFromUI();

                    CertificatePdfGenerator.GeneratePdf(
                        saveDialog.FileName,
                        template,
                        "Slovenská komora medicínsko-technických pracovníkov",
                        "Moderné trendy v zdravotníctve",
                        DateTime.Now,
                        "Ing. Ján Novák",
                        new DateTime(1990, 1, 1),
                        "12345",
                        "Toto je ukážkový text poznámky ku certifikátu. Môže obsahovať dodatočné informácie o absolvovanom podujatí.",
                        PageSize.A5
                    );

                    var result = MessageBox.Show(
                        "Test PDF bol úspešne vytvorený!\n\nChcete ho otvoriť?",
                        "Úspech", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri vytváraní test PDF:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


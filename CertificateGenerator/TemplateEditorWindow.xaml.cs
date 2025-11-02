using CertificateGenerator.Data;
using CertificateGenerator.Helpers;
using iText.Kernel.Geom;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms; // Pre ColorDialog
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;

namespace CertificateGenerator
{
    public partial class TemplateEditorWindow : Window
    {
        private CertificateTemplateRepository _templateRepo;
        private CertificateTemplateModel _currentTemplate;
        private bool _isLoading = false;
        private bool _hasUnsavedChanges = false;

        // Drag & Drop
        private System.Windows.Point _dragStartPoint;
        private object _draggedItem;
        private CertificateField _selectedField;

        public TemplateEditorWindow()
        {
            _isLoading = true;
            _currentTemplate = null;

            InitializeComponent();
            InitializeRepositories();
            InitializeFontPickers();
            LoadTemplates();
            LoadNewTemplate();

            _isLoading = false;
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
                System.Windows.MessageBox.Show($"Chyba pri inicializácii databázy:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeFontPickers()
        {
            var fonts = AvailableFonts.GetAll();

            CmbTitleFont.ItemsSource = fonts;
            CmbHeaderFont.ItemsSource = fonts;
            CmbTextFont.ItemsSource = fonts;

            // Nastav predvolené fonty
            CmbTitleFont.SelectedValue = "Helvetica-Bold";
            CmbHeaderFont.SelectedValue = "Helvetica-Bold";
            CmbTextFont.SelectedValue = "Helvetica";
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
                System.Windows.MessageBox.Show($"Chyba pri načítaní šablón:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnySettingChanged(object sender, EventArgs e)
        {
            if (!_isLoading) _hasUnsavedChanges = true;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                if (_hasUnsavedChanges)
                {
                    var result = System.Windows.MessageBox.Show(
                        "Máte rozpísanú prácu. Naozaj zavrieť?",
                        "Potvrdenie",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes) return;
                }
                this.Close();
            }
        }

        private void LoadNewTemplate()
        {
            _currentTemplate = DefaultTemplates.Classic;
            LoadTemplateToUI(_currentTemplate);
            LoadFieldsList(_currentTemplate);
        }

        private void LoadFieldsList(CertificateTemplateModel template)
        {
            var fieldOrder = template.GetFieldOrderList();
            var fields = new List<CertificateField>();

            var fieldMap = new Dictionary<string, string>
            {
                { "Organizer", "Organizátor" },
                { "EventTopic", "Téma podujatia" },
                { "EventDate", "Dátum podujatia" },
                { "Name", "Meno účastníka" },
                { "BirthDate", "Dátum narodenia" },
                { "RegistrationNumber", "Registračné číslo" },
                { "Notes", "Poznámky" }
            };

            for (int i = 0; i < fieldOrder.Count; i++)
            {
                var fieldId = fieldOrder[i];
                if (fieldMap.ContainsKey(fieldId))
                {
                    fields.Add(new CertificateField
                    {
                        Id = fieldId,
                        DisplayName = fieldMap[fieldId],
                        IsVisible = GetFieldVisibility(template, fieldId),
                        Order = i
                    });
                }
            }

            LstFields.ItemsSource = fields;
        }

        private bool GetFieldVisibility(CertificateTemplateModel template, string fieldId)
        {
            switch (fieldId)
            {
                case "Organizer": return template.ShowOrganizer;
                case "EventTopic": return template.ShowEventTopic;
                case "EventDate": return template.ShowEventDate;
                case "Name": return template.ShowName;
                case "BirthDate": return template.ShowBirthDate;
                case "RegistrationNumber": return template.ShowRegistrationNumber;
                case "Notes": return template.ShowNotes;
                default: return true;
            }
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
                TxtBorderColor.Text = template.BorderColor;

                UpdateColorPreviews();

                // Fonty
                CmbTitleFont.SelectedValue = template.TitleFontFamily;
                CmbHeaderFont.SelectedValue = template.HeaderFontFamily;
                CmbTextFont.SelectedValue = template.TextFontFamily;

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

                // Vlastný text
                TxtCustomHeader.Text = template.CustomHeaderText ?? "";
                TxtCustomFooter.Text = template.CustomFooterText ?? "";

                // Formátovanie záhlavia
                BtnHeaderBold.IsChecked = template.CustomHeaderBold;
                BtnHeaderItalic.IsChecked = template.CustomHeaderItalic;
                if (template.CustomHeaderAlignment == "CENTER")
                    CmbHeaderAlignment.SelectedIndex = 1;
                else if (template.CustomHeaderAlignment == "RIGHT")
                    CmbHeaderAlignment.SelectedIndex = 2;
                else
                    CmbHeaderAlignment.SelectedIndex = 0;

                // Formátovanie pätičky
                BtnFooterBold.IsChecked = template.CustomFooterBold;
                BtnFooterItalic.IsChecked = template.CustomFooterItalic;
                if (template.CustomFooterAlignment == "CENTER")
                    CmbFooterAlignment.SelectedIndex = 1;
                else if (template.CustomFooterAlignment == "RIGHT")
                    CmbFooterAlignment.SelectedIndex = 2;
                else
                    CmbFooterAlignment.SelectedIndex = 0;

                _currentTemplate = template;


                // Layout obsahu
                if (template.ContentLayout == "TWO_COLUMN")
                    CmbContentLayout.SelectedIndex = 1;
                else if (template.ContentLayout == "THREE_COLUMN")
                    CmbContentLayout.SelectedIndex = 2;
                else
                    CmbContentLayout.SelectedIndex = 0;

                TxtColumnSpacing.Text = template.ColumnSpacing.ToString();

                // Zobraz panel pre column layout ak nie je vertical
                PnlColumnLayout.Visibility = (template.ContentLayout != "VERTICAL")
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                Debug.WriteLine($"[LoadTemplateToUI] ContentLayout={template.ContentLayout}, ColumnSpacing={template.ColumnSpacing}");


                // Dekorácie
                ChkShowTopDecoration.IsChecked = template.ShowTopDecoration;
                TxtTopDecorationColor.Text = template.TopDecorationColor;
                TxtTopDecorationThickness.Text = template.TopDecorationThickness.ToString();
                //TxtBottomDecorationThickness.Text = template.BottomDecorationThickness.ToString();
                ChkShowBottomDecoration.IsChecked = template.ShowBottomDecoration;
                TxtBottomDecorationColor.Text = template.BottomDecorationColor;
            }
            finally
            {
                _isLoading = false;
                _hasUnsavedChanges = false;
                UpdatePreview();
            }
        }

        private void UpdateColorPreviews()
        {
            // Null check - TextBoxy môžu byť null počas XAML inicializácie
            if (TxtTitleColor == null || TxtTextColor == null ||
                TxtAccentColor == null || TxtBackgroundColor == null || TxtBorderColor == null)
                return;

            if (TxtTopDecorationColor != null)
                UpdateColorPreview(TopDecorationColorPreview, TxtTopDecorationColor.Text);

            if (TxtBottomDecorationColor != null)
                UpdateColorPreview(BottomDecorationColorPreview, TxtBottomDecorationColor.Text);

            UpdateColorPreview(TitleColorPreview, TxtTitleColor.Text);
            UpdateColorPreview(TextColorPreview, TxtTextColor.Text);
            UpdateColorPreview(AccentColorPreview, TxtAccentColor.Text);
            UpdateColorPreview(BackgroundColorPreview, TxtBackgroundColor.Text);
            UpdateColorPreview(BorderColorPreview, TxtBorderColor.Text);
        }

        private void UpdateColorPreview(Border preview, string colorHex)
        {
            // Null check - Border môže byť null pri inicializácii pred načítaním XAML
            if (preview == null)
                return;

            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
                preview.Background = new SolidColorBrush(color);
            }
            catch
            {
                preview.Background = new SolidColorBrush(Colors.Gray);
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
                BorderColor = TxtBorderColor.Text,

                // Layout obsahu
                ContentLayout = ((ComboBoxItem)CmbContentLayout.SelectedItem)?.Tag?.ToString() ?? "VERTICAL",
                ColumnSpacing = int.TryParse(TxtColumnSpacing.Text, out int colSpacing) ? colSpacing : 20,

                // Písma
                TitleFontFamily = CmbTitleFont.SelectedValue?.ToString() ?? "Helvetica-Bold",
                TitleFontSize = int.TryParse(TxtTitleFontSize.Text, out int titleSize) && titleSize > 0 ? titleSize : 13,
                HeaderFontFamily = CmbHeaderFont.SelectedValue?.ToString() ?? "Helvetica-Bold",
                HeaderFontSize = int.TryParse(TxtHeaderFontSize.Text, out int headerSize) && headerSize > 0 ? headerSize : 10,
                TextFontFamily = CmbTextFont.SelectedValue?.ToString() ?? "Helvetica",
                TextFontSize = int.TryParse(TxtTextFontSize.Text, out int textSize) && textSize > 0 ? textSize : 12,

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
                BorderWidth = int.TryParse(TxtBorderWidth.Text, out int borderWidth) ? borderWidth : 2,

                // Vlastný text
                CustomHeaderText = TxtCustomHeader.Text,
                CustomFooterText = TxtCustomFooter.Text,

                // Formátovanie vlastného textu
                CustomHeaderBold = BtnHeaderBold.IsChecked == true,
                CustomHeaderItalic = BtnHeaderItalic.IsChecked == true,
                CustomHeaderAlignment = ((ComboBoxItem)CmbHeaderAlignment.SelectedItem)?.Tag?.ToString() ?? "LEFT",
                CustomFooterBold = BtnFooterBold.IsChecked == true,
                CustomFooterItalic = BtnFooterItalic.IsChecked == true,
                CustomFooterAlignment = ((ComboBoxItem)CmbFooterAlignment.SelectedItem)?.Tag?.ToString() ?? "LEFT",

                // Dekorácie
                ShowTopDecoration = ChkShowTopDecoration.IsChecked == true,
                TopDecorationColor = TxtTopDecorationColor.Text,
                TopDecorationThickness = int.TryParse(TxtTopDecorationThickness.Text, out int topThickness) && topThickness > 0 ? topThickness : 2,

                ShowBottomDecoration = ChkShowBottomDecoration.IsChecked == true,
                BottomDecorationColor = TxtBottomDecorationColor.Text,
                BottomDecorationThickness = int.TryParse(TxtBottomDecorationThickness.Text, out int bottomThickness) && bottomThickness > 0 ? bottomThickness : 2,
            };

            Debug.WriteLine($"[GetTemplateFromUI] ContentLayout={template.ContentLayout}, ColumnSpacing={template.ColumnSpacing}");

            // Poradie a viditeľnosť polí
            if (LstFields.ItemsSource is List<CertificateField> fields)
            {
                var fieldOrder = fields.Select(f => f.Id).ToList();
                template.SetFieldOrderList(fieldOrder);

                foreach (var field in fields)
                {
                    switch (field.Id)
                    {
                        case "Organizer": template.ShowOrganizer = field.IsVisible; break;
                        case "EventTopic": template.ShowEventTopic = field.IsVisible; break;
                        case "EventDate": template.ShowEventDate = field.IsVisible; break;
                        case "Name": template.ShowName = field.IsVisible; break;
                        case "BirthDate": template.ShowBirthDate = field.IsVisible; break;
                        case "RegistrationNumber": template.ShowRegistrationNumber = field.IsVisible; break;
                        case "Notes": template.ShowNotes = field.IsVisible; break;
                    }
                }
            }

            return template;
        }

        // ===== COLOR PICKER EVENTS =====
        private void TitleColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            ShowColorPicker(TxtTitleColor, TitleColorPreview);
        }

        private void TextColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            ShowColorPicker(TxtTextColor, TextColorPreview);
        }

        private void AccentColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            ShowColorPicker(TxtAccentColor, AccentColorPreview);
        }

        private void BackgroundColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            ShowColorPicker(TxtBackgroundColor, BackgroundColorPreview);
        }

        private void BorderColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            ShowColorPicker(TxtBorderColor, BorderColorPreview);
        }

        private void TopDecorationColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = colorDialog.Color;
                    TxtTopDecorationColor.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                }
            }
        }

        private void BottomDecorationColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = colorDialog.Color;
                    TxtBottomDecorationColor.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                }
            }
        }

        private void ShowColorPicker(System.Windows.Controls.TextBox textBox, Border preview)
        {
            using (var colorDialog = new ColorDialog())
            {
                // Nastav aktuálnu farbu
                try
                {
                    var currentColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(textBox.Text);
                    colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);
                }
                catch { }

                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var selectedColor = colorDialog.Color;
                    string hexColor = $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
                    textBox.Text = hexColor;
                    UpdateColorPreview(preview, hexColor);
                    UpdatePreview();
                }
            }
        }

        #region DRAG & DROP EVENTS 
        private void LstFields_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _draggedItem = null;

            if (e.OriginalSource is FrameworkElement element)
            {
                var item = FindAncestor<ListBoxItem>(element);
                if (item != null)
                {
                    _draggedItem = item.DataContext;
                }
            }
        }

        private void LstFields_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
            {
                System.Windows.Point currentPosition = e.GetPosition(null);
                System.Windows.Vector diff = _dragStartPoint - currentPosition;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(LstFields, _draggedItem, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void LstFields_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.Move;
            e.Handled = true;
        }

        private void LstFields_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (_draggedItem == null) return;

            var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem == null) return;

            var draggedField = _draggedItem as CertificateField;
            var targetField = targetItem.DataContext as CertificateField;

            if (draggedField == null || targetField == null || draggedField == targetField) return;

            var fields = LstFields.ItemsSource as List<CertificateField>;
            if (fields == null) return;

            int draggedIndex = fields.IndexOf(draggedField);
            int targetIndex = fields.IndexOf(targetField);

            fields.RemoveAt(draggedIndex);
            fields.Insert(targetIndex, draggedField);

            // Update order
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].Order = i;
            }

            LstFields.ItemsSource = null;
            LstFields.ItemsSource = fields;

            UpdatePreview();
            _hasUnsavedChanges = true;
        }

        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                    return ancestor;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        // =====  METÓDY PRE ÚPRAVU POLÍ =====

        /// <summary>
        /// Obsluha výberu poľa v ListBoxe
        /// </summary>
        private void LstFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstFields.SelectedItem is CertificateField field)
            {
                _selectedField = field;
                LoadFieldToEditor(field);
                PnlFieldEditor.Visibility = Visibility.Visible;
            }
            else
            {
                _selectedField = null;
                PnlFieldEditor.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Načíta pole do editora
        /// </summary>
        private void LoadFieldToEditor(CertificateField field)
        {
            _isLoading = true;

            TxtFieldLabel.Text = field.CustomLabel ?? GetDefaultLabel(field.Id);
            TxtFieldFontSize.Text = field.FontSize > 0 ? field.FontSize.ToString() : "12";
            TxtFieldColor.Text = field.TextColor ?? "#000000";

            BtnFieldBold.IsChecked = field.IsBold;
            BtnFieldItalic.IsChecked = field.IsItalic;

            // Nastav zarovnanie
            string alignment = field.Alignment ?? "LEFT";
            foreach (ComboBoxItem item in CmbFieldAlignment.Items)
            {
                if (item.Tag.ToString() == alignment)
                {
                    CmbFieldAlignment.SelectedItem = item;
                    break;
                }
            }

            UpdateFieldColorPreview();
            _isLoading = false;
        }

        /// <summary>
        /// Získa predvolený label pre pole
        /// </summary>
        private string GetDefaultLabel(string fieldId)
        {
            var labels = new Dictionary<string, string>
    {
        { "Organizer", "Organizátor:" },
        { "EventTopic", "Téma podujatia:" },
        { "EventDate", "Dátum podujatia:" },
        { "Name", "Účastník:" },
        { "BirthDate", "Dátum narodenia:" },
        { "RegistrationNumber", "Registračné číslo:" },
        { "Notes", "Poznámky:" }
    };

            return labels.ContainsKey(fieldId) ? labels[fieldId] : fieldId;
        }

        /// <summary>
        /// Obsluha zmeny vlastností poľa
        /// </summary>
        private void FieldPropertyChanged(object sender, EventArgs e)
        {
            if (_isLoading || _selectedField == null) return;

            _selectedField.CustomLabel = TxtFieldLabel.Text;

            if (int.TryParse(TxtFieldFontSize.Text, out int fontSize))
                _selectedField.FontSize = fontSize;

            _selectedField.IsBold = BtnFieldBold.IsChecked == true;
            _selectedField.IsItalic = BtnFieldItalic.IsChecked == true;
            _selectedField.TextColor = TxtFieldColor.Text;

            if (CmbFieldAlignment.SelectedItem is ComboBoxItem item)
                _selectedField.Alignment = item.Tag.ToString();

            UpdateFieldColorPreview();
            _hasUnsavedChanges = true;
            UpdatePreview();
        }

        /// <summary>
        /// Aktualizuje náhľad farby pre pole
        /// </summary>
        private void UpdateFieldColorPreview()
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(TxtFieldColor.Text);
                FieldColorPreview.Background = new SolidColorBrush(color);
            }
            catch
            {
                FieldColorPreview.Background = Brushes.Black;
            }
        }

        /// <summary>
        /// Color picker pre farbu poľa
        /// </summary>
        private void FieldColorPreview_Click(object sender, MouseButtonEventArgs e)
        {
            using (var dialog = new ColorDialog())
            {
                var currentColor = (Color)ColorConverter.ConvertFromString(TxtFieldColor.Text);
                dialog.Color = System.Drawing.Color.FromArgb(currentColor.R, currentColor.G, currentColor.B);

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TxtFieldColor.Text = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                }
            }
        }

        // ===== UPRAVENÁ METÓDA LoadFieldsToUI =====
        // Nahraď existujúcu metódu LoadFieldsToUI touto rozšírenou verziou:

        private void LoadFieldsToUI(CertificateTemplateModel template)
        {
            var fieldOrder = template.GetFieldOrderList();
            var fields = new List<CertificateField>();

            var fieldMap = new Dictionary<string, string>
    {
        { "Organizer", "Organizátor" },
        { "EventTopic", "Téma podujatia" },
        { "EventDate", "Dátum podujatia" },
        { "Name", "Meno účastníka" },
        { "BirthDate", "Dátum narodenia" },
        { "RegistrationNumber", "Registračné číslo" },
        { "Notes", "Poznámky" }
    };

            for (int i = 0; i < fieldOrder.Count; i++)
            {
                var fieldId = fieldOrder[i];
                if (fieldMap.ContainsKey(fieldId))
                {
                    var field = new CertificateField
                    {
                        Id = fieldId,
                        DisplayName = fieldMap[fieldId],
                        IsVisible = GetFieldVisibility(template, fieldId),
                        Order = i,
                        CustomLabel = GetFieldCustomLabel(template, fieldId),
                        FontSize = template.TextFontSize,
                        IsBold = false,
                        IsItalic = false,
                        Alignment = "LEFT",
                        TextColor = template.TextColor
                    };
                    fields.Add(field);
                }
            }

            LstFields.ItemsSource = fields;
        }

        /// <summary>
        /// Získa vlastný label pre pole zo šablóny
        /// </summary>
        private string GetFieldCustomLabel(CertificateTemplateModel template, string fieldId)
        {
            switch (fieldId)
            {
                case "Organizer": return template.LabelOrganizer;
                case "EventTopic": return template.LabelEventTopic;
                case "EventDate": return template.LabelEventDate;
                case "Name": return template.LabelParticipant;
                case "BirthDate": return template.LabelBirthDate;
                case "RegistrationNumber": return template.LabelRegistrationNumber;
                case "Notes": return template.LabelNotes;
                default: return GetDefaultLabel(fieldId);
            }
        }

        // ===== UPRAVENÁ METÓDA UpdatePreview =====
        // V existujúcej metóde UpdatePreview() pridaj v cykle cez polia podporu pre vlastné formátovanie:

        /*
        V časti kde sa vytvárajú TextBlocky pre polia, uprav takto:

        foreach (var field in fields)
        {
            if (!field.IsVisible) continue;

            var labelBlock = new TextBlock
            {
                Text = field.CustomLabel,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(field.TextColor)),
                FontSize = field.FontSize,
                FontWeight = field.IsBold ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = field.IsItalic ? FontStyles.Italic : FontStyles.Normal,
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Zarovnanie
            switch (field.Alignment)
            {
                case "CENTER":
                    labelBlock.TextAlignment = TextAlignment.Center;
                    break;
                case "RIGHT":
                    labelBlock.TextAlignment = TextAlignment.Right;
                    break;
                default:
                    labelBlock.TextAlignment = TextAlignment.Left;
                    break;
            }

            PreviewContent.Children.Add(labelBlock);

            // Hodnota poľa (TextBox pre editáciu)
            var valueBox = new TextBox
            {
                Text = GetSampleFieldValue(field.Id),
                FontSize = field.FontSize,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(field.TextColor)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = Brushes.LightGray,
                Background = Brushes.Transparent,
                Margin = new Thickness(0, 0, 0, 10),
                IsReadOnly = true
            };

            PreviewContent.Children.Add(valueBox);
        }
        */

        /// <summary>
        /// Získa ukážkovú hodnotu pre pole
        /// </summary>
        private string GetSampleFieldValue(string fieldId)
        {
            switch (fieldId)
            {
                case "Organizer": return "Vzdelávacia akadémia, s.r.o.";
                case "EventTopic": return "Moderné metódy vzdelávania";
                case "EventDate": return DateTime.Now.ToString("dd.MM.yyyy");
                case "Name": return "Ing. Ján Nk";
                case "BirthDate": return "01.01.1990";
                case "RegistrationNumber": return "12345";
                case "Notes": return "Účastník absolvoval seminár ";
                default: return "[Ukážk hodnota]";
            }
        }

        #endregion

        // ===== UPDATE PREVIEW =====
        private void UpdatePreview()
        {
            if (_isLoading) return;

            try
            {
                PreviewContent.Children.Clear();

                var template = GetTemplateFromUI();

                // Aplikuj pozadie a okraje
                PreviewBorder.Background = new SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.BackgroundColor));
                PreviewBorder.Padding = new Thickness(
                    template.MarginLeft, template.MarginTop,
                    template.MarginRight, template.MarginBottom);

                // Rámček
                if (template.ShowBorder)
                {
                    PreviewBorder.BorderBrush = new SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.BorderColor));
                    PreviewBorder.BorderThickness = new Thickness(template.BorderWidth);
                }
                else
                {
                    PreviewBorder.BorderThickness = new Thickness(0);
                }

                // Vlastné záhlavie
                if (!string.IsNullOrWhiteSpace(template.CustomHeaderText))
                {
                    var headerBlock = new TextBlock
                    {
                        Text = template.CustomHeaderText,
                        FontSize = template.HeaderFontSize,
                        Foreground = new SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.TextColor)),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 15)
                    };

                    if (template.CustomHeaderBold) headerBlock.FontWeight = FontWeights.Bold;
                    if (template.CustomHeaderItalic) headerBlock.FontStyle = FontStyles.Italic;

                    headerBlock.TextAlignment = template.CustomHeaderAlignment == "CENTER" ? TextAlignment.Center :
                                              template.CustomHeaderAlignment == "RIGHT" ? TextAlignment.Right :
                                              TextAlignment.Left;

                    PreviewContent.Children.Add(headerBlock);
                }

                // Titulok
                if (template.ShowTitle)
                {
                    var titleBlock = new TextBlock
                    {
                        Text = template.CertificateTitle,
                        FontSize = template.TitleFontSize,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.TitleColor)),
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    titleBlock.TextAlignment = template.TitleAlignment == "CENTER" ? TextAlignment.Center :
                                             template.TitleAlignment == "RIGHT" ? TextAlignment.Right :
                                             TextAlignment.Left;

                    PreviewContent.Children.Add(titleBlock);
                }

                // Oddeľovač
                if (template.ShowSeparatorLine && template.SeparatorStyle != "NONE")
                {
                    var separator = new Border
                    {
                        Height = 2,
                        Background = new SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.AccentColor)),
                        Margin = new Thickness(0, 10, 0, 20)
                    };
                    PreviewContent.Children.Add(separator);
                }

                // Polia podľa poradia
                if (LstFields.ItemsSource is List<CertificateField> fields)
                {
                    Debug.WriteLine($"[UpdatePreview] ContentLayout={template.ContentLayout}, Počet viditeľných polí={fields.Count(f => f.IsVisible)}");

                    var visibleFields = fields.Where(f => f.IsVisible).ToList();

                    // Rozhodnutie na základe layoutu
                    if (template.ContentLayout == "TWO_COLUMN" || template.ContentLayout == "THREE_COLUMN")
                    {
                        // VIACSTĹPCOVÝ LAYOUT
                        int columnCount = template.ContentLayout == "TWO_COLUMN" ? 2 : 3;
                        Debug.WriteLine($"[UpdatePreview] Použitie {columnCount}-stĺpcového layoutu");

                        // Vytvor Grid pre stĺpce
                        var columnsGrid = new Grid();

                        // Definuj stĺpce
                        for (int i = 0; i < columnCount; i++)
                        {
                            columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                            // Pridaj medzeru medzi stĺpcami (okrem posledného)
                            if (i < columnCount - 1)
                            {
                                columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(template.ColumnSpacing) });
                            }
                        }

                        // Rozdeľ polia do stĺpcov
                        int fieldsPerColumn = (int)Math.Ceiling((double)visibleFields.Count / columnCount);
                        Debug.WriteLine($"[UpdatePreview] Polí na stĺpec: {fieldsPerColumn}");

                        for (int col = 0; col < columnCount; col++)
                        {
                            var columnPanel = new StackPanel();

                            // Vypočítaj rozsah polí pre tento stĺpec
                            int startIdx = col * fieldsPerColumn;
                            int endIdx = Math.Min(startIdx + fieldsPerColumn, visibleFields.Count);

                            Debug.WriteLine($"[UpdatePreview] Stĺpec {col}: polia {startIdx}-{endIdx}");

                            for (int i = startIdx; i < endIdx; i++)
                            {
                                var field = visibleFields[i];
                                AddFieldToPanel(columnPanel, field);
                            }

                            // Pridaj stĺpec do Gridu (Grid.Column = col * 2 kvôli medzerám)
                            Grid.SetColumn(columnPanel, col * 2);
                            columnsGrid.Children.Add(columnPanel);
                        }

                        PreviewContent.Children.Add(columnsGrid);
                    }
                    else
                    {
                        // VERTIKÁLNY LAYOUT (klasický)
                        Debug.WriteLine($"[UpdatePreview] Použitie vertikálneho layoutu");

                        foreach (var field in visibleFields)
                        {
                            AddFieldToPanel(PreviewContent, field);
                        }
                    }
                }

                // Vlastná pätička
                if (!string.IsNullOrWhiteSpace(template.CustomFooterText))
                {
                    var footerBlock = new TextBlock
                    {
                        Text = template.CustomFooterText,
                        FontSize = template.HeaderFontSize,
                        Foreground = new SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.TextColor)),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 15, 0, 0)
                    };

                    if (template.CustomFooterBold) footerBlock.FontWeight = FontWeights.Bold;
                    if (template.CustomFooterItalic) footerBlock.FontStyle = FontStyles.Italic;

                    footerBlock.TextAlignment = template.CustomFooterAlignment == "CENTER" ? TextAlignment.Center :
                                              template.CustomFooterAlignment == "RIGHT" ? TextAlignment.Right :
                                              TextAlignment.Left;

                    PreviewContent.Children.Add(footerBlock);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Chyba pri aktualizácii náhľadu:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        /// <summary>
        /// Pridá pole (label + value) do panelu
        /// </summary>
        private void AddFieldToPanel(System.Windows.Controls.Panel panel, CertificateField field)
        {
            Debug.WriteLine($"[AddFieldToPanel] Pridávam pole: {field.Id} ({field.DisplayName})");

            var labelBlock = new TextBlock
            {
                Text = field.CustomLabel,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(field.TextColor)),
                FontSize = field.FontSize,
                FontWeight = field.IsBold ? FontWeights.Bold : FontWeights.Normal,
                FontStyle = field.IsItalic ? FontStyles.Italic : FontStyles.Normal,
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Zarovnanie
            switch (field.Alignment)
            {
                case "CENTER":
                    labelBlock.TextAlignment = TextAlignment.Center;
                    break;
                case "RIGHT":
                    labelBlock.TextAlignment = TextAlignment.Right;
                    break;
                default:
                    labelBlock.TextAlignment = TextAlignment.Left;
                    break;
            }

            panel.Children.Add(labelBlock);

            // Hodnota poľa (TextBox pre editáciu)
            var valueBox = new System.Windows.Controls.TextBox
            {
                Text = GetSampleFieldValue(field.Id),
                FontSize = field.FontSize,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(field.TextColor)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = Brushes.LightGray,
                Background = Brushes.Transparent,
                Margin = new Thickness(0, 0, 0, 10),
                IsReadOnly = true
            };

            panel.Children.Add(valueBox);
        }

        private void AddPreviewField(string label, string value, CertificateTemplateModel template)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

            var labelBlock = new TextBlock
            {
                Text = label,
                FontSize = template.HeaderFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.AccentColor))
            };

            var valueBlock = new TextBlock
            {
                Text = value,
                FontSize = template.TextFontSize,
                Foreground = new SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(template.TextColor)),
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(labelBlock);
            stackPanel.Children.Add(valueBlock);
            PreviewContent.Children.Add(stackPanel);
        }

        // ===== EVENT HANDLERS =====

        private void DecorationChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdatePreview();
            _hasUnsavedChanges = true;
        }

        private void LayoutChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            UpdatePreview();
            _hasUnsavedChanges = true;
        }

        private void BackgroundChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdatePreview();
            _hasUnsavedChanges = true;
        }

        private void GradientStartPreview_Click(object sender, MouseButtonEventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = colorDialog.Color;
                    TxtGradientStart.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                }
            }
        }

        private void GradientEndPreview_Click(object sender, MouseButtonEventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = colorDialog.Color;
                    TxtGradientEnd.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                }
            }
        }

        private void CmbTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            if (CmbTemplates.SelectedItem is CertificateTemplateModel template)
            {
                LoadTemplateToUI(template);
                LoadFieldsList(template);
            }
        }

        private void CmbContentLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;

            if (CmbContentLayout.SelectedItem is ComboBoxItem item)
            {
                string layout = item.Tag?.ToString() ?? "VERTICAL";
                Debug.WriteLine($"[CmbContentLayout_SelectionChanged] Zvolený layout: {layout}");

                // Zobraz/skry panel s nastaveniami stĺpcov
                PnlColumnLayout.Visibility = (layout != "VERTICAL")
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            UpdatePreview();
            _hasUnsavedChanges = true;
        }

        private void ColorChanged(object sender, TextChangedEventArgs e)
        {
            UpdateColorPreviews();
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void FontChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void FontSizeChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void MarginChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void BorderSettingsChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void BorderSettingsChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void TitleSettingsChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void TitleSettingsChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void TitleSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void SeparatorSettingsChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void SeparatorSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void FieldVisibilityChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void CustomTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void HeaderFormattingChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void HeaderFormattingChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void FooterFormattingChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void FooterFormattingChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void StampChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void StampChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void SignatureChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void SignatureChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void LogoChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void LogoChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            AnySettingChanged(sender, e);
        }

        private void RefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        // ===== TEMPLATE MANAGEMENT =====
        private void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTemplateName.Text))
            {
                System.Windows.MessageBox.Show("Prosím zadajte názov šablóny.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var template = GetTemplateFromUI();

                if (template.Id > 0)
                {
                    _templateRepo.Update(template);
                    System.Windows.MessageBox.Show("Šablóna bola úspešne aktualizovaná.",
                        "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    int id = _templateRepo.Add(template);
                    template.Id = id;
                    _currentTemplate = template;
                    System.Windows.MessageBox.Show("Šablóna bola úspešne uložená.",
                        "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadTemplates();
                _hasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Chyba pri ukladaní šablóny:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadNewTemplate();
            CmbTemplates.SelectedIndex = -1;
            _hasUnsavedChanges = false;
        }

        private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTemplates.SelectedItem is CertificateTemplateModel template && template.Id > 0)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Naozaj chcete odstrániť šablónu '{template.Name}'?",
                    "Potvrdenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _templateRepo.Delete(template.Id);
                        System.Windows.MessageBox.Show("Šablóna bola úspešne odstránená.",
                            "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTemplates();
                        LoadNewTemplate();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Chyba pri odstraňovaní šablóny:\n{ex.Message}",
                            "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Prosím vyberte uloženú šablónu na odstránenie.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SelectStamp_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Obrázkové súbory (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Vyberte pečiatku"
            };
            if (openDialog.ShowDialog() == true)
            {
                TxtStampPath.Text = openDialog.FileName;
            }
        }

        private void SelectSignature_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Obrázkové súbory (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Vyberte podpis"
            };
            if (openDialog.ShowDialog() == true)
            {
                TxtSignaturePath.Text = openDialog.FileName;
            }
        }

        private void SelectLogo_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Obrázkové súbory (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Vyberte logo"
            };
            if (openDialog.ShowDialog() == true)
            {
                TxtLogoPath.Text = openDialog.FileName;
            }
        }

        private void LoadClassicTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Classic);
            LoadFieldsList(DefaultTemplates.Classic);
            CmbTemplates.SelectedIndex = -1;
        }

        private void LoadModernTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Modern);
            LoadFieldsList(DefaultTemplates.Modern);
            CmbTemplates.SelectedIndex = -1;
        }

        private void LoadElegantTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Elegant);
            LoadFieldsList(DefaultTemplates.Elegant);
            CmbTemplates.SelectedIndex = -1;
        }

        private void LoadMinimalistTemplate_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateToUI(DefaultTemplates.Minimalist);
            LoadFieldsList(DefaultTemplates.Minimalist);
            CmbTemplates.SelectedIndex = -1;
        }

        private void TestPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
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
                        "Ing. Ján Nk",
                        new DateTime(1990, 1, 1),
                        "12345",
                        "Toto je ukážkový text poznámky ku certifikátu. Môže obsahovať dodatočné informácie o absolvovanom podujatí.",
                        PageSize.A5
                    );

                    var result = System.Windows.MessageBox.Show(
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
                System.Windows.MessageBox.Show($"Chyba pri vytváraní test PDF:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Model pre šablónu certifikátu - rozšírená verzia
    /// </summary>
    public class CertificateTemplateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }

        // Farby
        public string TitleColor { get; set; } = "#000000";
        public string TextColor { get; set; } = "#000000";
        public string AccentColor { get; set; } = "#2563EB";
        public string BackgroundColor { get; set; } = "#FFFFFF";

        // Písma
        public string TitleFontFamily { get; set; } = "Helvetica-Bold";
        public int TitleFontSize { get; set; } = 20;

        public string HeaderFontFamily { get; set; } = "Helvetica-Bold";
        public int HeaderFontSize { get; set; } = 12;

        public string TextFontFamily { get; set; } = "Helvetica";
        public int TextFontSize { get; set; } = 10;

        // Rozloženie
        public int MarginTop { get; set; } = 30;
        public int MarginRight { get; set; } = 30;
        public int MarginBottom { get; set; } = 30;
        public int MarginLeft { get; set; } = 30;

        public string TitleAlignment { get; set; } = "CENTER";
        public bool ShowSeparatorLine { get; set; } = true;
        public string SeparatorStyle { get; set; } = "UNDERLINE"; // UNDERLINE, LINE, NONE

        // Logo/Obrázok
        public bool ShowLogo { get; set; } = false;
        public string LogoPath { get; set; }
        public string LogoPosition { get; set; } = "TOP"; // TOP, BOTTOM, NONE
        public int LogoWidth { get; set; } = 100;
        public int LogoHeight { get; set; } = 100;

        // Záhlavie certifikátu
        public string CertificateTitle { get; set; } = "CERTIFIKÁT O ABSOLVOVANÍ public string CertificateTitle { get; set; }";
        public bool ShowTitle { get; set; } = true;

        // Viditeľnosť polí
        public bool ShowOrganizer { get; set; } = true;
        public bool ShowEventTopic { get; set; } = true;
        public bool ShowEventDate { get; set; } = true;
        public bool ShowName { get; set; } = true;
        public bool ShowBirthDate { get; set; } = true;
        public bool ShowRegistrationNumber { get; set; } = true;
        public bool ShowNotes { get; set; } = true;

        //  Poradie polí (serialized as comma-separated string)
        public string FieldOrder { get; set; } = "Organizer,EventTopic,EventDate,Name,BirthDate,RegistrationNumber,Notes";

        // Vlastný text
        public string CustomHeaderText { get; set; }
        public string CustomFooterText { get; set; }

        //  Formátovanie vlastného textu
        public string CustomHeaderAlignment { get; set; } = "LEFT"; // LEFT, CENTER, RIGHT
        public bool CustomHeaderBold { get; set; } = false;
        public bool CustomHeaderItalic { get; set; } = false;

        public string CustomFooterAlignment { get; set; } = "LEFT";
        public bool CustomFooterBold { get; set; } = false;
        public bool CustomFooterItalic { get; set; } = false;

        // Štýl rámčeka
        public bool ShowBorder { get; set; } = false;
        public string BorderColor { get; set; } = "#000000";
        public int BorderWidth { get; set; } = 2;

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Predvolené texty polí (pre preklady/prispôsobenie)
        public string LabelOrganizer { get; set; } = "Organizátor:";
        public string LabelEventTopic { get; set; } = "Téma podujatia:";
        public string LabelParticipant { get; set; } = "Účastník:";
        public string LabelEventDate { get; set; } = "Dátum podujatia:";
        public string LabelBirthDate { get; set; } = "Dátum narodenia:";
        public string LabelRegistrationNumber { get; set; } = "Registračné číslo v komore:";
        public string LabelNotes { get; set; } = "Poznámky:";

        // Dekor písma 
        public bool TitleUnderlined { get; set; }

        // Dekoratívne prvky
        public bool ShowTopDecoration { get; set; } = false;
        public string TopDecorationType { get; set; } = "WAVY_LINE"; // WAVY_LINE, STRAIGHT_LINE, DIAGONAL_LINES, CORNER_ORNAMENTS
        public string TopDecorationColor { get; set; } = "#2563EB";
        public int TopDecorationThickness { get; set; } = 2;

        public bool ShowBottomDecoration { get; set; } = false;
        public string BottomDecorationType { get; set; } = "WAVY_LINE";
        public string BottomDecorationColor { get; set; } = "#2563EB";
        public int BottomDecorationThickness { get; set; } = 2;

        public bool ShowSideDecorations { get; set; } = false;
        public string SideDecorationType { get; set; } = "VERTICAL_LINE"; // VERTICAL_LINE, ORNAMENTAL_BORDER
        public string SideDecorationColor { get; set; } = "#2563EB";

        // Pokročilý layout
        public string ContentLayout { get; set; } = "VERTICAL"; // VERTICAL, TWO_COLUMN, THREE_COLUMN
        public int ColumnSpacing { get; set; } = 20;

        // Pozadie
        public bool UseGradientBackground { get; set; } = false;
        public string BackgroundGradientStart { get; set; } = "#FFFFFF";
        public string BackgroundGradientEnd { get; set; } = "#F3F4F6";
        public string GradientDirection { get; set; } = "VERTICAL"; // VERTICAL, HORIZONTAL, DIAGONAL
        public string EventLocation { get; internal set; }
        public bool UseCaduceusStyle { get; internal set; }

        // Helper metódy pre poradie polí
        public List<string> GetFieldOrderList()
        {
            if (string.IsNullOrWhiteSpace(FieldOrder))
                return new List<string> { "Organizer", "EventTopic", "EventDate", "Name", "BirthDate", "RegistrationNumber", "Notes" };

            return FieldOrder.Split(',').ToList();
        }

        public void SetFieldOrderList(List<string> fields)
        {
            FieldOrder = string.Join(",", fields);
        }
    }

    /// <summary>
    /// Helper class pre pole certifikátu s možnosťou drag&drop
    /// </summary>
    public class CertificateField : System.ComponentModel.INotifyPropertyChanged
    {
        private string _id;
        private string _displayName;
        private bool _isVisible;
        private int _order;
        private string _customLabel;
        private int _fontSize;
        private bool _isBold;
        private bool _isItalic;
        private string _alignment;
        private string _textColor;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(nameof(DisplayName)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public int Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(nameof(Order)); }
        }

        public string CustomLabel
        {
            get => _customLabel ?? DisplayName;
            set { _customLabel = value; OnPropertyChanged(nameof(CustomLabel)); }
        }

        public int FontSize
        {
            get => _fontSize > 0 ? _fontSize : 10;
            set { _fontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }

        public bool IsBold
        {
            get => _isBold;
            set { _isBold = value; OnPropertyChanged(nameof(IsBold)); }
        }

        public bool IsItalic
        {
            get => _isItalic;
            set { _isItalic = value; OnPropertyChanged(nameof(IsItalic)); }
        }

        public string Alignment
        {
            get => _alignment ?? "LEFT";
            set { _alignment = value; OnPropertyChanged(nameof(Alignment)); }
        }

        public string TextColor
        {
            get => _textColor ?? "#000000";
            set { _textColor = value; OnPropertyChanged(nameof(TextColor)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Predvolené šablóny
    /// </summary>
    public static class DefaultTemplates
    {
        public static CertificateTemplateModel Classic => new CertificateTemplateModel
        {
            Name = "Klasická",
            TitleColor = "#000000",
            TextColor = "#000000",
            AccentColor = "#2563EB",
            TitleFontSize = 13,
            HeaderFontSize = 10,
            TextFontSize = 12,
            ShowSeparatorLine = true,
            SeparatorStyle = "UNDERLINE"
        };

        public static CertificateTemplateModel Modern => new CertificateTemplateModel
        {
            Name = "Moderná",
            TitleColor = "#1E293B",
            TextColor = "#334155",
            AccentColor = "#10B981",
            TitleFontSize = 20,
            HeaderFontSize = 9,
            TextFontSize = 13,
            ShowSeparatorLine = true,
            SeparatorStyle = "LINE",
            ShowBorder = true,
            BorderColor = "#E2E8F0",
            BorderWidth = 1
        };

        public static CertificateTemplateModel Elegant => new CertificateTemplateModel
        {
            Name = "Elegantná",
            TitleColor = "#4B5563",
            TextColor = "#6B7280",
            AccentColor = "#8B5CF6",
            TitleFontSize = 22,
            HeaderFontSize = 12,
            TextFontSize = 10,
            ShowSeparatorLine = false,
            ShowBorder = true,
            BorderColor = "#8B5CF6",
            BorderWidth = 3,
            MarginTop = 40,
            MarginRight = 40,
            MarginBottom = 40,
            MarginLeft = 40
        };

        public static CertificateTemplateModel Minimalist => new CertificateTemplateModel
        {
            Name = "Minimalistická",
            TitleColor = "#000000",
            TextColor = "#374151",
            AccentColor = "#000000",
            TitleFontSize = 18,
            HeaderFontSize = 11,
            TextFontSize = 9,
            ShowSeparatorLine = false,
            MarginTop = 50,
            MarginRight = 50,
            MarginBottom = 50,
            MarginLeft = 50
        };

        public static List<CertificateTemplateModel> GetAll()
        {
            return new List<CertificateTemplateModel>
            {
                Classic,
                Modern,
                Elegant,
                Minimalist
            };
        }
    }

    /// <summary>
    /// Dostupné fonty pre PDF generovanie (iText podporované)
    /// </summary>
    public static class AvailableFonts
    {
        public static List<FontInfo> GetAll()
        {
            return new List<FontInfo>
            {
                // === WINDOWS SYSTÉMOVÉ FONTY (odporúčané pre slovenčinu) ===
                new FontInfo { Name = "Arial", DisplayName = "Arial (Odporúčaný - výborná diakritika)" },
                new FontInfo { Name = "Arial-Bold", DisplayName = "Arial Bold" },
                new FontInfo { Name = "Arial-Italic", DisplayName = "Arial Italic" },
                new FontInfo { Name = "Arial-BoldItalic", DisplayName = "Arial Bold Italic" },

                new FontInfo { Name = "Calibri", DisplayName = "Calibri (Moderný, čistý)" },
                new FontInfo { Name = "Calibri-Bold", DisplayName = "Calibri Bold" },
                new FontInfo { Name = "Calibri-Italic", DisplayName = "Calibri Italic" },
                new FontInfo { Name = "Calibri-BoldItalic", DisplayName = "Calibri Bold Italic" },

                new FontInfo { Name = "SegoeUI", DisplayName = "Segoe UI (Windows moderný)" },
                new FontInfo { Name = "SegoeUI-Bold", DisplayName = "Segoe UI Bold" },
                new FontInfo { Name = "SegoeUI-Italic", DisplayName = "Segoe UI Italic" },
                new FontInfo { Name = "SegoeUI-BoldItalic", DisplayName = "Segoe UI Bold Italic" },

                new FontInfo { Name = "Verdana", DisplayName = "Verdana (Výborná čitateľnosť)" },
                new FontInfo { Name = "Verdana-Bold", DisplayName = "Verdana Bold" },
                new FontInfo { Name = "Verdana-Italic", DisplayName = "Verdana Italic" },
                new FontInfo { Name = "Verdana-BoldItalic", DisplayName = "Verdana Bold Italic" },

                new FontInfo { Name = "Tahoma", DisplayName = "Tahoma (Kompaktný)" },
                new FontInfo { Name = "Tahoma-Bold", DisplayName = "Tahoma Bold" },

                new FontInfo { Name = "Georgia", DisplayName = "Georgia (Elegantný serif)" },
                new FontInfo { Name = "Georgia-Bold", DisplayName = "Georgia Bold" },
                new FontInfo { Name = "Georgia-Italic", DisplayName = "Georgia Italic" },
                new FontInfo { Name = "Georgia-BoldItalic", DisplayName = "Georgia Bold Italic" },
                
                // === OPEN-SOURCE FONTY (ak sú nainštalované) ===
                new FontInfo { Name = "DejaVuSans", DisplayName = "DejaVu Sans" },
                new FontInfo { Name = "DejaVuSans-Bold", DisplayName = "DejaVu Sans Bold" },
                new FontInfo { Name = "DejaVuSans-Oblique", DisplayName = "DejaVu Sans Italic" },
                new FontInfo { Name = "DejaVuSans-BoldOblique", DisplayName = "DejaVu Sans Bold Italic" },

                new FontInfo { Name = "LiberationSans", DisplayName = "Liberation Sans" },
                new FontInfo { Name = "LiberationSans-Bold", DisplayName = "Liberation Sans Bold" },
                new FontInfo { Name = "LiberationSans-Italic", DisplayName = "Liberation Sans Italic" },
                new FontInfo { Name = "LiberationSans-BoldItalic", DisplayName = "Liberation Sans Bold Italic" },
                
                // === ŠTANDARDNÉ PDF FONTY (obmedzená diakritika) ===
                new FontInfo { Name = "Helvetica", DisplayName = "Helvetica (Štandardný PDF - slabšia diakritika)" },
                new FontInfo { Name = "Helvetica-Bold", DisplayName = "Helvetica Bold" },
                new FontInfo { Name = "Helvetica-Oblique", DisplayName = "Helvetica Italic" },
                new FontInfo { Name = "Helvetica-BoldOblique", DisplayName = "Helvetica Bold Italic" },

                new FontInfo { Name = "Times-Roman", DisplayName = "Times New Roman (PDF - slabšia diakritika)" },
                new FontInfo { Name = "Times-Bold", DisplayName = "Times Bold" },
                new FontInfo { Name = "Times-Italic", DisplayName = "Times Italic" },
                new FontInfo { Name = "Times-BoldItalic", DisplayName = "Times Bold Italic" },

                new FontInfo { Name = "Courier", DisplayName = "Courier (Monospace)" },
                new FontInfo { Name = "Courier-Bold", DisplayName = "Courier Bold" },
                new FontInfo { Name = "Courier-Oblique", DisplayName = "Courier Italic" },
                new FontInfo { Name = "Courier-BoldOblique", DisplayName = "Courier Bold Italic" }
            };
        }
    }

    public class FontInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}

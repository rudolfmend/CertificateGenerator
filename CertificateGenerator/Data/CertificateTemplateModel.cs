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
        public string LogoPath { get; set; }
        public string LogoPosition { get; set; } = "TOP"; // TOP, BOTTOM, NONE
        public int LogoWidth { get; set; } = 100;
        public int LogoHeight { get; set; } = 100;

        // Záhlavie certifikátu
        public string CertificateTitle { get; set; } = "CERTIFIKÁT O ABSOLVOVANÍ";
        public bool ShowTitle { get; set; } = true;

        // Viditeľnosť polí
        public bool ShowOrganizer { get; set; } = true;
        public bool ShowEventTopic { get; set; } = true;
        public bool ShowEventDate { get; set; } = true;
        public bool ShowBirthDate { get; set; } = true;
        public bool ShowRegistrationNumber { get; set; } = true;
        public bool ShowNotes { get; set; } = true;

        //  Poradie polí (serialized as comma-separated string)
        public string FieldOrder { get; set; } = "Organizer,EventTopic,EventDate,BirthDate,RegistrationNumber,Notes";

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

        // Helper metódy pre poradie polí
        public List<string> GetFieldOrderList()
        {
            if (string.IsNullOrWhiteSpace(FieldOrder))
                return new List<string> { "Organizer", "EventTopic", "EventDate", "BirthDate", "RegistrationNumber", "Notes" };

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
    public class CertificateField
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsVisible { get; set; }
        public int Order { get; set; }
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
            TitleFontSize = 20,
            HeaderFontSize = 12,
            TextFontSize = 10,
            ShowSeparatorLine = true,
            SeparatorStyle = "UNDERLINE"
        };

        public static CertificateTemplateModel Modern => new CertificateTemplateModel
        {
            Name = "Moderná",
            TitleColor = "#1E293B",
            TextColor = "#334155",
            AccentColor = "#10B981",
            TitleFontSize = 24,
            HeaderFontSize = 13,
            TextFontSize = 11,
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
                new FontInfo { Name = "Helvetica", DisplayName = "Helvetica (Štandardný)" },
                new FontInfo { Name = "Helvetica-Bold", DisplayName = "Helvetica Bold" },
                new FontInfo { Name = "Helvetica-Oblique", DisplayName = "Helvetica Italic" },
                new FontInfo { Name = "Helvetica-BoldOblique", DisplayName = "Helvetica Bold Italic" },
                new FontInfo { Name = "Times-Roman", DisplayName = "Times New Roman" },
                new FontInfo { Name = "Times-Bold", DisplayName = "Times Bold" },
                new FontInfo { Name = "Times-Italic", DisplayName = "Times Italic" },
                new FontInfo { Name = "Times-BoldItalic", DisplayName = "Times Bold Italic" },
                new FontInfo { Name = "Courier", DisplayName = "Courier" },
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

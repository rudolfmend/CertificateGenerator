using System;
using System.Collections.Generic;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Model pre šablónu certifikátu
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

        // Vlastný text
        public string CustomHeaderText { get; set; }
        public string CustomFooterText { get; set; }

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
}


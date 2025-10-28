using System;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Extension metódy pre CertificateTemplateModel
    /// </summary>
    public static class CertificateTemplateModelExtensions
    {
        /// <summary>
        /// Vytvorí hlbokú kópiu šablóny certifikátu
        /// </summary>
        public static CertificateTemplateModel Clone(this CertificateTemplateModel source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new CertificateTemplateModel
            {
                // Základné info (ID sa nekopíruje - nová šablóna dostane nové ID)
                Name = source.Name,
                IsDefault = false, // Kópia nie je predvolená

                // Farby
                TitleColor = source.TitleColor,
                TextColor = source.TextColor,
                AccentColor = source.AccentColor,
                BackgroundColor = source.BackgroundColor,

                // Písma
                TitleFontFamily = source.TitleFontFamily,
                TitleFontSize = source.TitleFontSize,
                HeaderFontFamily = source.HeaderFontFamily,
                HeaderFontSize = source.HeaderFontSize,
                TextFontFamily = source.TextFontFamily,
                TextFontSize = source.TextFontSize,

                // Rozloženie
                MarginTop = source.MarginTop,
                MarginRight = source.MarginRight,
                MarginBottom = source.MarginBottom,
                MarginLeft = source.MarginLeft,
                TitleAlignment = source.TitleAlignment,
                ShowSeparatorLine = source.ShowSeparatorLine,
                SeparatorStyle = source.SeparatorStyle,

                // Logo
                LogoPath = source.LogoPath,
                LogoPosition = source.LogoPosition,
                LogoWidth = source.LogoWidth,
                LogoHeight = source.LogoHeight,

                // Záhlavie
                CertificateTitle = source.CertificateTitle,
                ShowTitle = source.ShowTitle,

                // Viditeľnosť polí
                ShowOrganizer = source.ShowOrganizer,
                ShowEventTopic = source.ShowEventTopic,
                ShowEventDate = source.ShowEventDate,
                ShowName = source.ShowName,
                ShowBirthDate = source.ShowBirthDate,
                ShowRegistrationNumber = source.ShowRegistrationNumber,
                ShowNotes = source.ShowNotes,

                // Vlastné texty
                CustomHeaderText = source.CustomHeaderText,
                CustomFooterText = source.CustomFooterText,

                // Rám
                ShowBorder = source.ShowBorder,
                BorderColor = source.BorderColor,
                BorderWidth = source.BorderWidth,

                // Popisky
                LabelOrganizer = source.LabelOrganizer,
                LabelEventTopic = source.LabelEventTopic,
                LabelParticipant = source.LabelParticipant,
                LabelEventDate = source.LabelEventDate,
                LabelBirthDate = source.LabelBirthDate,
                LabelRegistrationNumber = source.LabelRegistrationNumber,
                LabelNotes = source.LabelNotes,

                // Layout a poradi
                FieldOrder = source.FieldOrder,
                ContentLayout = source.ContentLayout,
                CustomHeaderAlignment = source.CustomHeaderAlignment,
                CustomHeaderBold = source.CustomHeaderBold,
                CustomHeaderItalic = source.CustomHeaderItalic,
                CustomFooterAlignment = source.CustomFooterAlignment,
                CustomFooterBold = source.CustomFooterBold,
                CustomFooterItalic = source.CustomFooterItalic,

                // Dekorácie
                ShowTopDecoration = source.ShowTopDecoration,
                TopDecorationColor = source.TopDecorationColor,
                TopDecorationThickness = source.TopDecorationThickness,
                ShowBottomDecoration = source.ShowBottomDecoration,
                BottomDecorationColor = source.BottomDecorationColor,
                BottomDecorationThickness = source.BottomDecorationThickness
            };
        }
    }
}

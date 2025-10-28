using System.Collections.Generic;
using CertificateGenerator.Data;

namespace CertificateGenerator.Helpers
{
    /// <summary>
    /// Moderné preddefinované šablóny certifikátov podľa aktuálnych dizajnových trendov
    /// </summary>
    public static class ModernTemplatePresets
    {
        /// <summary>
        /// Získa všetky dostupné moderné šablóny
        /// </summary>
        public static List<TemplatePreset> GetAllPresets()
        {
            return new List<TemplatePreset>
            {
                GetMinimalistBluePreset(),
                GetElegantGoldPreset(),
                GetModernGradientPreset(),
                GetProfessionalDarkPreset(),
                GetCreativeColorfulPreset(),
                GetClassicBorderedPreset()
            };
        }

        /// <summary>
        /// Minimalistická modrá šablóna - čistý, profesionálny dizajn
        /// </summary>
        public static TemplatePreset GetMinimalistBluePreset()
        {
            return new TemplatePreset
            {
                Name = "Minimalistická modrá",
                Description = "Čistý a profesionálny dizajn s modernými modrými akcentmi",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/minimalist_blue.png",
                Category = "Moderný",
                Template = new CertificateTemplateModel
                {
                    Name = "Minimalistická modrá",

                    // Farby
                    TitleColor = "#1E3A8A", // Tmavo modrá
                    TextColor = "#1F2937", // Takmer čierna
                    AccentColor = "#3B82F6", // Jasná modrá
                    BackgroundColor = "#FFFFFF",

                    // Fonty
                    TitleFontFamily = "Helvetica-Bold",
                    TitleFontSize = 36,
                    HeaderFontFamily = "Helvetica-Bold",
                    HeaderFontSize = 14,
                    TextFontFamily = "Helvetica",
                    TextFontSize = 12,

                    // Okraje
                    MarginTop = 80,
                    MarginRight = 60,
                    MarginBottom = 80,
                    MarginLeft = 60,

                    // Titulok
                    TitleAlignment = "CENTER",
                    CertificateTitle = "CERTIFIKÁT O ÚČASTI",
                    ShowTitle = true,

                    // Oddeľovacia čiara
                    ShowSeparatorLine = true,
                    SeparatorStyle = "UNDERLINE",

                    // Dekorácie
                    ShowTopDecoration = true,
                    TopDecorationColor = "#3B82F6",
                    TopDecorationThickness = 3,
                    ShowBottomDecoration = true,
                    BottomDecorationColor = "#3B82F6",
                    BottomDecorationThickness = 3,

                    // Viditeľnosť polí
                    ShowOrganizer = true,
                    ShowEventTopic = true,
                    ShowEventDate = true,
                    ShowName = true,
                    ShowBirthDate = false,
                    ShowRegistrationNumber = false,
                    ShowNotes = false,

                    // Layout
                    ContentLayout = "VERTICAL",
                    FieldOrder = "Name,EventTopic,EventDate,Organizer",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 120,
                    LogoHeight = 120,

                    // Border
                    ShowBorder = false
                }
            };
        }

        /// <summary>
        /// Elegantná zlatá šablóna - luxusný vzhľad
        /// </summary>
        public static TemplatePreset GetElegantGoldPreset()
        {
            return new TemplatePreset
            {
                Name = "Elegantná zlatá",
                Description = "Luxusný dizajn so zlatými dekoráciami a elegantnou typografiou",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/elegant_gold.png",
                Category = "Luxusný",
                Template = new CertificateTemplateModel
                {
                    Name = "Elegantná zlatá",

                    // Farby
                    TitleColor = "#92400E", // Tmavé zlato/hnedá
                    TextColor = "#1C1917", // Takmer čierna
                    AccentColor = "#D97706", // Zlatá
                    BackgroundColor = "#FFFBEB", // Krémová

                    // Fonty
                    TitleFontFamily = "Times-Bold",
                    TitleFontSize = 15,
                    HeaderFontFamily = "Times-Bold",
                    HeaderFontSize = 10,
                    TextFontFamily = "Times-Roman",
                    TextFontSize = 14,

                    // Okraje
                    MarginTop = 30,
                    MarginRight = 70,
                    MarginBottom = 90,
                    MarginLeft = 70,

                    // Titulok
                    TitleAlignment = "CENTER",
                    CertificateTitle = "POTVRDENIE O ÚČASTI NA ODBORNOM SEMINÁRI \n Cumulus, s.r.o. Rastislavova 45, Košice 040 01",
                    ShowTitle = true,

                    // Oddeľovacia čiara
                    ShowSeparatorLine = true,
                    SeparatorStyle = "DOUBLE_LINE",

                    // Dekorácie
                    ShowTopDecoration = true,
                    TopDecorationColor = "#D97706",
                    TopDecorationThickness = 2,
                    ShowBottomDecoration = true,
                    BottomDecorationColor = "#D97706",
                    BottomDecorationThickness = 2,

                    // Border
                    ShowBorder = true,
                    BorderColor = "#D97706",
                    BorderWidth = 2,

                    // Viditeľnosť polí
                    ShowOrganizer = true,
                    ShowEventTopic = true,
                    ShowEventDate = true,
                    ShowName = true,
                    ShowBirthDate = false,
                    ShowRegistrationNumber = true,
                    ShowNotes = false,

                    // Layout
                    ContentLayout = "VERTICAL",
                    FieldOrder = "Name,EventTopic,EventDate,RegistrationNumber,Organizer",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 100,
                    LogoHeight = 100
                }
            };
        }

        /// <summary>
        /// Moderná gradientová šablóna - súčasný trend
        /// </summary>
        public static TemplatePreset GetModernGradientPreset()
        {
            return new TemplatePreset
            {
                Name = "Moderný gradient",
                Description = "Súčasný dizajn s farebnými akcentmi a čistým layoutom",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/modern_gradient.png",
                Category = "Moderný",
                Template = new CertificateTemplateModel
                {
                    Name = "Moderný gradient",

                    // Farby
                    TitleColor = "#7C3AED", // Fialová
                    TextColor = "#374151", // Sivá
                    AccentColor = "#8B5CF6", // Svetlá fialová
                    BackgroundColor = "#FFFFFF",

                    // Fonty
                    TitleFontFamily = "Helvetica-Bold",
                    TitleFontSize = 40,
                    HeaderFontFamily = "Helvetica-Bold",
                    HeaderFontSize = 13,
                    TextFontFamily = "Helvetica",
                    TextFontSize = 11,

                    // Okraje
                    MarginTop = 70,
                    MarginRight = 60,
                    MarginBottom = 70,
                    MarginLeft = 60,

                    // Titulok
                    TitleAlignment = "LEFT",
                    CertificateTitle = "CERTIFIKÁT O ÚČASTI",
                    ShowTitle = true,

                    // Oddeľovacia čiara
                    ShowSeparatorLine = false,

                    // Dekorácie
                    ShowTopDecoration = true,
                    TopDecorationColor = "#8B5CF6",
                    TopDecorationThickness = 8,
                    ShowBottomDecoration = false,

                    // Viditeľnosť polí
                    ShowOrganizer = true,
                    ShowEventTopic = true,
                    ShowEventDate = true,
                    ShowName = true,
                    ShowBirthDate = false,
                    ShowRegistrationNumber = false,
                    ShowNotes = false,

                    // Layout
                    ContentLayout = "VERTICAL",
                    FieldOrder = "Name,EventTopic,EventDate,Organizer",

                    // Logo
                    LogoPosition = "TOP_RIGHT",
                    LogoWidth = 80,
                    LogoHeight = 80,

                    // Border
                    ShowBorder = false,

                    // Custom texty
                    CustomHeaderText = null,
                    CustomFooterText = "Tento certifikát potvrdzuje účasť na odbornom seminári"
                }
            };
        }

        /// <summary>
        /// Profesionálna tmavá šablóna - formálny vzhľad
        /// </summary>
        public static TemplatePreset GetProfessionalDarkPreset()
        {
            return new TemplatePreset
            {
                Name = "Profesionálna tmavá",
                Description = "Formálny dizajn s tmavými farbami pre oficiálne dokumenty",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/professional_dark.png",
                Category = "Profesionálny",
                Template = new CertificateTemplateModel
                {
                    Name = "Profesionálna tmavá",

                    // Farby
                    TitleColor = "#111827", // Čierna
                    TextColor = "#374151", // Tmavá sivá
                    AccentColor = "#1F2937", // Tmavá
                    BackgroundColor = "#FFFFFF",

                    // Fonty
                    TitleFontFamily = "Helvetica-Bold",
                    TitleFontSize = 38,
                    HeaderFontFamily = "Helvetica-Bold",
                    HeaderFontSize = 13,
                    TextFontFamily = "Helvetica",
                    TextFontSize = 11,

                    // Okraje
                    MarginTop = 100,
                    MarginRight = 80,
                    MarginBottom = 100,
                    MarginLeft = 80,

                    // Titulok
                    TitleAlignment = "CENTER",
                    CertificateTitle = "CERTIFIKÁT",
                    ShowTitle = true,

                    // Oddeľovacia čiara
                    ShowSeparatorLine = true,
                    SeparatorStyle = "SINGLE_LINE",

                    // Dekorácie
                    ShowTopDecoration = false,
                    ShowBottomDecoration = false,

                    // Border
                    ShowBorder = true,
                    BorderColor = "#1F2937",
                    BorderWidth = 2,

                    // Viditeľnosť polí
                    ShowOrganizer = true,
                    ShowEventTopic = true,
                    ShowEventDate = true,
                    ShowName = true,
                    ShowBirthDate = true,
                    ShowRegistrationNumber = true,
                    ShowNotes = false,

                    // Layout
                    ContentLayout = "TWO_COLUMN",
                    FieldOrder = "Name,BirthDate,RegistrationNumber,EventTopic,EventDate,Organizer",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 90,
                    LogoHeight = 90
                }
            };
        }

        /// <summary>
        /// Kreatívna farebná šablóna - živý dizajn
        /// </summary>
        public static TemplatePreset GetCreativeColorfulPreset()
        {
            return new TemplatePreset
            {
                Name = "Kreatívna farebná",
                Description = "Živý a moderný dizajn s farebnými prvkami",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/creative_colorful.png",
                Category = "Kreatívny",
                Template = new CertificateTemplateModel
                {
                    Name = "Kreatívna farebná",

                    // Farby
                    TitleColor = "#DC2626", // Červená
                    TextColor = "#1F2937", // Tmavá
                    AccentColor = "#F59E0B", // Oranžová
                    BackgroundColor = "#FFFFFF",

                    // Fonty
                    TitleFontFamily = "Helvetica-Bold",
                    TitleFontSize = 44,
                    HeaderFontFamily = "Helvetica-Bold",
                    HeaderFontSize = 14,
                    TextFontFamily = "Helvetica",
                    TextFontSize = 12,

                    // Okraje
                    MarginTop = 60,
                    MarginRight = 50,
                    MarginBottom = 60,
                    MarginLeft = 50,

                    // Titulok
                    TitleAlignment = "CENTER",
                    CertificateTitle = "CERTIFIKÁT",
                    ShowTitle = true,

                    // Oddeľovacia čiara
                    ShowSeparatorLine = true,
                    SeparatorStyle = "DECORATIVE",

                    // Dekorácie
                    ShowTopDecoration = true,
                    TopDecorationColor = "#DC2626",
                    TopDecorationThickness = 5,
                    ShowBottomDecoration = true,
                    BottomDecorationColor = "#F59E0B",
                    BottomDecorationThickness = 5,

                    // Viditeľnosť polí
                    ShowOrganizer = true,
                    ShowEventTopic = true,
                    ShowEventDate = true,
                    ShowName = true,
                    ShowBirthDate = false,
                    ShowRegistrationNumber = false,
                    ShowNotes = true,

                    // Layout
                    ContentLayout = "VERTICAL",
                    FieldOrder = "Name,EventTopic,EventDate,Organizer,Notes",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 110,
                    LogoHeight = 110,

                    // Border
                    ShowBorder = false
                }
            };
        }

        /// <summary>
        /// Klasická rámovaná šablóna - tradičný vzhľad
        /// </summary>
        public static TemplatePreset GetClassicBorderedPreset()
        {
            return new TemplatePreset
            {
                Name = "Klasická rámovaná",
                Description = "Tradičný certifikát s elegantným rámom",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/classic_bordered.png",
                Category = "Klasický",
                Template = new CertificateTemplateModel
                {
                    Name = "Klasická rámovaná",

                    // Farby
                    TitleColor = "#0F172A", // Tmavá
                    TextColor = "#334155", // Sivá
                    AccentColor = "#0F172A", // Tmavá
                    BackgroundColor = "#FFFFFF",

                    // Fonty
                    TitleFontFamily = "Times-Bold",
                    TitleFontSize = 40,
                    HeaderFontFamily = "Times-Bold",
                    HeaderFontSize = 13,
                    TextFontFamily = "Times-Roman",
                    TextFontSize = 12,

                    // Okraje
                    MarginTop = 100,
                    MarginRight = 90,
                    MarginBottom = 100,
                    MarginLeft = 90,

                    // Titulok
                    TitleAlignment = "CENTER",
                    CertificateTitle = "CERTIFIKÁT O ABSOLVOVANÍ",
                    ShowTitle = true,

                    // Oddeľovacia čiara
                    ShowSeparatorLine = true,
                    SeparatorStyle = "DOUBLE_LINE",

                    // Dekorácie
                    ShowTopDecoration = false,
                    ShowBottomDecoration = false,

                    // Border
                    ShowBorder = true,
                    BorderColor = "#0F172A",
                    BorderWidth = 5,

                    // Viditeľnosť polí
                    ShowOrganizer = true,
                    ShowEventTopic = true,
                    ShowEventDate = true,
                    ShowName = true,
                    ShowBirthDate = true,
                    ShowRegistrationNumber = true,
                    ShowNotes = false,

                    // Layout
                    ContentLayout = "VERTICAL",
                    FieldOrder = "Name,BirthDate,EventTopic,EventDate,RegistrationNumber,Organizer",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 100,
                    LogoHeight = 100
                }
            };
        }
    }

    /// <summary>
    /// Reprezentuje preddefinovanú šablónu s metadátami
    /// </summary>
    public class TemplatePreset
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string PreviewImagePath { get; set; }
        public string Category { get; set; }
        public CertificateTemplateModel Template { get; set; }
    }
}

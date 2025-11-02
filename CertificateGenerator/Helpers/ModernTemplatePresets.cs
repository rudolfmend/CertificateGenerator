using System.Collections.Generic;
using CertificateGenerator.Data;
using System.Diagnostics;

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
                GetCaduceusBluePreset(),
                GetMinimalistBluePreset(),
                GetElegantGoldPreset(),
                GetModernGradientPreset(),
                GetProfessionalDarkPreset(),
                GetCreativeColorfulPreset(),
                GetClassicBorderedPreset(),
                GetOrnamentalLuxuryPreset()
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
                    ShowTopDecoration = false,
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
                    ShowRegistrationNumber = true,
                    ShowNotes = false,

                    // Layout
                    ContentLayout = "MODERN",
                    FieldOrder = "EventTopic,Name,BirthDate,RegistrationNumber,EventDate,Organizer",

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
                    TitleColor = "#92400E",  // Tmavé zlato/hnedá
                    TextColor = "#1C1917",   // Takmer čierna
                    AccentColor = "#D97706", // Zlatá
                    BackgroundColor = "#FFFBEB", // Krém

                    // Fonty
                    TitleFontFamily = "Times-Bold",
                    TitleFontSize = 13,
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
                    TitleUnderlined = true,

                    // Oddeľovacia čiara
                    ShowSeparatorLine = false,
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
                    ContentLayout = "MODERN",
                    FieldOrder = "Name,EventTopic,EventDate,RegistrationNumber,Organizer",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 100,
                    LogoHeight = 100
                }
            };
        }

        /// <summary>
        /// Moderná gradient šablóna - súčasný trend
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
                    TitleColor = "#7C3AED", // Fial
                    TextColor = "#374151", // Sivá
                    AccentColor = "#8B5CF6", // Svetlá fial
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
                    ShowBirthDate = true,
                    ShowRegistrationNumber = true,
                    ShowNotes = false,

                    // Layout
                    ContentLayout = "MODERN",
                    FieldOrder = "EventTopic,Name,BirthDate,RegistrationNumber,EventDate,Organizer",

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
                    TitleUnderlined = true,

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
                    AccentColor = "#F59E0B", // Oranž
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
                    ShowRegistrationNumber = true,
                    ShowNotes = true,

                    // Layout
                    ContentLayout = "MODERN",
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
            //Debug.WriteLine("⚠️ Generating Classic Bordered Certificate Preset");
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
                    CertificateTitle = "CERTIFIKÁT O ABSOLVOVANÍ public static TemplatePreset GetClassicBorderedPreset()",
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
                    ContentLayout = "MODERN",
                    FieldOrder = "Name,BirthDate,EventTopic,EventDate,RegistrationNumber,Organizer",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 100,
                    LogoHeight = 100
                }
            };

            //System.Diagnostics.Process.Start("cmd.exe", "/C echo Generating Caduceus Certificate: " + filePath);
            //System.Diagnostics.Debug.WriteLine($"⚠️ Chyba pri načítaní PNG loga: {ex.Message}");
            //System.Diagnostics.Process.Start("cmd.exe", "/C echo Adding Caduceus Logo PNG: " + logoPath);
        }

        /// <summary>
        /// Ozdobná luxusná šablóna - bohatý dizajn s viacerými ozdobnými prvkami
        /// </summary>
        public static TemplatePreset GetOrnamentalLuxuryPreset()
        {
            //Debug.WriteLine("⚠️ Generating Ornamental Luxury Certificate Preset");
            return new TemplatePreset
            {
                Name = "Ozdobná luxusná",
                Description = "Bohatý dizajn s viacerými čiarami, ozdobnými prvkami a detailnými textovými poliami",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/ornamental_luxury.png",
                Category = "Luxusný",
                Template = new CertificateTemplateModel
                {
                    Name = "Ozdobná luxusná",

                    // Farby - elegantná kombinécia bordovej a zlata
                    TitleColor = "#7C2D12",  // Tmavo bord
                    TextColor = "#1C1917",   // Takmer čierna
                    AccentColor = "#B45309", // Bronz/zlatá
                    BackgroundColor = "#FFF9F0", // Jemná krém

                    // Fonty 
                    TitleFontFamily = "Times-Bold",
                    TitleFontSize = 14,
                    HeaderFontFamily = "Times-Bold",
                    HeaderFontSize = 9,
                    TextFontFamily = "Times-Roman",
                    TextFontSize = 12,

                    // Okraje 
                    MarginTop = 10,
                    MarginRight = 75,
                    MarginBottom = 20,
                    MarginLeft = 75,

                    // Titulok
                    TitleAlignment = "CENTER",
                    CertificateTitle = "POTVRDENIE O ÚČASTI NA ODBORNOM SEMINÁRI",
                    ShowTitle = true,
                    TitleUnderlined = false,

                    // Oddeľovacia čiara pod titulkom
                    ShowSeparatorLine = true,
                    SeparatorStyle = "DOUBLE_LINE",

                    // Horná dekorácia 
                    ShowTopDecoration = false,
                    TopDecorationType = "WAVY_LINE",
                    TopDecorationColor = "#B45309",
                    TopDecorationThickness = 1,

                    // Dolná dekorácia - zodpovedá hornej
                    ShowBottomDecoration = false,
                    BottomDecorationType = "WAVY_LINE",
                    BottomDecorationColor = "#B45309",
                    BottomDecorationThickness = 1,

                    // Bočné dekorácie - ornamenty
                    ShowSideDecorations = true,
                    SideDecorationType = "ORNAMENTAL_BORDER",
                    SideDecorationColor = "#B45309",

                    // Ozdobný rámček
                    ShowBorder = true,
                    BorderColor = "#7C2D12",
                    BorderWidth = 1,

                    // Viditeľnosť všetkých polí - maximálne informácie
                    ShowOrganizer = true,
                    ShowEventTopic = true,
                    ShowEventDate = true,
                    ShowName = true,
                    ShowBirthDate = true,
                    ShowRegistrationNumber = true,
                    ShowNotes = true,

                    // Layout - vertikálny 
                    ContentLayout = "TWO_COLUMN",
                    FieldOrder = "Name,BirthDate,RegistrationNumber,EventTopic,EventDate,Organizer,Notes",

                    // Logo
                    LogoPosition = "TOP",
                    LogoWidth = 110,
                    LogoHeight = 110,

                    // Vlastné texty - pridané špeciálne hlavičky a pätičky
                    CustomHeaderText = "",
                    CustomHeaderAlignment = "CENTER",
                    CustomHeaderBold = false,

                    CustomFooterText = "Tento certifikát potvrdzuje úspešné absolvovanie vzdelávacieho podujatia\n" +
                                        "a udeľuje sa zaň jeden kredit.\n" +
                                        "Cumulus, s.r.o. Rastislavova 45, Košice 040 01",
                    CustomFooterAlignment = "CENTER",
                    CustomFooterBold = false,
                    CustomFooterItalic = true,

                    // Predvolené popisky polí - detailnejšie
                    LabelOrganizer = "Cumulus, s.r.o. Rastislavova 45, Košice 040 01",
                    LabelEventTopic = "Téma a obsah seminára\n",
                    LabelParticipant = "",
                    LabelEventDate = "Dátum konania podujatia\n",
                    LabelBirthDate = "Dátum narodenia\n",
                    LabelRegistrationNumber = "Registračné číslo\n",
                    LabelNotes = "Ďalšie poznámky a údaje\n"

                    //LabelParticipant = "Meno a priezvisko účastníka\n",
                }
            };
        }

        public static TemplatePreset GetCaduceusBluePreset()
        {
            //Debug.WriteLine("⚠️ Generating Cumulus - template with logo Caduceus, color blue, Certificate Preset");
            return new TemplatePreset
            {
                Name = "Cumulus",
                Description = "Modrá medicínska šablóna s Caduceus logom",
                PreviewImagePath = "pack://application:,,,/Resources/Templates/caduceus_green.png",
                Category = "Profesionálny",
                Template = new CertificateTemplateModel
                {
                    Name = "Cumulus",
                    UseCaduceusStyle = true, // ← MUSÍ BYŤ TRUE

                    // Zelené farby
                    TitleColor = "#0d66ff", // svetlo modrá
                    TextColor = "#062e73",
                    AccentColor = "#0a37bf",
                    BorderColor = "#0a37bf",
                    BackgroundColor = "#FFFFFF",

                    // Fonty
                    TitleFontFamily = "Arial-Bold",
                    TitleFontSize = 28,
                    HeaderFontFamily = "Arial-Bold",
                    HeaderFontSize = 22,
                    TextFontFamily = "Arial",
                    TextFontSize = 14,

                    // Okraje
                    MarginTop = 40,
                    MarginRight = 40,
                    MarginBottom = 40,
                    MarginLeft = 40,

                    // Border
                    ShowBorder = true,
                    BorderWidth = 2,

                    // Logo
                    ShowLogo = true,
                    LogoPosition = "TOP",

                    // Miesto konania
                    EventLocation = "Košice" // alebo iné mesto
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

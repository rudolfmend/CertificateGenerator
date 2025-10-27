using CertificateGenerator.Data;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf.Colorspace;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.IO;
using System.Diagnostics;

namespace CertificateGenerator.Helpers
{
    /// <summary>
    /// Helper pre generovanie PDF certifikátov podľa šablón
    /// </summary>
    public class CertificatePdfGenerator
    {
        public static void GeneratePdf(
            string filePath,
            CertificateTemplateModel template,
            string organizerName,
            string eventTopic,
            DateTime? eventDate,
            string participantName,
            DateTime? birthDate,
            string registrationNumber,
            string notes,
            PageSize pageSize)
        {
            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdfDocument = new PdfDocument(writer))
            using (Document document = new Document(pdfDocument, pageSize))
            {
                // Explicitné vytvorenie stránky pred akýmkoľvek prístupom
                pdfDocument.AddNewPage(pageSize);

                // Nastavenie okrajov
                document.SetMargins(
                    template.MarginTop,
                    template.MarginRight,
                    template.MarginBottom,
                    template.MarginLeft
                );

                // Načítanie fontov
                PdfFont titleFont = GetFont(template.TitleFontFamily);
                PdfFont headerFont = GetFont(template.HeaderFontFamily);
                PdfFont textFont = GetFont(template.TextFontFamily);

                // Farby
                Color titleColor = ParseColor(template.TitleColor);
                Color textColor = ParseColor(template.TextColor);
                Color accentColor = ParseColor(template.AccentColor);

                // Rámček okolo celého dokumentu
                if (template.ShowBorder)
                {
                    AddPageBorder(pdfDocument, pageSize, template);
                }

                // Horná dekorácia(vlnovka)
                if (template.ShowTopDecoration)
                {
                    PdfCanvas canvas = new PdfCanvas(pdfDocument.GetPage(1));

                    // Pozícia: tesne pod horným okrajom
                    float decorY = pageSize.GetHeight() - template.MarginTop + 15;
                    float decorStartX = template.MarginLeft;
                    float decorEndX = pageSize.GetWidth() - template.MarginRight;

                    DrawWavyLine(
                        canvas,
                        decorStartX,        // Začiatok X
                        decorY,             // Y pozícia
                        decorEndX,          // Koniec X
                        5,                  // Amplitúda vlny (výška)
                        20,                 // Vlnová dĺžka
                        ParseColor(template.TopDecorationColor),
                        template.TopDecorationThickness
                    );
                }

                // Logo na začiatku
                if (!string.IsNullOrWhiteSpace(template.LogoPath) &&
                    template.LogoPosition == "TOP" &&
                    File.Exists(template.LogoPath))
                {
                    AddLogo(document, template, HorizontalAlignment.CENTER);
                }

                // Vlastný text v záhlaví
                if (!string.IsNullOrWhiteSpace(template.CustomHeaderText))
                {
                    document.Add(new Paragraph(template.CustomHeaderText)
                        .SetFont(textFont)
                        .SetFontSize(template.TextFontSize)
                        .SetFontColor(textColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(15));
                }

                // Titulok certifikátu
                if (template.ShowTitle && !string.IsNullOrWhiteSpace(template.CertificateTitle))
                {
                    TextAlignment alignment = template.TitleAlignment == "LEFT" ? TextAlignment.LEFT :
                                             template.TitleAlignment == "RIGHT" ? TextAlignment.RIGHT :
                                             TextAlignment.CENTER;

                    Paragraph title = new Paragraph(template.CertificateTitle)
                        .SetFont(titleFont)
                        .SetFontSize(template.TitleFontSize)
                        .SetFontColor(titleColor)
                        .SetTextAlignment(alignment)
                        .SetMarginBottom(20);

                    document.Add(title);
                }

                // Oddeľovacia čiara
                if (template.ShowSeparatorLine)
                {
                    AddSeparator(document, template, accentColor);
                }


                // === POLIA PODĽA PORADIA A LAYOUTU ===
                Debug.WriteLine($"[PDF Generator] ContentLayout={template.ContentLayout}, FieldOrder={template.FieldOrder}");

                var fieldOrder = template.GetFieldOrderList();
                var fieldData = new System.Collections.Generic.List<FieldData>();

                // Zozbieraj všetky viditeľné polia podľa poradia
                foreach (var fieldId in fieldOrder)
                {
                    string label = "";
                    string value = "";
                    bool isVisible = false;

                    switch (fieldId)
                    {
                        case "Organizer":
                            if (template.ShowOrganizer && !string.IsNullOrWhiteSpace(organizerName))
                            {
                                label = template.LabelOrganizer;
                                value = organizerName;
                                isVisible = true;
                            }
                            break;

                        case "EventTopic":
                            if (template.ShowEventTopic && !string.IsNullOrWhiteSpace(eventTopic))
                            {
                                label = template.LabelEventTopic;
                                value = eventTopic;
                                isVisible = true;
                            }
                            break;

                        case "EventDate":
                            if (template.ShowEventDate && eventDate.HasValue)
                            {
                                label = template.LabelEventDate;
                                value = eventDate.Value.ToString("dd.MM.yyyy");
                                isVisible = true;
                            }
                            break;

                        case "Name":
                            if (template.ShowName && !string.IsNullOrWhiteSpace(participantName))
                            {
                                label = template.LabelParticipant;
                                value = participantName;
                                isVisible = true;
                            }
                            break;

                        case "BirthDate":
                            if (template.ShowBirthDate && birthDate.HasValue)
                            {
                                label = template.LabelBirthDate;
                                value = birthDate.Value.ToString("dd.MM.yyyy");
                                isVisible = true;
                            }
                            break;

                        case "RegistrationNumber":
                            if (template.ShowRegistrationNumber && !string.IsNullOrWhiteSpace(registrationNumber))
                            {
                                label = template.LabelRegistrationNumber;
                                value = registrationNumber;
                                isVisible = true;
                            }
                            break;

                        case "Notes":
                            if (template.ShowNotes && !string.IsNullOrWhiteSpace(notes))
                            {
                                label = template.LabelNotes;
                                value = notes;
                                isVisible = true;
                            }
                            break;
                    }

                    if (isVisible)
                    {
                        fieldData.Add(new FieldData { Id = fieldId, Label = label, Value = value });
                    }
                }

                Debug.WriteLine($"[PDF Generator] Viditeľných polí: {fieldData.Count}");

                // Generuj polia podľa zvoleného layoutu
                if (template.ContentLayout == "TWO_COLUMN" || template.ContentLayout == "THREE_COLUMN")
                {
                    // VIACSTĹPCOVÝ LAYOUT
                    int columnCount = template.ContentLayout == "TWO_COLUMN" ? 2 : 3;
                    Debug.WriteLine($"[PDF Generator] Generujem {columnCount}-stĺpcový layout");

                    // Vytvor iText Table
                    Table fieldsTable = new Table(columnCount);
                    fieldsTable.SetWidth(UnitValue.CreatePercentValue(100));
                    fieldsTable.SetMarginBottom(20);

                    // Rozdeľ polia do stĺpcov
                    int fieldsPerColumn = (int)Math.Ceiling((double)fieldData.Count / columnCount);

                    for (int col = 0; col < columnCount; col++)
                    {
                        // Vytvor stĺpec ako vnorený StackPanel (Div)
                        Div columnDiv = new Div();

                        int startIdx = col * fieldsPerColumn;
                        int endIdx = Math.Min(startIdx + fieldsPerColumn, fieldData.Count);

                        Debug.WriteLine($"[PDF Generator] Stĺpec {col}: polia {startIdx}-{endIdx}");

                        for (int i = startIdx; i < endIdx; i++)
                        {
                            var field = fieldData[i];

                            // Špeciálne spracovanie pre Notes (dlhší text)
                            if (field.Id == "Notes")
                            {
                                columnDiv.Add(new Paragraph(field.Label)
                                    .SetFont(headerFont)
                                    .SetFontSize(template.TextFontSize + 1)
                                    .SetFontColor(textColor)
                                    .SetMarginTop(15)
                                    .SetMarginBottom(5));

                                columnDiv.Add(new Paragraph(field.Value)
                                    .SetFont(textFont)
                                    .SetFontSize(template.TextFontSize)
                                    .SetFontColor(textColor)
                                    .SetMarginBottom(20));
                            }
                            else
                            {
                                // Štandardné pole
                                columnDiv.Add(new Paragraph($"{field.Label} {field.Value}")
                                    .SetFont(GetFieldFont(field.Id, textFont, headerFont))
                                    .SetFontSize(GetFieldFontSize(field.Id, template))
                                    .SetFontColor(GetFieldColor(field.Id, textColor, titleColor, accentColor))
                                    .SetMarginBottom(GetFieldMarginBottom(field.Id)));
                            }
                        }

                        // Pridaj stĺpec do tabuľky
                        Cell cell = new Cell().Add(columnDiv);
                        cell.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                        cell.SetPaddingRight(template.ColumnSpacing / 2);
                        cell.SetPaddingLeft(template.ColumnSpacing / 2);
                        fieldsTable.AddCell(cell);
                    }

                    document.Add(fieldsTable);
                }
                else
                {
                    // VERTIKÁLNY LAYOUT (klasický)
                    Debug.WriteLine($"[PDF Generator] Generujem vertikálny layout");

                    foreach (var field in fieldData)
                    {
                        if (field.Id == "Notes")
                        {
                            // Notes má špeciálne formátovanie
                            document.Add(new Paragraph(field.Label)
                                .SetFont(headerFont)
                                .SetFontSize(template.TextFontSize + 1)
                                .SetFontColor(textColor)
                                .SetMarginTop(15)
                                .SetMarginBottom(5));

                            document.Add(new Paragraph(field.Value)
                                .SetFont(textFont)
                                .SetFontSize(template.TextFontSize)
                                .SetFontColor(textColor)
                                .SetMarginBottom(20));
                        }
                        else
                        {
                            document.Add(new Paragraph($"{field.Label} {field.Value}")
                                .SetFont(GetFieldFont(field.Id, textFont, headerFont))
                                .SetFontSize(GetFieldFontSize(field.Id, template))
                                .SetFontColor(GetFieldColor(field.Id, textColor, titleColor, accentColor))
                                .SetMarginBottom(GetFieldMarginBottom(field.Id)));
                        }
                    }
                }

                // Logo na konci
                if (!string.IsNullOrWhiteSpace(template.LogoPath) &&
                    template.LogoPosition == "BOTTOM" &&
                    File.Exists(template.LogoPath))
                {
                    AddLogo(document, template, HorizontalAlignment.CENTER);
                }

                Paragraph footer = new Paragraph($"Tento certifikát slúži ako potvrdenie o udelení jedného kreditu  za účasť na seminári na oddelení FBLR Rastislavova 45, Košice.")
                    .SetFont(textFont)
                    .SetFontSize(template.TextFontSize - 1)
                    .SetFontColor(ParseColor("#888888"))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(30);

                document.Add(footer);

                // Dolná dekorácia - na konci dokumentu (pred footer, riadok ~200)
                if (template.ShowBottomDecoration)
                {
                    PdfCanvas canvas = new PdfCanvas(pdfDocument.GetPage(1));

                    float decorY = template.MarginBottom - 15;
                    float decorStartX = template.MarginLeft;
                    float decorEndX = pageSize.GetWidth() - template.MarginRight;

                    DrawWavyLine(
                        canvas,
                        decorStartX,
                        decorY,
                        decorEndX,
                        5,
                        20,
                        ParseColor(template.BottomDecorationColor),
                        template.BottomDecorationThickness
                    );
                }
            }
        }

        private static void AddPageBorder(PdfDocument pdfDocument, PageSize pageSize, CertificateTemplateModel template)
        {
            PdfPage page = pdfDocument.GetPage(1);
            PdfCanvas canvas = new PdfCanvas(page);

            Color borderColor = ParseColor(template.BorderColor);
            canvas.SetStrokeColor(borderColor);
            canvas.SetLineWidth(template.BorderWidth);

            float margin = 15;
            canvas.Rectangle(
                margin,
                margin,
                pageSize.GetWidth() - (2 * margin),
                pageSize.GetHeight() - (2 * margin)
            );
            canvas.Stroke();
        }

        private static void AddLogo(Document document, CertificateTemplateModel template, HorizontalAlignment alignment)
        {
            try
            {
                ImageData imageData = ImageDataFactory.Create(template.LogoPath);
                Image logo = new Image(imageData);

                logo.SetWidth(template.LogoWidth);
                logo.SetHeight(template.LogoHeight);
                logo.SetHorizontalAlignment(alignment);
                logo.SetMarginBottom(15);

                document.Add(logo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri načítaní loga: {ex.Message}");
            }
        }

        private static void AddSeparator(Document document, CertificateTemplateModel template, Color color)
        {
            if (template.SeparatorStyle == "UNDERLINE")
            {
                document.Add(new Paragraph("_____________________________________")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(color)
                    .SetMarginBottom(20));
            }
            else if (template.SeparatorStyle == "LINE")
            {
                LineSeparator separator = new LineSeparator(new SolidLine(1f));
                separator.SetMarginBottom(20);
                document.Add(separator);
            }
        }

        private static PdfFont GetFont(string fontFamily)
        {
            try
            {
                string fontPath = null;

                // === WINDOWS SYSTÉMOVÉ FONTY ===
                // Arial
                if (fontFamily != null && fontFamily.Contains("Arial-BoldItalic"))
                    fontPath = @"C:\Windows\Fonts\arialbi.ttf";
                else if (fontFamily != null && fontFamily.Contains("Arial-Bold"))
                    fontPath = @"C:\Windows\Fonts\arialbd.ttf";
                else if (fontFamily != null && fontFamily.Contains("Arial-Italic"))
                    fontPath = @"C:\Windows\Fonts\ariali.ttf";
                else if (fontFamily != null && fontFamily.Contains("Arial"))
                    fontPath = @"C:\Windows\Fonts\arial.ttf";

                // Calibri
                else if (fontFamily != null && fontFamily.Contains("Calibri-BoldItalic"))
                    fontPath = @"C:\Windows\Fonts\calibriz.ttf";
                else if (fontFamily != null && fontFamily.Contains("Calibri-Bold"))
                    fontPath = @"C:\Windows\Fonts\calibrib.ttf";
                else if (fontFamily != null && fontFamily.Contains("Calibri-Italic"))
                    fontPath = @"C:\Windows\Fonts\calibrii.ttf";
                else if (fontFamily != null && fontFamily.Contains("Calibri"))
                    fontPath = @"C:\Windows\Fonts\calibri.ttf";

                // Segoe UI
                else if (fontFamily != null && fontFamily.Contains("SegoeUI-BoldItalic"))
                    fontPath = @"C:\Windows\Fonts\segoeuiz.ttf";
                else if (fontFamily != null && fontFamily.Contains("SegoeUI-Bold"))
                    fontPath = @"C:\Windows\Fonts\segoeuib.ttf";
                else if (fontFamily != null && fontFamily.Contains("SegoeUI-Italic"))
                    fontPath = @"C:\Windows\Fonts\segoeuii.ttf";
                else if (fontFamily != null && fontFamily.Contains("SegoeUI"))
                    fontPath = @"C:\Windows\Fonts\segoeui.ttf";

                // Verdana
                else if (fontFamily != null && fontFamily.Contains("Verdana-BoldItalic"))
                    fontPath = @"C:\Windows\Fonts\verdanaz.ttf";
                else if (fontFamily != null && fontFamily.Contains("Verdana-Bold"))
                    fontPath = @"C:\Windows\Fonts\verdanab.ttf";
                else if (fontFamily != null && fontFamily.Contains("Verdana-Italic"))
                    fontPath = @"C:\Windows\Fonts\verdanai.ttf";
                else if (fontFamily != null && fontFamily.Contains("Verdana"))
                    fontPath = @"C:\Windows\Fonts\verdana.ttf";

                // Tahoma
                else if (fontFamily != null && fontFamily.Contains("Tahoma-Bold"))
                    fontPath = @"C:\Windows\Fonts\tahomabd.ttf";
                else if (fontFamily != null && fontFamily.Contains("Tahoma"))
                    fontPath = @"C:\Windows\Fonts\tahoma.ttf";

                // Georgia
                else if (fontFamily != null && fontFamily.Contains("Georgia-BoldItalic"))
                    fontPath = @"C:\Windows\Fonts\georgiaz.ttf";
                else if (fontFamily != null && fontFamily.Contains("Georgia-Bold"))
                    fontPath = @"C:\Windows\Fonts\georgiab.ttf";
                else if (fontFamily != null && fontFamily.Contains("Georgia-Italic"))
                    fontPath = @"C:\Windows\Fonts\georgiai.ttf";
                else if (fontFamily != null && fontFamily.Contains("Georgia"))
                    fontPath = @"C:\Windows\Fonts\georgia.ttf";

                // === OPEN-SOURCE FONTY (ak sú nainštalované) ===
                // DejaVu Sans
                else if (fontFamily != null && fontFamily.Contains("DejaVuSans-BoldOblique"))
                    fontPath = @"C:\Windows\Fonts\DejaVuSans-BoldOblique.ttf";
                else if (fontFamily != null && fontFamily.Contains("DejaVuSans-Bold"))
                    fontPath = @"C:\Windows\Fonts\DejaVuSans-Bold.ttf";
                else if (fontFamily != null && fontFamily.Contains("DejaVuSans-Oblique"))
                    fontPath = @"C:\Windows\Fonts\DejaVuSans-Oblique.ttf";
                else if (fontFamily != null && fontFamily.Contains("DejaVuSans"))
                    fontPath = @"C:\Windows\Fonts\DejaVuSans.ttf";

                // Liberation Sans
                else if (fontFamily != null && fontFamily.Contains("LiberationSans-BoldItalic"))
                    fontPath = @"C:\Windows\Fonts\LiberationSans-BoldItalic.ttf";
                else if (fontFamily != null && fontFamily.Contains("LiberationSans-Bold"))
                    fontPath = @"C:\Windows\Fonts\LiberationSans-Bold.ttf";
                else if (fontFamily != null && fontFamily.Contains("LiberationSans-Italic"))
                    fontPath = @"C:\Windows\Fonts\LiberationSans-Italic.ttf";
                else if (fontFamily != null && fontFamily.Contains("LiberationSans"))
                    fontPath = @"C:\Windows\Fonts\LiberationSans-Regular.ttf";

                // Ak máme cestu k systémovému fontu, načítame ho
                if (fontPath != null && File.Exists(fontPath))
                {
                    return PdfFontFactory.CreateFont(fontPath, "Identity-H", PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
                }

                // === FALLBACK NA ŠTANDARDNÉ PDF FONTY ===
                string encoding = "Cp1250";

                if (fontFamily == null || fontFamily.Contains("Helvetica-Bold"))
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD, encoding, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                else if (fontFamily.Contains("Helvetica-Oblique") || fontFamily.Contains("Italic"))
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE, encoding, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                else if (fontFamily.Contains("Times-Bold"))
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD, encoding, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                else if (fontFamily.Contains("Times"))
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN, encoding, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                else if (fontFamily.Contains("Courier"))
                    return PdfFontFactory.CreateFont(StandardFonts.COURIER, encoding, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                else
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA, encoding, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            }
            catch
            {
                try
                {
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA, "Cp1250", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }
                catch
                {
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                }
            }
        }

        private static Color ParseColor(string hexColor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hexColor))
                    return ColorConstants.BLACK;

                hexColor = hexColor.TrimStart('#');

                if (hexColor.Length != 6)
                    return ColorConstants.BLACK;

                int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);

                return new DeviceRgb(r, g, b);
            }
            catch
            {
                return ColorConstants.BLACK;
            }
        }

        private static void DrawWavyLine(PdfCanvas canvas, float startX, float startY, float endX,
    float amplitude, float wavelength, Color color, float thickness)
        {
            canvas.SaveState();
            canvas.SetStrokeColor(color);
            canvas.SetLineWidth(thickness);

            float currentX = startX;
            bool isUp = true;

            while (currentX < endX)
            {
                float nextX = Math.Min(currentX + wavelength, endX);
                float controlY = isUp ? startY + amplitude : startY - amplitude;

                canvas.CurveTo(
                    currentX + wavelength / 3, controlY,
                    currentX + 2 * wavelength / 3, controlY,
                    nextX, startY
                );

                currentX = nextX;
                isUp = !isUp;
            }

            canvas.Stroke();
            canvas.RestoreState();
        }

        private void DrawDiagonalLines(PdfCanvas canvas, float x, float y, float width,
            float height, Color color, float thickness, int lineCount)
        {
            canvas.SaveState();
            canvas.SetStrokeColor(color);
            canvas.SetLineWidth(thickness);

            float spacing = width / lineCount;

            for (int i = 0; i < lineCount; i++)
            {
                float startX = x + i * spacing;
                canvas.MoveTo(startX, y);
                canvas.LineTo(startX + height, y + height);
            }

            canvas.Stroke();
            canvas.RestoreState();
        }

        private void DrawCornerOrnament(PdfCanvas canvas, float x, float y,
            float size, Color color, string corner)
        {
            canvas.SaveState();
            canvas.SetStrokeColor(color);
            canvas.SetLineWidth(2);

            // Kreslí ozdobný rohový prvok
            switch (corner)
            {
                case "TOP_LEFT":
                    // Ornament v ľavom hornom rohu
                    canvas.MoveTo(x, y - size);
                    canvas.LineTo(x, y);
                    canvas.LineTo(x + size, y);

                    // Pridať detaily
                    canvas.MoveTo(x + 5, y - 5);
                    canvas.LineTo(x + size / 2, y - 5);
                    canvas.MoveTo(x + 5, y - 5);
                    canvas.LineTo(x + 5, y - size / 2);
                    break;
                    // Podobne pre ostatné rohy...
            }

            canvas.Stroke();
            canvas.RestoreState();
        }


        /// <summary>
        /// Helper class pre uloženie údajov o poli
        /// </summary>
        private class FieldData
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// Získa font pre pole podľa jeho typu
        /// </summary>
        private static PdfFont GetFieldFont(string fieldId, PdfFont defaultFont, PdfFont headerFont)
        {
            switch (fieldId)
            {
                case "EventTopic":
                case "Name":
                    return headerFont;
                default:
                    return defaultFont;
            }
        }

        /// <summary>
        /// Získa veľkosť fontu pre pole
        /// </summary>
        private static int GetFieldFontSize(string fieldId, CertificateTemplateModel template)
        {
            switch (fieldId)
            {
                case "Organizer":
                    return template.TextFontSize + 1;
                case "EventTopic":
                    return template.HeaderFontSize;
                case "Name":
                    return template.HeaderFontSize + 2;
                default:
                    return template.TextFontSize;
            }
        }

        /// <summary>
        /// Získa farbu pre pole
        /// </summary>
        private static Color GetFieldColor(string fieldId, Color defaultColor, Color titleColor, Color accentColor)
        {
            switch (fieldId)
            {
                case "EventTopic":
                    return accentColor;
                case "Name":
                    return titleColor;
                default:
                    return defaultColor;
            }
        }

        /// <summary>
        /// Získa dolný okraj pre pole
        /// </summary>
        private static float GetFieldMarginBottom(string fieldId)
        {
            switch (fieldId)
            {
                case "Organizer":
                case "Name":
                    return 15;
                case "EventTopic":
                    return 10;
                default:
                    return 8;
            }
        }


        //  zatiaľ vynechať gradient funkciu a sústrediť sa na vlnovky a čiary, ktoré fungujú s PdfCanvas metódami
        //private void DrawGradientBackground(PdfCanvas canvas, PdfDocument pdfDoc,
        //    float x, float y, float width, float height,
        //    Color startColor, Color endColor, string direction)
        //{
        //    // iText gradient implementation
        //    PdfShading.Axial shading;

        //    switch (direction)
        //    {
        //        case "HORIZONTAL":
        //            shading = new PdfShading.Axial(
        //                pdfDoc.GetDefaultColorSpace(),
        //                x, y + height / 2,
        //                x + width, y + height / 2,
        //                startColor.GetColorValue(),
        //                endColor.GetColorValue()
        //            );
        //            break;
        //        case "DIAGONAL":
        //            shading = new PdfShading.Axial(
        //                pdfDoc.GetDefaultColorSpace(),
        //                x, y,
        //                x + width, y + height,
        //                startColor.GetColorValue(),
        //                endColor.GetColorValue()
        //            );
        //            break;
        //        default: // VERTICAL
        //            shading = new PdfShading.Axial(
        //                pdfDoc.GetDefaultColorSpace(),
        //                x + width / 2, y,
        //                x + width / 2, y + height,
        //                startColor.GetColorValue(),
        //                endColor.GetColorValue()
        //            );
        //            break;
        //    }

        //    PdfPattern.Shading pattern = new PdfPattern.Shading(shading);
        //    PdfPatternColor patternColor = new PdfPatternColor(pattern);

        //    canvas.SaveState();
        //    canvas.SetFillColor(patternColor);
        //    canvas.Rectangle(x, y, width, height);
        //    canvas.Fill();
        //    canvas.RestoreState();
        //}
    }
}

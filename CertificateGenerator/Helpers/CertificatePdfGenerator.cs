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

                // Organizátor
                if (template.ShowOrganizer && !string.IsNullOrWhiteSpace(organizerName))
                {
                    document.Add(new Paragraph($"{template.LabelOrganizer} {organizerName}")
                        .SetFont(textFont)
                        .SetFontSize(template.TextFontSize + 1)
                        .SetFontColor(textColor)
                        .SetMarginBottom(15));
                }

                // Téma podujatia
                if (template.ShowEventTopic && !string.IsNullOrWhiteSpace(eventTopic))
                {
                    document.Add(new Paragraph($"{template.LabelEventTopic} {eventTopic}")
                        .SetFont(headerFont)
                        .SetFontSize(template.HeaderFontSize)
                        .SetFontColor(accentColor)
                        .SetMarginBottom(10));
                }

                // Meno účastníka
                if (template.ShowName && !string.IsNullOrWhiteSpace(participantName))
                {
                    document.Add(new Paragraph($"{template.LabelParticipant} {participantName}")
                        .SetFont(headerFont)
                        .SetFontSize(template.HeaderFontSize + 2)
                        .SetFontColor(titleColor)
                        .SetMarginBottom(15));
                }

                // Dátum podujatia
                if (template.ShowEventDate && eventDate.HasValue)
                {
                    document.Add(new Paragraph($"{template.LabelEventDate} {eventDate.Value:dd.MM.yyyy}")
                        .SetFont(textFont)
                        .SetFontSize(template.TextFontSize)
                        .SetFontColor(textColor)
                        .SetMarginBottom(8));
                }

                // Dátum narodenia
                if (template.ShowBirthDate && birthDate.HasValue)
                {
                    document.Add(new Paragraph($"{template.LabelBirthDate} {birthDate.Value:dd.MM.yyyy}")
                        .SetFont(textFont)
                        .SetFontSize(template.TextFontSize)
                        .SetFontColor(textColor)
                        .SetMarginBottom(8));
                }

                // Registračné číslo
                if (template.ShowRegistrationNumber && !string.IsNullOrWhiteSpace(registrationNumber))
                {
                    document.Add(new Paragraph($"{template.LabelRegistrationNumber} {registrationNumber}")
                        .SetFont(textFont)
                        .SetFontSize(template.TextFontSize)
                        .SetFontColor(textColor)
                        .SetMarginBottom(8));
                }

                // Poznámky
                if (template.ShowNotes && !string.IsNullOrWhiteSpace(notes))
                {
                    document.Add(new Paragraph(template.LabelNotes)
                        .SetFont(headerFont)
                        .SetFontSize(template.TextFontSize + 1)
                        .SetFontColor(textColor)
                        .SetMarginTop(15)
                        .SetMarginBottom(5));

                    document.Add(new Paragraph(notes)
                        .SetFont(textFont)
                        .SetFontSize(template.TextFontSize)
                        .SetFontColor(textColor)
                        .SetMarginBottom(20));
                }

                // Vlastný text v pätičke
                if (!string.IsNullOrWhiteSpace(template.CustomFooterText))
                {
                    document.Add(new Paragraph(template.CustomFooterText)
                        .SetFont(textFont)
                        .SetFontSize(template.TextFontSize - 1)
                        .SetFontColor(textColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginTop(20));
                }

                // Logo na konci
                if (!string.IsNullOrWhiteSpace(template.LogoPath) &&
                    template.LogoPosition == "BOTTOM" &&
                    File.Exists(template.LogoPath))
                {
                    AddLogo(document, template, HorizontalAlignment.CENTER);
                }

                // Časová pečiatka
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
                // Použitie fontov s Unicode (CP1250) podporou pre slovenčinu
                string encoding = "Cp1250"; // Central European encoding

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
                // Fallback: ak zlyha Cp1250, skús bez encodingu
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

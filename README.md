# CertificateGenerator

Application for generating PDF certificates of completion for professional seminars and training courses.

## Features

- **Quick Creation** - Individual certificates with intuitive form
- **Bulk Generation** - Create certificates for groups of participants
- **CSV Import** - Load participant lists
- **Data Management** - Database of participants, organizers, and event topics
- **Template Editor** - Customize appearance, colors, and layout
- **History** - Track generated certificates
- **Format Support** - A3, A4, A5

## Technologies

- **Framework**: .NET Framework 4.6.2
- **UI**: WPF (Windows Presentation Foundation)
- **PDF Generator**: iText7 9.3.0
- **Database**: SQLite
- **Language**: C# with Slovak diacritics support

## System Requirements

- Windows 10/11
- .NET Framework 4.6.2 or higher
- Min. 4 GB RAM
- 100 MB free disk space

## Installation

1. Install .NET Framework 4.6.2 or higher
2. Download latest version from releases
3. Extract archive
4. Run `CertificateGenerator.exe`

## Usage

### Quick Creation
1. Fill in event and participant details
2. Select paper format
3. Click "Create PDF"

### Bulk Generation
1. Add participants (manually or CSV import)
2. Enter event topics
3. Assign dates
4. Select folder and generate

### CSV Import Format
```csv
Name;Birth Date;Reg. Number;Notes
John Smith;01.01.1990;12345;Note
```

## Project Structure

```
CertificateGenerator/
├── Data/              # Database models and repositories
├── Helpers/           # PDF generator
├── Images/            # Graphics
├── Properties/        # Assembly info
├── MainWindow.xaml    # Main window
├── *.xaml             # Additional windows and dialogs
└── ModernStyles.xaml  # UI styles
```

## Database

SQLite database is automatically created at:
```
C:\Users\[User Name]\Documents\CertificateGenerator\Databases\CertificateGenerator_{8-char-GUID}_{timestamp}.db
Name format: CertificateGenerator_{8-char-GUID}_{timestamp}.db
Automatic detection and merging
Welcome message on first launch
```

Contains tables:
- `Participants` - Participants
- `Organizers` - Organizers
- `EventTopics` - Event topics
- `Certificates` - Certificate history
- `CertificateTemplates` - Templates

## Privacy

The application:
- Does not collect personal data
- Does not send data to external servers
- Stores everything locally
- Fully GDPR compliant

## License

Copyright © 2025 Rudolf Mendzezof. All rights reserved.

## Contact

**Email**: rudolf.mend@gmail.com  
**Subject**: CertificateGenerator - [Your question]

## Version

**Current version**: 1.0  
**Release date**: January 2025
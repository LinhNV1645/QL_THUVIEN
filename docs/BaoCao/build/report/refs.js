const { Paragraph, AlignmentType, HeadingLevel } = require('docx');
const { runs, CM } = require('./lib');

const ACCESS = 'truy cập ngày 01/10/2026';
const REFS = [
  `Microsoft, *ASP.NET Core 8 MVC Documentation*, https://docs.microsoft.com/aspnet/core, ${ACCESS}.`,
  `Microsoft, *Entity Framework Core 8 Documentation*, https://docs.microsoft.com/ef/core, ${ACCESS}.`,
  'Robert C. Martin, *Clean Architecture*.',
  `*MailKit Documentation*, https://github.com/jstedfast/MailKit, ${ACCESS}.`,
  `*QRCoder Documentation*, https://github.com/codebude/QRCoder, ${ACCESS}.`,
  `*ClosedXML Documentation*, https://github.com/ClosedXML/ClosedXML, ${ACCESS}.`,
  `*Hangfire Documentation*, https://docs.hangfire.io, ${ACCESS}.`,
  `*BCrypt.Net-Next*, https://github.com/BcryptNet/bcrypt.net, ${ACCESS}.`,
  `*Bootstrap 5 Documentation*, https://getbootstrap.com/docs/5.0, ${ACCESS}.`,
  `Microsoft, *SQL Server 2019: Hardware and software requirements*, https://learn.microsoft.com/en-us/sql/sql-server/install/hardware-and-software-requirements-for-installing-sql-server-2019, ${ACCESS}.`,
  `Microsoft, *Visual Studio 2022 System Requirements*, https://learn.microsoft.com/en-us/visualstudio/releases/2022/system-requirements, ${ACCESS}.`,
  `Microsoft, *.NET 8 – Supported OS versions*, https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md, ${ACCESS}.`,
];

module.exports = () => [
  new Paragraph({ text: 'TÀI LIỆU THAM KHẢO', heading: HeadingLevel.HEADING_1 }),
  ...REFS.map((t, i) => new Paragraph({
    alignment: AlignmentType.LEFT,
    indent: { left: CM, hanging: CM },
    children: runs(`[${i + 1}]\t${t}`),
    tabStops: [{ type: 'left', position: CM }],
  })),
];

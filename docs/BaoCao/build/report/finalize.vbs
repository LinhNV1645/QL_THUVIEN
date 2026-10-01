' Mở file .docx bằng Word, cập nhật mục lục / danh mục hình / trường SEQ, lưu lại
' và in thông tin kiểm tra: số trang, số hình, ảnh vượt lề.
' Cách dùng: cscript //nologo finalize.vbs "<đường dẫn docx>" [pdf]
Option Explicit
Dim path, wantPdf, word, doc, i, toc, shp, maxW, overflow, pages, fld
path = WScript.Arguments(0)
wantPdf = (WScript.Arguments.Count > 1)

Set word = CreateObject("Word.Application")
word.Visible = False
word.DisplayAlerts = 0
Set doc = word.Documents.Open(path, False, False)
If doc.ReadOnly Then
  WScript.Echo "ERROR: file is locked (read-only)"
  doc.Close False
  word.Quit
  WScript.Quit 1
End If

doc.Fields.Update
For i = 1 To doc.TablesOfContents.Count
  doc.TablesOfContents(i).Update
Next
For i = 1 To doc.TablesOfFigures.Count
  doc.TablesOfFigures(i).Update
Next
doc.Repaginate
For i = 1 To doc.TablesOfContents.Count
  doc.TablesOfContents(i).UpdatePageNumbers
Next
For i = 1 To doc.TablesOfFigures.Count
  doc.TablesOfFigures(i).UpdatePageNumbers
Next

' Vùng chữ: 21cm - 3cm - 2cm = 16cm = 453.5pt
maxW = 0 : overflow = 0
For Each shp In doc.InlineShapes
  If shp.Width > maxW Then maxW = shp.Width
  If shp.Width > 454 Then overflow = overflow + 1
Next

pages = doc.ComputeStatistics(2)
WScript.Echo "pages=" & pages
WScript.Echo "inlineShapes=" & doc.InlineShapes.Count & " maxWidthPt=" & Round(maxW, 1) & " overflow=" & overflow
WScript.Echo "tocs=" & doc.TablesOfContents.Count & " tofs=" & doc.TablesOfFigures.Count
For i = 1 To doc.TablesOfContents.Count
  WScript.Echo "toc" & i & " paragraphs=" & doc.TablesOfContents(i).Range.Paragraphs.Count
Next
For i = 1 To doc.TablesOfFigures.Count
  WScript.Echo "tof" & i & " paragraphs=" & doc.TablesOfFigures(i).Range.Paragraphs.Count
Next

doc.Save
If wantPdf Then
  doc.ExportAsFixedFormat Replace(path, ".docx", ".pdf"), 17
  WScript.Echo "pdf exported"
End If
doc.Close False
word.Quit

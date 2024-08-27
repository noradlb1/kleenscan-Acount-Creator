Imports System.Net.Http
Imports HtmlAgilityPack
Imports System.Text.RegularExpressions
Imports System.Diagnostics

Public Class Form2
    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' الرابط المباشر للصفحة
        'رابط الصفحة عند وصول الايميل وسيتم استخراج رابط التفعيل منه
        Dim url As String = TextBox1.Text ' استبدل بـ URL الفعلي

        Try
            ' استخدام HttpClient لتحميل المحتوى
            Using client As New HttpClient()
                Dim htmlContent As String = Await client.GetStringAsync(url)

                ' التعبير المنتظم للبحث عن الروابط التي تبدأ بـ https://kleenscan.com/verify وتنتهي بـ @mailcatch.com
                Dim pattern As String = "https://kleenscan\.com/verify/[^\s@]+@mailcatch\.com"
                Dim regex As New Regex(pattern, RegexOptions.IgnoreCase)

                ' البحث عن جميع الروابط في النص
                Dim matches As MatchCollection = regex.Matches(htmlContent)

                ' استخدام HashSet لتخزين الروابط الفريدة
                Dim uniqueLinks As New HashSet(Of String)()
                For Each match As Match In matches
                    uniqueLinks.Add(match.Value)
                Next

                ' التحقق مما إذا تم العثور على روابط
                If uniqueLinks.Count > 0 Then
                    ' فتح أول رابط من الروابط الفريدة
                    Dim firstLink As String = uniqueLinks.First()
                    Process.Start(New ProcessStartInfo With {
                        .FileName = firstLink,
                        .UseShellExecute = True
                    })
                Else
                    ' إذا لم يتم العثور على روابط
                    MessageBox.Show("لم يتم العثور على روابط.")
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("حدث خطأ: " & ex.Message)
        End Try
    End Sub
End Class

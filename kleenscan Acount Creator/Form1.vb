Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome
Imports OpenQA.Selenium.Support.UI
Imports System.Text
Imports System.Net.Http
Imports System.Text.RegularExpressions
Imports System.Linq

Public Class Form1
    Private driver As IWebDriver

    Private Async Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        Dim numberOfAccounts As Integer = CInt(NumericUpDown1.Value) ' الحصول على عدد الحسابات من NumericUpDown

        For i As Integer = 1 To numberOfAccounts
            ' إعداد المتصفح
            driver = New ChromeDriver()

            ' توليد اسم بريد إلكتروني عشوائي
            Dim randomEmailName As String = GenerateRandomString(8)
            Dim fullEmail As String = randomEmailName & "@mailcatch.com"

            ' الانتقال إلى موقع mailcatch.com
            driver.Navigate().GoToUrl("http://mailcatch.com/en/disposable-email")

            ' إدخال الاسم العشوائي في حقل البريد الإلكتروني
            Dim mailCatchTextBox As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/div[4]/div/center[1]/form/input[1]"))
            mailCatchTextBox.SendKeys(randomEmailName)

            ' النقر على زر إنشاء البريد
            Dim createEmailButton As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/div[4]/div/center[1]/form/input[2]"))
            createEmailButton.Click()

            ' فتح تبويب جديد
            CType(driver, IJavaScriptExecutor).ExecuteScript("window.open();")
            ' التحويل إلى التبويب الجديد
            driver.SwitchTo().Window(driver.WindowHandles(1))

            ' الانتقال إلى صفحة التسجيل في kleenscan.com في التبويب الجديد
            driver.Navigate().GoToUrl("https://kleenscan.com/register")

            ' انتظار تحميل الصفحة
            Dim wait As New WebDriverWait(driver, TimeSpan.FromSeconds(10))
            wait.Until(Function(d) d.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/h3")))

            ' توليد اسم ولقب عشوائيين
            Dim randomFirstName As String = GenerateRandomString(8)
            Dim randomLastName As String = GenerateRandomString(8)

            ' إدخال المعلومات في الحقول في kleenscan.com
            Dim nameField As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[1]/input"))
            nameField.SendKeys(randomFirstName)

            Dim surnameField As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[2]/input"))
            surnameField.SendKeys(randomLastName)

            Dim emailFieldKleenScan As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[3]/input"))
            emailFieldKleenScan.SendKeys(fullEmail)

            Dim passwordField As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[4]/input"))
            passwordField.SendKeys(fullEmail)

            Dim confirmPasswordField As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[5]/input"))
            confirmPasswordField.SendKeys(fullEmail)

            Dim jabberIdField As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[6]/input"))
            jabberIdField.SendKeys(fullEmail)

            ' تحديد التشيك بوكس
            Dim checkBox As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[8]/label/input"))
            If Not checkBox.Selected Then
                checkBox.Click()
            End If

            ' النقر على زر التسجيل
            Dim registerButton As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/section/div/div/div/form/div[9]/input"))
            registerButton.Click()

            ' العودة إلى التبويب الأول (mailcatch.com)
            driver.SwitchTo().Window(driver.WindowHandles(0))

            ' الانتظار لحين العثور على الرسالة في صندوق البريد
            Dim emailFound As Boolean = False
            Dim maxAttempts As Integer = 10 ' عدد المحاولات القصوى
            Dim attempts As Integer = 0

            While Not emailFound AndAlso attempts < maxAttempts
                ' النقر على زر تحديث البريد
                Dim refreshEmailButton As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div/h2/a[2]/img"))
                refreshEmailButton.Click()

                ' الانتظار لبضع ثوانٍ قبل التحقق
                Threading.Thread.Sleep(6000)

                ' محاولة العثور على الرسالة باستخدام FullXPath
                Try
                    Dim OpenMailBox As IWebElement = driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div/table/tbody/tr[2]/td[1]"))
                    OpenMailBox.Click()
                    emailFound = True ' تم العثور على الرسالة
                Catch ex As NoSuchElementException
                    ' لم يتم العثور على الرسالة بعد، سيتم المحاولة مرة أخرى
                    attempts += 1
                End Try
            End While

            If emailFound Then
                ' استخدم HttpClient لتحليل محتوى البريد واستخراج رابط التفعيل
                Dim emailPageUrl As String = driver.Url
                Dim verificationLink As String = Await GetVerificationLinkFromEmailAsync(emailPageUrl)

                If Not String.IsNullOrEmpty(verificationLink) Then
                    Threading.Thread.Sleep(6000)
                    ' فتح رابط التفعيل في نفس المتصفح
                    driver.Navigate().GoToUrl(verificationLink)
                    Threading.Thread.Sleep(6000)
                Else
                    MessageBox.Show("لم يتم العثور على رابط التفعيل.")
                End If

                ' حفظ البريد الإلكتروني المستخدم
                SaveEmail(fullEmail)
            Else
                MessageBox.Show("لم يتم العثور على الرسالة بعد عدة محاولات.")
            End If
            Threading.Thread.Sleep(2000)
            ' إغلاق التبويب الحالي والانتقال إلى التبويب الأول
            driver.Close()
            driver.SwitchTo().Window(driver.WindowHandles(0))

            ' إغلاق المتصفح بعد كل عملية تسجيل
            driver.Quit()

            ' انتظار قبل بدء العملية التالية
            Threading.Thread.Sleep(2000)
        Next
    End Sub

    Private Function GenerateRandomString(length As Integer) As String
        Dim characters As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
        Dim random As New Random()
        Dim result As New StringBuilder(length)
        For i As Integer = 1 To length
            Dim index As Integer = random.Next(0, characters.Length)
            result.Append(characters(index))
        Next
        Return result.ToString()
    End Function

    Private Sub SaveEmail(email As String)
        ' وظيفة لحفظ البريد الإلكتروني في ملف نصي أو قاعدة بيانات
        System.IO.File.AppendAllText("emails.txt", email & Environment.NewLine)
    End Sub

    Private Async Function GetVerificationLinkFromEmailAsync(emailPageUrl As String) As Task(Of String)
        Try
            Using client As New HttpClient()
                Dim htmlContent As String = Await client.GetStringAsync(emailPageUrl)

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
                    ' إرجاع أول رابط من الروابط الفريدة
                    Return uniqueLinks.First()
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("حدث خطأ أثناء محاولة الحصول على رابط التفعيل: " & ex.Message)
        End Try
        Return String.Empty
    End Function

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://t.me/MONSTERMCSY")
    End Sub
End Class

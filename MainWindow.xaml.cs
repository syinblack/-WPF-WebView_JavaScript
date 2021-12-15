using System;
using System.Windows;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace WpfApp1
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //this.Closed += new EventHandler(MainWindow1_Closing);
            //_WebBrowser.Navigating += Navigated_EventHandler;
            

            // 네이티브에 웹뷰를 연결하여 스크립트 사용
            _WebBrowser.ObjectForScripting = this;

            // 초기 웹뷰 주소
            Uri source = new Uri("https://www.naver.com");
            //string source = @"file:///C:\Users\NHN\source\repos\WpfApp1\page1.html";
            _WebBrowser.Navigate(@$"{source}");

        }

     
        /** 네이티브(WPF) 환경에서 웹뷰(html)를 구현하고, 양방 통신 핸들링을 최종 목적으로 한다.
         *  {xaml, C#(이하 cs)} 와 {html, JavaScript(이하 js)}간 양방향 통신으로 함수를 구현해보는 실습
         *  네이티브 C#은 메소드, JavaScipt는 함수로 구분하겠음
         *  
         *  1. cs 함수 : cs 기반 wpf에서 실행되는 native 전용 함수
         *  2. js 함수 : js 기반 html에서 실행되는 웹뷰 전용 함수
         *  
         *  3. cs to js : 네이티브에서 웹뷰 함수 호출 및 매개변수 전달(단방향)
         *  4. js to cs : 웹뷰에서 네이티브 메소드 호출 및 매개변수 전달(단방향)
         *
         *  5. cs <-> js : 네이티브와 웹뷰 양방향 호출 및 매개변수 전달
        **/

        // 1. 네이티브에서만 실행되는 메소드
        // 1-1 뒤로가기
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_WebBrowser.CanGoBack)
            {
                _WebBrowser.GoBack();
            }
        }
        // 1-2 uri 주소 검색 : 네이티브 주소창에 입력된 uri로 이동
        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            _WebBrowser.Navigate(addressBar.Text);
        }

        // 1-3 웹뷰 창에 Html(js)를 새로 호출한다.
        private void JavaScriptButton_Click(object sender, RoutedEventArgs e)
        {
            string source = @"file:///C:\Users\NHN\source\repos\WpfApp1\page1.html";

            _WebBrowser.Navigate(@$"{source}");
        }

        // 3. JavaScript 스크립트를 호출하는 네이티브 메소드
        // 3-1 웹뷰에 현재 시간을 띄운다.
        private void WhatTime_Click(object sender, RoutedEventArgs e)
        {
            if (_WebBrowser.Source.ToString() == "file:///C:/Users/NHN/source/repos/WpfApp1/page1.html")
            {
                _ = _WebBrowser.InvokeScript("WhatTimeIsItNow");
                NativeTextBox3.Text = "";
            }
            else
            {
                NativeTextBox3.Text = "There's no HTML.";
            }
        }

        // 4.웹뷰(js)에서 네이티브 함수 호출
        // 4-1 웹뷰에서 메시지 전달
        public void MessageFromJavaScript(string msg)
        {
            _ = MessageBox.Show($"Native Message : {msg}");
        }
        
        // 4-2 웹뷰에서 계산 결과 전달
        public void ResultFromJavaScript(int res)
        {
            _WebBrowser.Navigating += Navigated_EventHandler;
        }

        public void UriChangedFromJavaScript()
        {
            _WebBrowser.Navigating += Navigated_EventHandler;
        }

        // 5. 양방향 통신 (C# -> js -> C#)
        // 5-1 C#의 인풋을 JavaScript가 전달받고 계산한다. 해당 결과를 다시 C# 메시지로 돌려준다.
        private void TransferDataToWebviewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int[] inputArray = new int[] {
                Cal_ComboBox.SelectedIndex,
                int.Parse(NativeTextBox1.Text),
                int.Parse(NativeTextBox2.Text) };

                // WebBrowser 컨트롤에 연결된 JavaScript의 함수를 호출
                _ = _WebBrowser.InvokeScript("CalculatorByJavaScript", inputArray[0], inputArray[1], inputArray[2]);
            }
            catch (COMException)
            {
                _ = MessageBox.Show("Html 스크립트를 먼저 호출하세요.");
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("Native Input을 정수로 채워야 합니다.");
            }
            catch (OverflowException)
            {
                _ = MessageBox.Show("정수형 범위가 Int32를 초과합니다.");
            }
        }

        private void UriButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        // Event Handler
        void MainWindow1_Closing(object sender, EventArgs e)
        {
            MessageBox.Show("창을 닫습니다.", "WebBrowser Closing");
        }

        void Navigated_EventHandler(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            //MessageBox.Show($"{e.Uri}");

            string uri = e.Uri.ToString();

            string[] words = uri.Split("?");
            string[] word = words[0].Split(":");

            string protocol = word[0];
            string host = word[1];

            string cmd = words[1].Substring(3);     // op=add
            string param1 = words[2].Substring(7);  // param1=1
            string param2 = words[3].Substring(7);  // param2=2

            _WebBrowser.Navigating -= Navigated_EventHandler;

            switch (protocol)
            {
                case "calc":
                    _WebBrowser.Navigate("file:///C:/Users/NHN/source/repos/WpfApp1/page1.html");

                    if(cmd == "Add") { Cal_ComboBox.SelectedIndex = 0;}
                    else if(cmd == "Sub") { Cal_ComboBox.SelectedIndex = 1;}
                    else if(cmd == "Mul") { Cal_ComboBox.SelectedIndex = 2;}
                    else if(cmd == "Div") { Cal_ComboBox.SelectedIndex = 3;}

                    NativeTextBox1.Text = param1;
                    NativeTextBox2.Text = param2;
                    
                    break;
                case "http":
                case "https":
                    _WebBrowser.Navigate("https://" + host);
                    break;
                default:
                    break;
            }

        }
    }
}

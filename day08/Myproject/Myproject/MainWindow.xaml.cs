using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RestaurantMap
{
    public partial class MainWindow : MetroWindow
    {
        private const string ApiKey = "4554b280f57cb01f46eab538be2cb2d0";
        private const string ApiUrl = "https://apis.data.go.kr/6260000/FoodService/getFoodKr?serviceKey=o3%2BwGQ3X6KrRXJ6R7zhAjv3yc%2FSz7Cn4SEMv4PeV18Aboru7JKB28OzCPjMW91H89tc4z3OxGumWtg2Jk%2BcSzg%3D%3D&pageNo=1&numOfRows=10&resultType=json";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task LoadRestaurantsAsync()
        {
            try
            {
                string encoding_foodName = HttpUtility.UrlEncode("", System.Text.Encoding.UTF8);
                string openApiUri = $"{ApiUrl}&language=ko-KR&page=1&include_adult=false&query={encoding_foodName}";

                string result = string.Empty;
                WebRequest req = WebRequest.Create(openApiUri);
                WebResponse res = await req.GetResponseAsync();
                StreamReader reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();
                reader.Close();
                res.Close();

                Debug.WriteLine(result);

                var jsonResult = JObject.Parse(result);
                var itemsArray = jsonResult["getFoodKr"]["item"]; // 실제 데이터의 키로 수정 필요

                var foodItems = new List<FoodItem>();
                foreach (var item in itemsArray)
                {
                    var foodItem = new FoodItem()
                    {
                        Id = Int32.Parse(item["UC_SEQ"].ToString()),
                        Name = item["MAIN_TITLE"].ToString(),
                        ADDR1 = item["ADDR1"].ToString(),
                        대표메뉴 = item["RPRSNTV_MENU"].ToString(),
                        메뉴소개 = item["ITEMCNTNTS"].ToString(),
                        LAT = Double.Parse(item["LAT"].ToString()), // 위도 값을 double로 변환하여 할당
                        LNG = Double.Parse(item["LNG"].ToString()) // 경도 값을 double로 변환하여 할당
                    };

                    foodItems.Add(foodItem);
                }

                // 데이터 그리드 뷰에 데이터 바인딩
                dataGridView.ItemsSource = foodItems;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
                MessageBox.Show("API 요청 중 오류가 발생했습니다: " + ex.Message);
            }
        }

        private void ShowMap_Click(object sender, RoutedEventArgs e)
        {
            // 데이터 그리드 뷰에서 선택된 항목 가져오기
            if (dataGridView.SelectedItem != null)
            {
                FoodItem selectedFood = dataGridView.SelectedItem as FoodItem;
                double latitude = selectedFood.LAT; // 위도 값 가져오기
                double longitude = selectedFood.LNG; // 경도 값 가져오기

                // MapWindow 인스턴스 생성 및 ShowDialog 호출
                MapWindow mapWindow = new MapWindow(latitude, longitude);
                mapWindow.ShowDialog();
            }
        }


        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRestaurantsAsync();
        }

    }

    public class FoodItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ADDR1 { get; set; }
        public string 대표메뉴 { get; set; }
        public string 메뉴소개 { get; set; }

        public double LAT { get; set; } // 위도를 나타내는 속성
        public double LNG { get; set; } // 경도를 나타내는 속성

    }
}

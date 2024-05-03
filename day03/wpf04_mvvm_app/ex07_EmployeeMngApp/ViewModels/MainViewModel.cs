using Caliburn.Micro;
using ex07_EmployeeMngApp.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ex07_EmployeeMngApp.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        // 멤버변수 (소문자) 이걸 속성으로 쓰면 대문자
        private int id;
        private string empName;
        private decimal salary;
        private string deptName;
        private string addr;

        // List는 정적인 컬렉션, BindableCollection은 동적인 컬렉션
        // MVVM처럼 List를 사용 못함
        private BindableCollection<Employees> listEmployees;

        private Employees selectedEmployee;

        public Employees SelectedEmployee
        {
            get => selectedEmployee;
            set
            {
                selectedEmployee = value;
                // 데이터를 TextBox들에 전달

                if (selectedEmployee != null)
                {
                    Id = value.Id;
                    EmpName = value.EmpName;
                    Salary = value.Salary;
                    DeptName = value.DeptName;
                    Addr = value.Addr;

                    NotifyOfPropertyChange(() => SelectedEmployee); // View에 데이터가 표시될려면 필수
                    NotifyOfPropertyChange(() => Id);
                    NotifyOfPropertyChange(() => EmpName);
                    NotifyOfPropertyChange(() => Salary);
                    NotifyOfPropertyChange(() => DeptName);
                    NotifyOfPropertyChange(() => Addr);
                }
            }
        }

        // 속성 
        public int Id
        {
            get => id;
            set
            {
                id = value;
                NotifyOfPropertyChange(() => id);
                NotifyOfPropertyChange(() => CanDelEmployee); // 삭제여부 속성도 변경했다.
            }
        }

        public string EmpName
        {
            get { return empName; }
            set
            {
                empName = value;
                NotifyOfPropertyChange(() => EmpName);
                NotifyOfPropertyChange(() => CanSaveEmployee);
            }
        }

        public decimal Salary
        {
            get { return salary; }
            set
            {
                salary = value;
                NotifyOfPropertyChange(() => Salary);
                NotifyOfPropertyChange(() => CanSaveEmployee);
            }
        }

        public string DeptName
        {
            get { return deptName; }
            set
            {
                deptName = value;
                NotifyOfPropertyChange(() => DeptName);
                NotifyOfPropertyChange(() => CanSaveEmployee);
            }
        }

        // 저장버튼 활성화 여부 속성
        public bool CanSaveEmployee
        {
            get
            {
                if (string.IsNullOrEmpty(EmpName) || Salary == 0 || string.IsNullOrEmpty(DeptName))
                    return false;
                else
                    return true;
            }
        }
        public string Addr
        {
            get { return addr; }
            set
            {
                addr = value;
                NotifyOfPropertyChange(() => Addr);
            }
        }


        // DataGrid에 뿌릴 Employees 테이블 데이터
        public BindableCollection<Employees> ListEmployees
        {
            get => listEmployees;
            set
            {
                listEmployees = value;
                // 값이 변경된 것을 시스템에 알려즘
                NotifyOfPropertyChange(() => listEmployees); // 필수
            }
        }

        public MainViewModel()
        {
            DisplayName = "직원관리 시스템";

            // 조회 실행
            GetEmployees();
        }


        /// <summary>
        /// Caliburn.Micro가 Xaml의 버튼 x:Name과 동일한 메서드로 매핑
        /// </summary>
        public void SaveEmployee()
        {
            //MessageBox.Show("저장버튼 동작");
            using (SqlConnection conn = new SqlConnection(Helpers.Common.CONNSTRING))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                if (Id == 0) // INSERT
                    cmd.CommandText = Models.Employees.INSERT_QUERY;
                else // UPDATE
                    cmd.CommandText = Models.Employees.UPDATE_QUERY;

                SqlParameter prmEmpName = new SqlParameter("@EmpName", EmpName);
                cmd.Parameters.Add(prmEmpName);
                SqlParameter prmSalary = new SqlParameter("@Salary", Salary);
                cmd.Parameters.Add(prmSalary);
                SqlParameter prmDeptName = new SqlParameter("@DeptName", DeptName);
                cmd.Parameters.Add(prmDeptName);
                SqlParameter prmAddr = new SqlParameter("@Addr", Addr ?? (object)DBNull.Value); // 주소가 빈값일때 컬럼에 null값을 입력
                cmd.Parameters.Add(prmAddr);

                if (Id != 1) // 업데이트면 Id 파라미터가 필요
                {
                    SqlParameter prmId = new SqlParameter("@Id", Id);
                    cmd.Parameters.Add(prmId);
                }

                var result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("저장성공!");
                }
                else
                {
                    MessageBox.Show("저장실패!");
                }
                GetEmployees(); // 그리드 재조회
                NewEmployee();  // 모든 입력컨트롤 초기화
            }
        }

        public void GetEmployees()
        {
            using (SqlConnection conn = new SqlConnection(Helpers.Common.CONNSTRING))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Models.Employees.SELECT_QUERY, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                ListEmployees = new BindableCollection<Employees>();

                while (reader.Read())
                {
                    ListEmployees.Add(new Employees()
                    {
                        Id = (int)reader["Id"],
                        EmpName = reader["EmpName"].ToString(),
                        Salary = (decimal)reader["Salary"],
                        DeptName = reader["DeptName"].ToString(),
                        Addr = reader["Addr"].ToString()
                    });
                }
            }
        }

        // 삭제 버튼을 누를 수 있는지 여부를 확인하는 구문
        public bool CanDelEmployee
        {
            get { return Id != 0; } // TextBox Id 속성의 값이 0이면 false, 0이 아니면 True
        }

        public void DelEmployee()
        {
            if (Id == 0)
            {
                MessageBox.Show("삭제불가!");
                return;
            }

            if (MessageBox.Show("삭제하시겠습니까?","삭제여부", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(Helpers.Common.CONNSTRING))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(Models.Employees.DELETE_QUERY, conn);
                SqlParameter prmId = new SqlParameter("@Id", Id);
                cmd.Parameters.Add(prmId);

                var res = cmd.ExecuteNonQuery();
                if (res >= 0)
                {
                    MessageBox.Show("삭제 성공!");
                }
                else
                {
                    MessageBox.Show("삭제 실패!");
                }

                GetEmployees();
                NewEmployee();  // 입력값 초기화
            }
        }

        public void NewEmployee()
        {
            Id = 0;
            Salary = 0;
            EmpName = DeptName = Addr = "";

        }
    }
}
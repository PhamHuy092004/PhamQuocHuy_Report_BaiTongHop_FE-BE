using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PhamQuocHuy_Report_BaiTongHop_FE_BE.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người dùng.")]
        [StringLength(30, ErrorMessage = "Tên tài khoản không được quá dài")]
        [MinLength(6, ErrorMessage = "Tên tài khoản vui lòng hơn 6 ký tự")]
        public string username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$", ErrorMessage = "Mật khẩu phải dài hơn 6 ký tự, có số, ký tự đặc biệt, và chữ in hoa.")]
        public string password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string hoVaTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^(0[139785])\d{8}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string soDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
        public string gioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh.")]
        public DateTime ngaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố.")]
        public string tinhThanh { get; set; }
        public string repass { get; set; }
    }
    public class Jwt
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string Audience { get; set; }
    }
    public class UserLoginDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

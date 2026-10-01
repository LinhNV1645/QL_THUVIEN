using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Web.Controllers;

[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorController : Controller
{
    [Route("Error/{code:int}")]
    public IActionResult Index(int code)
    {
        var (tieuDe, moTa) = code switch
        {
            403 => ("Không có quyền truy cập", "Tài khoản của bạn không được phép xem trang này."),
            404 => ("Không tìm thấy trang", "Trang hoặc dữ liệu bạn yêu cầu không tồn tại."),
            500 => ("Lỗi hệ thống", "Đã có lỗi xảy ra trong quá trình xử lý. Vui lòng thử lại sau."),
            _   => ("Có lỗi xảy ra", "Yêu cầu không thể xử lý.")
        };
        ViewBag.Code   = code;
        ViewBag.TieuDe = tieuDe;
        ViewBag.MoTa   = moTa;
        Response.StatusCode = code;
        return View("~/Views/Shared/StatusCode.cshtml");
    }
}
